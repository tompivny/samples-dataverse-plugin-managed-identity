variable "name" {
  description = "Key Vault name. Globally unique, 3-24 characters, letters, digits and hyphens."
  type        = string
}

variable "location" {
  description = "Azure region."
  type        = string
}

variable "resource_group_name" {
  description = "Resource group that holds the Key Vault."
  type        = string
}

variable "certificate_name" {
  description = "Name of the code-signing certificate in Key Vault."
  type        = string
  default     = "dataverse-plugin-signing"
}

variable "certificate_subject" {
  description = "Subject DN of the self-signed certificate."
  type        = string
  default     = "CN=Samples Dataverse Plugin Signing (DEV)"
}

variable "extended_key_usage" {
  description = "Extended key usage OIDs. 1.3.6.1.5.5.7.3.3 is code signing; Microsoft's own example uses 1.3.6.1.5.5.7.3.4 (email protection), so both are set by default."
  type        = list(string)
  default     = ["1.3.6.1.5.5.7.3.3", "1.3.6.1.5.5.7.3.4"]
}

variable "validity_in_months" {
  description = "Certificate validity in months."
  type        = number
  default     = 36
}

variable "signer_principal_ids" {
  description = "Object IDs of principals (for example the CI service principal) allowed to sign with the key."
  type        = list(string)
  default     = []
}

variable "purge_protection_enabled" {
  description = "Enable purge protection. Keep true outside throwaway environments: it blocks deleting the vault name for the retention period."
  type        = bool
  default     = true
}

variable "soft_delete_retention_days" {
  description = "Soft-delete retention in days (7-90)."
  type        = number
  default     = 7
}

variable "tags" {
  description = "Tags applied to the Key Vault."
  type        = map(string)
  default     = {}
}
