using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Xml.Linq;
using Samples.Dataverse.ManagedIdentity.Package.Deployment.Logging;

namespace Samples.Dataverse.ManagedIdentity.Package.Deployment.ManagedIdentities
{
    /// <summary>
    /// Writes per-environment managed identity values into the solution zips of the extracted
    /// package, before Package Deployer imports them.
    /// <para>
    /// The result is still one managed layer. Nothing is changed in Dataverse after the import, so
    /// no unmanaged layer is created.
    /// </para>
    /// <para>
    /// A packed solution keeps its managed identities in <c>customizations.xml</c>, under
    /// <c>&lt;managedidentities&gt;&lt;managedidentity&gt;</c>. The patcher checks all settings and
    /// all solutions before it writes the first zip. If anything is wrong, every zip stays as it was.
    /// </para>
    /// </summary>
    public sealed class ManagedIdentityPatcher
    {
        private const string CustomizationsEntryName = "customizations.xml";

        private readonly ILogSink _log;

        public ManagedIdentityPatcher(ILogSink log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <param name="packageDataFolder">The folder that holds the solution zips of the extracted package.</param>
        /// <param name="runtimeSettings">The Package Deployer runtime settings. Null means none.</param>
        /// <exception cref="AggregateException">
        /// A setting is not valid, or it names a managed identity that is in none of the solutions.
        /// </exception>
        public void Patch(string packageDataFolder, IEnumerable<KeyValuePair<string, object>> runtimeSettings)
        {
            if (packageDataFolder == null) throw new ArgumentNullException(nameof(packageDataFolder));

            runtimeSettings = runtimeSettings ?? new Dictionary<string, object>();

            var errors = new List<string>();
            var overrides = ManagedIdentityOverride.Parse(runtimeSettings, errors);

            if (overrides.Count == 0)
            {
                ThrowIfAny(errors);
                _log.Info("No managed identity settings found. The values committed in the solutions are deployed.");
                return;
            }

            var matched = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var patched = new List<KeyValuePair<string, XDocument>>();

            foreach (var zipPath in Directory.GetFiles(packageDataFolder, "*.zip"))
            {
                var customizations = ReadCustomizations(zipPath);

                if (customizations != null && PatchSolution(customizations, Path.GetFileName(zipPath), overrides, matched, errors))
                {
                    patched.Add(new KeyValuePair<string, XDocument>(zipPath, customizations));
                }
            }

            foreach (var missing in overrides.Where(o => !matched.Contains(o.Name)))
            {
                errors.Add($"Managed identity '{missing.Name}' from the runtime settings is in none of the solutions in '{packageDataFolder}'.");
            }

            ThrowIfAny(errors);

            foreach (var solution in patched)
            {
                WriteCustomizations(solution.Key, solution.Value);
            }
        }

        private bool PatchSolution(
            XDocument customizations,
            string solutionFile,
            IReadOnlyList<ManagedIdentityOverride> overrides,
            ISet<string> matched,
            ICollection<string> errors)
        {
            var changed = false;

            foreach (var identity in customizations.Root.Elements("managedidentities").Elements("managedidentity"))
            {
                var name = identity.Element("name")?.Value;
                var entry = overrides.FirstOrDefault(o => string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));

                if (entry == null)
                {
                    continue;
                }

                matched.Add(entry.Name);

                foreach (var value in entry.Values)
                {
                    changed |= SetGuid(identity, value.Key, value.Value, entry.Name, solutionFile, errors);
                }
            }

            return changed;
        }

        private bool SetGuid(XElement identity, string elementName, Guid value, string identityName, string solutionFile, ICollection<string> errors)
        {
            var element = identity.Element(elementName);

            if (element == null)
            {
                // The import validates customizations.xml against a schema with a fixed element order.
                // So a missing element is an error. The code does not guess where to insert it.
                errors.Add($"Managed identity '{identityName}' in '{solutionFile}' has no <{elementName}> element to set.");
                return false;
            }

            var previous = element.Value;
            element.Value = value.ToString("B"); // Dataverse writes GUIDs in braces: {xxxxxxxx-...}
            _log.Info($"Managed identity '{identityName}' in '{solutionFile}': {elementName} {previous} -> {element.Value}.");
            return true;
        }

        private void ThrowIfAny(IReadOnlyCollection<string> errors)
        {
            if (errors.Count == 0)
            {
                return;
            }

            foreach (var error in errors)
            {
                _log.Error(error);
            }

            throw new AggregateException(
                $"Applying managed identity settings failed with {errors.Count} error(s).",
                errors.Select(e => new InvalidOperationException(e)));
        }

        private static XDocument ReadCustomizations(string zipPath)
        {
            using (var zip = ZipFile.OpenRead(zipPath))
            {
                var entry = FindCustomizations(zip);

                if (entry == null)
                {
                    return null;
                }

                string text;
                using (var reader = new StreamReader(entry.Open()))
                {
                    text = reader.ReadToEnd();
                }

                // Most solutions have no managed identity. Skip the XML parsing for them.
                return text.IndexOf("<managedidentities>", StringComparison.Ordinal) < 0
                    ? null
                    : XDocument.Parse(text, LoadOptions.PreserveWhitespace);
            }
        }

        private static void WriteCustomizations(string zipPath, XDocument customizations)
        {
            using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Update))
            using (var stream = FindCustomizations(zip).Open())
            {
                stream.SetLength(0);
                customizations.Save(stream, SaveOptions.DisableFormatting);
            }
        }

        private static ZipArchiveEntry FindCustomizations(ZipArchive zip)
        {
            return zip.Entries.FirstOrDefault(e => e.FullName.Equals(CustomizationsEntryName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
