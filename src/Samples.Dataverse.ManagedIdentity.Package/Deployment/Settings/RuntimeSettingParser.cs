using System;
using System.Collections.Generic;

namespace Samples.Dataverse.ManagedIdentity.Package.Deployment.Settings
{
    /// <summary>
    /// Reads the runtime settings of one feature. Every key of a feature starts with the same prefix
    /// and has three segments separated by colons: <c>&lt;Prefix&gt;:&lt;Name&gt;:&lt;Property&gt;</c>.
    /// The parser has no knowledge of any feature, so other features can reuse it.
    /// </summary>
    public static class RuntimeSettingParser
    {
        private const char Separator = ':';

        /// <summary>
        /// Returns the settings whose key starts with <paramref name="prefix"/> and ignores all others.
        /// A key with the prefix but a wrong shape is added to <paramref name="errors"/>. Segments are
        /// compared without regard to case by the callers, so the parser keeps them as written.
        /// </summary>
        public static IReadOnlyList<RuntimeSetting> Parse(
            string prefix,
            IEnumerable<KeyValuePair<string, object>> runtimeSettings,
            ICollection<string> errors)
        {
            var result = new List<RuntimeSetting>();

            foreach (var setting in runtimeSettings)
            {
                var segments = setting.Key.Split(Separator);

                if (!segments[0].Equals(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var name = segments.Length == 3 ? segments[1].Trim() : string.Empty;
                var property = segments.Length == 3 ? segments[2].Trim() : string.Empty;

                if (name.Length == 0 || property.Length == 0)
                {
                    errors.Add($"Runtime setting '{setting.Key}' must have the form '{prefix}:<name>:<property>'.");
                    continue;
                }

                result.Add(new RuntimeSetting(setting.Key, name, property, Convert.ToString(setting.Value)?.Trim()));
            }

            return result;
        }
    }
}
