output "managed_identity_client_id" {
  description = "Client ID for the Dataverse managedidentity record (applicationid)."
  value       = module.managed_identity.client_id
}

output "managed_identity_principal_id" {
  description = "Object ID of the identity, for Azure role assignments on the resources the plug-in calls."
  value       = module.managed_identity.principal_id
}

output "tenant_id" {
  description = "Tenant ID for the Dataverse managedidentity record (tenantid)."
  value       = module.managed_identity.tenant_id
}

output "function_app_name" {
  description = "Function app to deploy the sample code to."
  value       = module.function_app.name
}

output "function_app_hostname" {
  description = "Host name the plug-in calls."
  value       = module.function_app.default_hostname
}

output "function_token_scope" {
  description = "Scope the plug-in passes to AcquireToken."
  value       = module.function_app.token_scope
}

output "key_vault_uri" {
  description = "Key Vault URI used by the signing step."
  value       = module.signing_keyvault.vault_uri
}

output "signing_certificate_name" {
  description = "Name of the HSM-backed signing certificate."
  value       = module.signing_keyvault.certificate_name
}

output "signing_certificate_data_base64" {
  description = "Public certificate (DER, base64). Hash it to compute the federated credential subject."
  value       = module.signing_keyvault.certificate_data_base64
}
