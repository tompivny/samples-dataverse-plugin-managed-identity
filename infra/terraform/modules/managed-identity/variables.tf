variable "name" {
  description = "Name of the user-assigned managed identity."
  type        = string
}

variable "location" {
  description = "Azure region."
  type        = string
}

variable "resource_group_name" {
  description = "Resource group that holds the identity."
  type        = string
}

variable "federated_credential_name" {
  description = "Name of the federated identity credential."
  type        = string
  default     = "dataverse-plugin"
}

variable "dataverse_environment_id" {
  description = "ID of the Dataverse environment that runs the plug-in. Part of the federated credential subject."
  type        = string
}

variable "signing_certificate_data_base64" {
  description = "Public part of the signing certificate (DER, base64). Its SHA-256 hash is part of the federated credential subject."
  type        = string
}

variable "tags" {
  description = "Tags applied to the identity."
  type        = map(string)
  default     = {}
}
