terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
  }
}

data "azurerm_client_config" "current" {}
data "azuread_client_config" "current" {}

# --- Entra app registration that represents the function's API ---------------
# The plug-in requests a token for this app; the function app validates it.

resource "azuread_application" "api" {
  display_name = var.function_app_name
  owners       = [data.azuread_client_config.current.object_id]

  api {
    # Managed identity tokens default to v1; the v2 endpoint configured on the
    # function app expects v2 tokens, so ask for them explicitly.
    requested_access_token_version = 2
  }
}

resource "azuread_application_identifier_uri" "api" {
  application_id = azuread_application.api.id
  identifier_uri = "api://${azuread_application.api.client_id}"
}

resource "azuread_service_principal" "api" {
  client_id = azuread_application.api.client_id
  owners    = [data.azuread_client_config.current.object_id]
}

# --- Flex Consumption function app (.NET 10 isolated) ------------------------
# .NET 10 on Linux is only supported on the Flex Consumption plan.

resource "azurerm_storage_account" "this" {
  name                     = var.storage_account_name
  location                 = var.location
  resource_group_name      = var.resource_group_name
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"
  tags                     = var.tags
}

resource "azurerm_storage_container" "deployment" {
  name                  = "app-package"
  storage_account_id    = azurerm_storage_account.this.id
  container_access_type = "private"
}

resource "azurerm_service_plan" "this" {
  name                = "asp-${var.function_app_name}"
  location            = var.location
  resource_group_name = var.resource_group_name
  os_type             = "Linux"
  sku_name            = "FC1"
  tags                = var.tags
}

resource "azurerm_function_app_flex_consumption" "this" {
  name                = var.function_app_name
  location            = var.location
  resource_group_name = var.resource_group_name
  service_plan_id     = azurerm_service_plan.this.id

  storage_container_type      = "blobContainer"
  storage_container_endpoint  = "${azurerm_storage_account.this.primary_blob_endpoint}${azurerm_storage_container.deployment.name}"
  storage_authentication_type = "StorageAccountConnectionString"
  storage_access_key          = azurerm_storage_account.this.primary_access_key

  runtime_name           = "dotnet-isolated"
  runtime_version        = "10.0"
  maximum_instance_count = var.maximum_instance_count
  instance_memory_in_mb  = 2048

  https_only = true
  tags       = var.tags

  site_config {}

  # Built-in authentication (Easy Auth): reject every request that does not
  # carry an Entra token for this API issued to an allowed client application.
  auth_settings_v2 {
    auth_enabled           = true
    require_authentication = true
    unauthenticated_action = "Return401"
    default_provider       = "azureactivedirectory"

    active_directory_v2 {
      client_id            = azuread_application.api.client_id
      tenant_auth_endpoint = "https://login.microsoftonline.com/${data.azurerm_client_config.current.tenant_id}/v2.0/"
      allowed_audiences    = [azuread_application_identifier_uri.api.identifier_uri]

      # The managed identity's client ID shows up as the caller (azp/appid).
      jwt_allowed_client_applications = var.allowed_client_application_ids
    }

    login {}
  }

  depends_on = [azuread_application_identifier_uri.api]
}
