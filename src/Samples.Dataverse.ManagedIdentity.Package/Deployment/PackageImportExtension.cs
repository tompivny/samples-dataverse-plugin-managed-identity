using System;
using System.ComponentModel.Composition;
using System.IO;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;
using Samples.Dataverse.ManagedIdentity.Package.Deployment.Logging;
using Samples.Dataverse.ManagedIdentity.Package.Deployment.ManagedIdentities;

namespace Samples.Dataverse.ManagedIdentity.Package.Deployment
{
    /// <summary>
    /// Import package starter frame.
    /// </summary>
    [Export(typeof(IImportExtensions))]
    public class PackageImportExtension : ImportExtension
    {
        #region Metadata

        /// <summary>
        /// Folder name where package assets are located in the final output package zip.
        /// </summary>
        public override string GetImportPackageDataFolderName => "PkgAssets";

        /// <summary>
        /// Name of the Import Package to Use
        /// </summary>
        /// <param name="plural">if true, return plural version</param>
        public override string GetNameOfImport(bool plural) => "Samples.Dataverse.ManagedIdentity.Package";

        /// <summary>
        /// Long name of the Import Package.
        /// </summary>
        public override string GetLongNameOfImport => "Samples.Dataverse.ManagedIdentity.Package";

        /// <summary>
        /// Description of the package, used in the package selection UI
        /// </summary>
        public override string GetImportPackageDescriptionText => "Samples.Dataverse.ManagedIdentity.Package";

        #endregion

        /// <summary>
        /// Sets the managed identity values of this environment in the solution zips, from runtime settings
        /// such as <c>ManagedIdentity:&lt;name&gt;:ApplicationId</c>. This is the last stage before the first
        /// solution is imported. <c>BeforeImportStage</c> is too late: it runs after the solution import.
        /// </summary>
        /// <see cref="ImportExtension.InitializeCustomExtension"/>
        public override void InitializeCustomExtension()
        {
            new ManagedIdentityPatcher(new TraceLoggerLogSink(PackageLog))
                .Patch(Path.Combine(CurrentPackageLocation, GetImportPackageDataFolderName), RuntimeSettings);
        }

        /// <summary>
        /// Called before the Main Import process begins, after solutions and data.
        /// </summary>
        /// <see cref="ImportExtension.BeforeImportStage"/>
        /// <returns></returns>
        public override bool BeforeImportStage()
        {
            return true;
        }

        /// <summary>
        /// Raised before the named solution is imported to allow for any configuration settings to be made to the import process
        /// </summary>
        /// <see cref="ImportExtension.PreSolutionImport"/>
        /// <param name="solutionName">name of the solution about to be imported</param>
        /// <param name="solutionOverwriteUnmanagedCustomizations">Value of this field from the solution configuration entry</param>
        /// <param name="solutionPublishWorkflowsAndActivatePlugins">Value of this field from the solution configuration entry</param>
        /// <param name="overwriteUnmanagedCustomizations">If set to true, imports the Solution with Override Customizations enabled</param>
        /// <param name="publishWorkflowsAndActivatePlugins">If set to true, attempts to auto publish workflows and activities as part of solution deployment</param>
        public override void PreSolutionImport(string solutionName, bool solutionOverwriteUnmanagedCustomizations, bool solutionPublishWorkflowsAndActivatePlugins, out bool overwriteUnmanagedCustomizations, out bool publishWorkflowsAndActivatePlugins)
        {
            base.PreSolutionImport(solutionName, solutionOverwriteUnmanagedCustomizations, solutionPublishWorkflowsAndActivatePlugins, out overwriteUnmanagedCustomizations, out publishWorkflowsAndActivatePlugins);
        }

        /// <summary>
        /// Called during a solution upgrade when both solutions, old and new, are present in the system.
        /// This function can be used to provide a means to do data transformation or upgrade while a solution update is occurring.
        /// </summary>
        /// <see cref="ImportExtension.RunSolutionUpgradeMigrationStep"/>
        /// <param name="solutionName">Name of the solution</param>
        /// <param name="oldVersion">version number of the old solution</param>
        /// <param name="newVersion">Version number of the new solution</param>
        /// <param name="oldSolutionId">Solution ID of the old solution</param>
        /// <param name="newSolutionId">Solution ID of the new solution</param>
        public override void RunSolutionUpgradeMigrationStep(string solutionName, string oldVersion, string newVersion, Guid oldSolutionId, Guid newSolutionId)
        {
            base.RunSolutionUpgradeMigrationStep(solutionName, oldVersion, newVersion, oldSolutionId, newSolutionId);
        }

        /// <summary>
        /// Called After all Import steps are complete, allowing for final customizations or tweaking of the instance.
        /// </summary>
        /// <see cref="ImportExtension.AfterPrimaryImport"/>
        /// <returns></returns>
        public override bool AfterPrimaryImport()
        {
            return true;
        }
    }
}
