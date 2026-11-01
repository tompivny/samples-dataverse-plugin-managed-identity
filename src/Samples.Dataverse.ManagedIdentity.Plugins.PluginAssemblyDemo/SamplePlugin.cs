using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Extensions;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Samples.Dataverse.ManagedIdentity.Plugins.PluginAssemblyDemo
{
    public class SamplePlugin : IPlugin
    {
        private static readonly HttpClient Http = new HttpClient();

        private readonly string _functionUrl;
        private readonly string _scope;

        // Unsecure configuration: "<function url>|<token scope>"
        // e.g. "https://func-x.azurewebsites.net/api/whoami|api://<client id>/.default"
        public SamplePlugin(string unsecureConfiguration, string secureConfiguration)
        {
            var parts = unsecureConfiguration.Split('|');
            _functionUrl = parts[0];
            _scope = parts[1];
        }

        public void Execute(IServiceProvider serviceProvider)
        {
            var tracing = serviceProvider.Get<ITracingService>();
            var token = serviceProvider.Get<IManagedIdentityService>().AcquireToken(new[] { _scope });

            using (var request = new HttpRequestMessage(HttpMethod.Get, _functionUrl))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                using (var response = Http.SendAsync(request).GetAwaiter().GetResult())
                {
                    response.EnsureSuccessStatusCode();
                    tracing.Trace(response.Content.ReadAsStringAsync().GetAwaiter().GetResult());
                }
            }
        }
    }
}
