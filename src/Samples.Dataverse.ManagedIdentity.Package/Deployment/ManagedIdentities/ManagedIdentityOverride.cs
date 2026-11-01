using System;
using System.Collections.Generic;
using System.Linq;
using Samples.Dataverse.ManagedIdentity.Package.Deployment.Settings;

namespace Samples.Dataverse.ManagedIdentity.Package.Deployment.ManagedIdentities
{
    /// <summary>
    /// The per-environment values for one managed identity. A property without a value keeps the
    /// value that is committed in the solution.
    /// </summary>
    public sealed class ManagedIdentityOverride
    {
        /// <summary>First segment of every runtime setting that this feature reads.</summary>
        public const string SettingPrefix = "ManagedIdentity";

        /// <summary>
        /// The properties that can be set. The key is the setting property, the value is the element of
        /// the <c>managedidentity</c> record in <c>customizations.xml</c>. All of them hold a GUID.
        /// To support another property, add it here.
        /// </summary>
        private static readonly IReadOnlyDictionary<string, string> GuidProperties =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["ApplicationId"] = "applicationid",
                ["TenantId"] = "tenantid",
            };

        private readonly Dictionary<string, Guid> _values = new Dictionary<string, Guid>(StringComparer.Ordinal);

        public ManagedIdentityOverride(string name)
        {
            Name = name;
        }

        /// <summary>The <c>name</c> of the managed identity record that the values apply to.</summary>
        public string Name { get; }

        /// <summary>The values to set, by the name of the element in <c>customizations.xml</c>.</summary>
        public IReadOnlyDictionary<string, Guid> Values => _values;

        /// <summary>
        /// Groups the <c>ManagedIdentity</c> runtime settings by identity name. An unknown property or a
        /// value that is not a GUID is added to <paramref name="errors"/>. A typo must fail the
        /// deployment, because otherwise the committed (non-production) identity is deployed without a
        /// warning.
        /// </summary>
        public static IReadOnlyList<ManagedIdentityOverride> Parse(
            IEnumerable<KeyValuePair<string, object>> runtimeSettings,
            ICollection<string> errors)
        {
            var overrides = new Dictionary<string, ManagedIdentityOverride>(StringComparer.OrdinalIgnoreCase);

            foreach (var setting in RuntimeSettingParser.Parse(SettingPrefix, runtimeSettings, errors))
            {
                if (!GuidProperties.TryGetValue(setting.Property, out var element))
                {
                    errors.Add($"Runtime setting '{setting.Key}' has the unknown property '{setting.Property}'. Use one of: {string.Join(", ", GuidProperties.Keys)}.");
                    continue;
                }

                if (!Guid.TryParse(setting.Value, out var value))
                {
                    errors.Add($"Runtime setting '{setting.Key}' has the value '{setting.Value}', which is not a GUID.");
                    continue;
                }

                if (!overrides.TryGetValue(setting.Name, out var entry))
                {
                    entry = new ManagedIdentityOverride(setting.Name);
                    overrides.Add(setting.Name, entry);
                }

                entry._values[element] = value;
            }

            return overrides.Values.Where(o => o.Values.Count > 0).ToList();
        }
    }
}
