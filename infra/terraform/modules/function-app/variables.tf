variable "function_app_name" {
  description = "Function app name. Globally unique; also used as the Entra app registration display name."
  type        = string
}

variable "storage_account_name" {
  description = "Storage account for the function host and deployment package. Globally unique, 3-24 lowercase letters and digits."
  type        = string
}

variable "location" {
  description = "Azure region. It must support Flex Consumption with .NET 10."
  type        = string
}

variable "resource_group_name" {
  description = "Resource group that holds the function app."
  type        = string
}

variable "allowed_client_application_ids" {
  description = "Client IDs of the identities allowed to call the function (the plug-in's managed identity)."
  type        = list(string)
}

variable "maximum_instance_count" {
  description = "Upper bound on scale-out. Keep it low for a demo."
  type        = number
  default     = 40
}

variable "tags" {
  description = "Tags applied to all resources."
  type        = map(string)
  default     = {}
}
