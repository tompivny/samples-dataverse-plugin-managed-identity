terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }
}

data "azurerm_client_config" "current" {}

# Premium SKU is what makes HSM-backed keys available (RSA-HSM).
resource "azurerm_key_vault" "this" {
  name                = var.name
  location            = var.location
  resource_group_name = var.resource_group_name
  tenant_id           = data.azurerm_client_config.current.tenant_id
  sku_name            = "premium"

  rbac_authorization_enabled = true
  purge_protection_enabled   = var.purge_protection_enabled
  soft_delete_retention_days = var.soft_delete_retention_days

  tags = var.tags
}

# Whoever runs Terraform needs data-plane rights to create the certificate.
resource "azurerm_role_assignment" "deployer" {
  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Administrator"
  principal_id         = data.azurerm_client_config.current.object_id
}

# Principals that sign in CI: read the certificate and use the key to sign.
resource "azurerm_role_assignment" "signer_crypto" {
  for_each = toset(var.signer_principal_ids)

  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Crypto User"
  principal_id         = each.value
}

resource "azurerm_role_assignment" "signer_certificate" {
  for_each = toset(var.signer_principal_ids)

  scope                = azurerm_key_vault.this.id
  role_definition_name = "Key Vault Certificate User"
  principal_id         = each.value
}

resource "azurerm_key_vault_certificate" "signing" {
  name         = var.certificate_name
  key_vault_id = azurerm_key_vault.this.id

  certificate_policy {
    issuer_parameters {
      name = "Self"
    }

    # The private key is generated inside the HSM and can never be exported.
    key_properties {
      exportable = false
      key_size   = 2048
      key_type   = "RSA-HSM"
      reuse_key  = false
    }

    # Deliberately no lifetime_action: for a self-signed certificate the
    # federated credential subject is a hash of the certificate itself, so an
    # automatic renewal would silently break managed identity authentication.
    # Rotate by hand: add a second federated credential first.

    secret_properties {
      content_type = "application/x-pkcs12"
    }

    x509_certificate_properties {
      extended_key_usage = var.extended_key_usage
      key_usage          = ["digitalSignature"]
      subject            = var.certificate_subject
      validity_in_months = var.validity_in_months
    }
  }

  depends_on = [azurerm_role_assignment.deployer]
}
