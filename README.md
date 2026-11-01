# Dataverse plug-in managed identity: ALM sample

This sample shows how to handle the application lifecycle (ALM) of a managed identity
for Dataverse plug-ins. It covers signing, packaging, Azure infrastructure, and one
detail that breaks most pipelines: the managed identity's `applicationid` and `tenantid`
differ in each environment, but a managed solution carries one fixed value.

It complements the blog article
[ALM of managed identities for Dataverse plug-ins](https://www.tompivny.com/posts/alm-of-managed-identities-for-dataverse-plug-ins/).
The article explains the reasoning. This repo holds the working code.

```
Key Vault certificate signs the plug-in assembly and package
  -> solution (managed identity record + binding to the plug-in)
  -> Package Deployer package
  -> Package Deployer patches applicationid/tenantid from a runtime setting
  -> plug-in calls the Azure Function with its managed identity token
```

## Projects

All projects are in `src/`, with the prefix `Samples.Dataverse.ManagedIdentity.`.

| Project | Purpose |
| --- | --- |
| `Build` | NuGet package with MSBuild targets. The targets sign files with a Key Vault certificate. |
| `Plugins.PluginAssemblyDemo` | Sample plug-in in a signed assembly. |
| `Plugins.PluginPackageDemo` | The same sample plug-in in a signed plug-in package. |
| `Solution` | Dataverse solution. It declares the `managedidentity` record and binds it to the assembly and the package. |
| `Package` | Package Deployer package. `Deployment/` has the code that patches the managed identity before the import. |
| `AzureFunction` | Sample function (`whoami`) that the plug-in calls with the managed identity. |

## Infrastructure

Terraform creates a user-assigned managed identity with a federated credential, a
Premium Key Vault with a self-signed signing certificate, and the Azure Function app.
See [infra/terraform/README.md](infra/terraform/README.md).

## Prerequisites

- .NET SDK 10 or newer.
- The `pac` CLI, signed in to the target environment.
- A Dataverse environment that supports managed identity for plug-ins.
- A code-signing certificate in Azure Key Vault (the Terraform code creates one).

## Build

1. Pack the build targets. The local NuGet feed (`artifacts/packages`) serves them.

   ```bash
   dotnet pack src/Samples.Dataverse.ManagedIdentity.Build
   ```

2. Build the package with signing. Sign in with `az login` first.

   ```bash
   export SIGNING_KEYVAULT_URL=https://<vault-name>.vault.azure.net
   export SIGNING_CERTIFICATE_NAME=<certificate-name>
   dotnet build src/Samples.Dataverse.ManagedIdentity.Package -c Release
   ```

   Without a Key Vault, build unsigned: add `-p:SignPlugins=false`. The result
   works as a build test, but Dataverse needs signed plug-ins for managed identity.

The output is `src/Samples.Dataverse.ManagedIdentity.Package/bin/Release/*.pdpkg.zip`.

## Deploy

The solution contains placeholder values for `applicationid` and `tenantid`.
Pass the real values for each environment as runtime settings:

```bash
pac package deploy --package <path-to>.pdpkg.zip \
  --settings "ManagedIdentity:smp-dataverse-plugin-uami:ApplicationId=<guid>|ManagedIdentity:smp-dataverse-plugin-uami:TenantId=<guid>"
```

The package patches the solution before the first import. If a setting is wrong,
the deployment stops and changes nothing. The code is in
`src/Samples.Dataverse.ManagedIdentity.Package/Deployment`.

## Not included

- Provisioning of the Dataverse environment.
- Publishing the Azure Function code. Terraform only creates the app.
- Real identity values. The committed `applicationid` and `tenantid` are placeholders.
- A build or deploy pipeline, for example GitHub Actions or Azure DevOps.

## License

[MIT](LICENSE)
