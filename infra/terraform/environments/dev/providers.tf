terraform {
  required_version = ">= 1.6.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
  }

  # Partial configuration: supply the values with
  #   terraform init -backend-config=backend.hcl
  # (copy backend.hcl.example to backend.hcl).
  backend "azurerm" {}
}

provider "azurerm" {
  subscription_id = var.subscription_id

  features {
    key_vault {
      purge_soft_delete_on_destroy = false
    }
  }

  # Authenticate against the data plane with Entra ID, not storage keys.
  storage_use_azuread = true
}
