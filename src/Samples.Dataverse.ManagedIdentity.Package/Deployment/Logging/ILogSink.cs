using System;

namespace Samples.Dataverse.ManagedIdentity.Package.Deployment.Logging
{
    /// <summary>
    /// Log target for the package's deployment logic. It keeps that logic independent of the
    /// Package Deployer host's <c>TraceLogger</c>, so the logic can be tested without the host.
    /// </summary>
    public interface ILogSink
    {
        void Info(string message);

        void Error(string message, Exception exception = null);
    }
}
