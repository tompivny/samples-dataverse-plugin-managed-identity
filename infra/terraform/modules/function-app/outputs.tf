output "name" {
  description = "Function app name. Use it as the deployment target."
  value       = azurerm_function_app_flex_consumption.this.name
}

output "default_hostname" {
  description = "Host name the plug-in calls over HTTPS."
  value       = azurerm_function_app_flex_consumption.this.default_hostname
}

output "api_client_id" {
  description = "Client ID of the function's Entra app registration."
  value       = azuread_application.api.client_id
}

output "token_scope" {
  description = "Scope the plug-in passes to IManagedIdentityService.AcquireToken."
  value       = "${azuread_application_identifier_uri.api.identifier_uri}/.default"
}
