terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    external = {
      source  = "hashicorp/external"
      version = "~> 2.3"
    }
  }
}

data "azurerm_client_config" "current" {}

resource "azurerm_user_assigned_identity" "this" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

# The subject of a version 2 federated credential contains the SHA-256 hash of
# the signing certificate. Terraform cannot hash binary data, so a script
# builds the subject from the public certificate. Terraform runs it during
# apply, after the certificate exists.
data "external" "plugin_subject" {
  program = ["pwsh", "-NoProfile", "-NonInteractive", "-File", "${path.module}/scripts/plugin-subject.ps1"]

  query = {
    tenant_id       = data.azurerm_client_config.current.tenant_id
    environment_id  = var.dataverse_environment_id
    certificate_b64 = var.signing_certificate_data_base64
  }
}

# The federated credential trusts tokens that Dataverse issues for the plug-in.
resource "azurerm_federated_identity_credential" "dataverse_plugin" {
  name                = var.federated_credential_name
  resource_group_name = var.resource_group_name
  parent_id           = azurerm_user_assigned_identity.this.id
  audience            = ["api://AzureADTokenExchange"]
  issuer              = "https://login.microsoftonline.com/${data.azurerm_client_config.current.tenant_id}/v2.0"
  subject             = data.external.plugin_subject.result.subject
}
