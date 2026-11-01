output "id" {
  description = "Resource ID of the managed identity."
  value       = azurerm_user_assigned_identity.this.id
}

output "client_id" {
  description = "Application (client) ID. Goes into the Dataverse managedidentity record as applicationid."
  value       = azurerm_user_assigned_identity.this.client_id
}

output "principal_id" {
  description = "Object ID of the identity's service principal. Use it for Azure role assignments."
  value       = azurerm_user_assigned_identity.this.principal_id
}

output "tenant_id" {
  description = "Tenant ID of the identity. Goes into the Dataverse managedidentity record as tenantid."
  value       = azurerm_user_assigned_identity.this.tenant_id
}
