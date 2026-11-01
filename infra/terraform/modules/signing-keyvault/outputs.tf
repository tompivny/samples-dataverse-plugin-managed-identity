output "id" {
  description = "Resource ID of the Key Vault."
  value       = azurerm_key_vault.this.id
}

output "vault_uri" {
  description = "Key Vault URI, as used by signing tools."
  value       = azurerm_key_vault.this.vault_uri
}

output "certificate_name" {
  description = "Name of the signing certificate."
  value       = azurerm_key_vault_certificate.signing.name
}

output "certificate_thumbprint" {
  description = "SHA-1 thumbprint of the signing certificate."
  value       = azurerm_key_vault_certificate.signing.thumbprint
}

output "certificate_data_base64" {
  description = "Public part of the certificate (DER, base64). Hash it to build the federated credential subject."
  value       = azurerm_key_vault_certificate.signing.certificate_data_base64
}
