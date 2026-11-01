using System;
using System.Diagnostics;
using Microsoft.Xrm.Tooling.PackageDeployment.CrmPackageExtentionBase;

namespace Samples.Dataverse.ManagedIdentity.Package.Deployment.Logging
{
    /// <summary>
    /// Writes to the Package Deployer deployment log through the host's <see cref="TraceLogger"/>.
    /// </summary>
    internal sealed class TraceLoggerLogSink : ILogSink
    {
        private readonly TraceLogger _traceLogger;

        public TraceLoggerLogSink(TraceLogger traceLogger)
        {
            _traceLogger = traceLogger ?? throw new ArgumentNullException(nameof(traceLogger));
        }

        public void Info(string message) => _traceLogger.Log(message, TraceEventType.Information);

        public void Error(string message, Exception exception = null)
        {
            if (exception == null)
            {
                _traceLogger.Log(message, TraceEventType.Error);
            }
            else
            {
                _traceLogger.Log(message, TraceEventType.Error, exception);
            }
        }
    }
}
