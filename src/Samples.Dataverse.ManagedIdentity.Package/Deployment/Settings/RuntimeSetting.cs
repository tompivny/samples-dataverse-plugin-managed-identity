namespace Samples.Dataverse.ManagedIdentity.Package.Deployment.Settings
{
    /// <summary>
    /// One Package Deployer runtime setting in the form <c>&lt;Prefix&gt;:&lt;Name&gt;:&lt;Property&gt;</c>,
    /// for example <c>ManagedIdentity:smp-dataverse-plugin-uami:ApplicationId</c>.
    /// </summary>
    public sealed class RuntimeSetting
    {
        public RuntimeSetting(string key, string name, string property, string value)
        {
            Key = key;
            Name = name;
            Property = property;
            Value = value;
        }

        /// <summary>The complete setting key, for messages.</summary>
        public string Key { get; }

        /// <summary>The middle segment: the name of the item that the setting applies to.</summary>
        public string Name { get; }

        /// <summary>The last segment: the property of the item to set.</summary>
        public string Property { get; }

        public string Value { get; }
    }
}
