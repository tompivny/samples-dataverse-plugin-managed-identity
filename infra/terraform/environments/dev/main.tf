locals {
  suffix = "${var.workload}-${var.environment}"

  tags = merge(
    {
      workload    = var.workload
      environment = var.environment
      managed-by  = "terraform"
    },
    var.tags,
  )
}

resource "azurerm_resource_group" "this" {
  name     = "rg-${local.suffix}"
  location = var.location
  tags     = local.tags
}

module "managed_identity" {
  source = "../../modules/managed-identity"

  name                         = "id-${local.suffix}"
  location                     = azurerm_resource_group.this.location
  resource_group_name          = azurerm_resource_group.this.name
  dataverse_environment_id        = var.dataverse_environment_id
  signing_certificate_data_base64 = module.signing_keyvault.certificate_data_base64
  tags                            = local.tags
}

module "function_app" {
  source = "../../modules/function-app"

  function_app_name              = var.function_app_name
  storage_account_name           = var.function_storage_account_name
  location                       = azurerm_resource_group.this.location
  resource_group_name            = azurerm_resource_group.this.name
  allowed_client_application_ids = [module.managed_identity.client_id]
  tags                           = local.tags
}

module "signing_keyvault" {
  source = "../../modules/signing-keyvault"

  name                     = var.key_vault_name
  location                 = azurerm_resource_group.this.location
  resource_group_name      = azurerm_resource_group.this.name
  signer_principal_ids     = var.signer_principal_ids
  purge_protection_enabled = var.purge_protection_enabled
  tags                     = local.tags
}
