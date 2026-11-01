# Terraform

Infrastructure for the managed identity demo: a user-assigned managed identity,
and a Premium Key Vault holding an HSM-backed, self-signed code-signing certificate.

```
modules/
  managed-identity/   user-assigned identity + federated identity credential
  signing-keyvault/   Key Vault (Premium) + RSA-HSM self-signed certificate + RBAC
  function-app/       Flex Consumption function (.NET 10) + Entra app registration + Easy Auth
environments/
  dev/                root module: state, providers, wiring
```

## Prerequisites

- Terraform >= 1.6, PowerShell 7 (`pwsh`) and the Azure CLI, signed in (`az login`) to the target tenant.
- Rights to create resources and role assignments on the subscription
  (for example Contributor plus User Access Administrator, or Owner).
- A storage account and container for remote state.

## Deploy

```bash
cd environments/dev
cp backend.hcl.example backend.hcl        # fill in your state storage account
cp dev.tfvars.example dev.tfvars          # fill in subscription and Key Vault name
terraform init -backend-config=backend.hcl
terraform plan  -var-file=dev.tfvars
terraform apply -var-file=dev.tfvars
```

`backend.hcl` and `dev.tfvars` are git-ignored.

## Federated credential subject

The version 2 subject contains the SHA-256 hash of the signing certificate.
Terraform cannot hash binary data, so `modules/managed-identity/scripts/plugin-subject.ps1`
builds the subject. The `external` data source runs it during apply, after
Terraform creates the certificate. The inputs are the tenant ID, the
`dataverse_environment_id` variable and the public certificate from Key Vault.
One `terraform apply` creates everything. The script needs PowerShell 7 (`pwsh`),
which runs on Windows, Linux and macOS.

The subject follows
[Microsoft's setup guide](https://learn.microsoft.com/en-us/power-platform/admin/set-up-managed-identity).

Terraform does not manage the Dataverse side (the `managedidentity` record and its
binding to the plug-in assembly and package). The `Solution` project declares both.

## Sample function

The `function-app` module creates the Azure resource the plug-in calls with its
managed identity. The function app rejects every request without an Entra token
for its own app registration, issued to the managed identity's client ID. The
code is in `src/Samples.Dataverse.ManagedIdentity.AzureFunction` (a `whoami` endpoint
that echoes the caller). The plug-in requests a token with the
`function_token_scope` output as the scope and sends it as a bearer token.

- **Plan:** .NET 10 on Linux is only supported on the Flex Consumption plan, not
  classic Linux Consumption. Flex Consumption has a monthly free grant, but the
  storage account is billed, so "free" means cents, not zero.
- **Extra provider:** the module uses the `azuread` provider to create the app
  registration, so the identity running `apply` also needs rights to create app
  registrations (for example the Application Developer role).
- **Deploy the code:** Terraform only creates the app. Publish the code with
  `func azure functionapp publish <function_app_name>` or your pipeline.
- **Region:** pick one that supports Flex Consumption with .NET 10 (check with
  `az functionapp list-flexconsumption-runtimes`).

## Notes

- The signing key is created inside the HSM with `exportable = false`. Tools
  that need the private key as a file, such as `osslsigncode` with a PFX,
  cannot use it; signing has to go through Key Vault (for example Sign CLI or
  AzureSignTool, which run on Windows).
- The certificate has no auto-renew policy on purpose: its hash is part of the
  federated credential subject, so renewing it changes the subject.
- Purge protection is on by default. It prevents reusing a deleted vault's name
  until retention ends; set `purge_protection_enabled = false` in throwaway dev
  environments.
