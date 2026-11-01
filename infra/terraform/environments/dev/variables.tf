variable "subscription_id" {
  description = "Azure subscription to deploy into."
  type        = string
}

variable "location" {
  description = "Azure region."
  type        = string
  default     = "westeurope"
}

variable "workload" {
  description = "Short workload name used in resource names."
  type        = string
  default     = "dvplugin"
}

variable "environment" {
  description = "Environment name used in resource names."
  type        = string
  default     = "dev"
}

variable "key_vault_name" {
  description = "Globally unique Key Vault name (3-24 characters)."
  type        = string
}

variable "function_app_name" {
  description = "Globally unique function app name."
  type        = string
}

variable "function_storage_account_name" {
  description = "Globally unique storage account name for the function app (3-24 lowercase letters and digits)."
  type        = string
}

variable "signer_principal_ids" {
  description = "Object IDs of the CI principals allowed to sign with the HSM-backed key."
  type        = list(string)
  default     = []
}

variable "purge_protection_enabled" {
  description = "Enable Key Vault purge protection. Set false only for throwaway environments."
  type        = bool
  default     = true
}

variable "dataverse_environment_id" {
  description = "ID of the Dataverse environment that runs the plug-in. Terraform builds the federated credential subject from it."
  type        = string
}

variable "tags" {
  description = "Tags applied to all resources."
  type        = map(string)
  default     = {}
}
