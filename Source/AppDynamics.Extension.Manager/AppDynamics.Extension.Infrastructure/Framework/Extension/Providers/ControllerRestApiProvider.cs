using AppDynamics.Infrastructure.Framework.Extension.Configuration;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension.Providers
{
    public class ControllerRestApiProvider : IDataProvider
    {
        private static readonly AppDynamicsAgentType agentConfig = AgentConfig.LoadAgentConfiguration();

        /// <summary>
        /// Make a request to the AppDynamics REST API
        /// </summary>
        /// <param name="value">RestRequest</param>
        /// <returns>Response</returns>
        public string Request(object value)
        {
            var restParams = (RestRequest)value;

            var request = (HttpWebRequest)WebRequest.Create(FormatUrl(restParams.Url));

            request.Method = restParams.Verb;
            request.PreAuthenticate = true;

            var proxy = GetProxy();
            if (proxy != null)
            {
                request.Proxy = proxy;
            }

            var auth = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(restParams.Username + ":" + restParams.Password));
            request.Headers.Add("Authorization", auth);
            request.PreAuthenticate = true;

            using (var response = (HttpWebResponse) request.GetResponse())
            {
                using (var dataStream = response.GetResponseStream())
                {
                    if (dataStream == null) return string.Empty;

                    var reader = new StreamReader(dataStream);
                    return reader.ReadToEnd();
                }
            }
        }


        /// <summary>
        /// Format the URL to contain the protocol if required
        /// </summary>
        /// <param name="url">The URL</param>
        /// <returns>Formatted URL</returns>
        private static string FormatUrl(string url)
        {
            return (url.Contains("://")) ? url : Protocol() + "://" + url;
        }


        /// <summary>
        /// Get a configured proxy if specified
        /// </summary>
        /// <returns>WebProxy if configured otherwise null</returns>
        private static WebProxy GetProxy()
        {
            var proxyConf = agentConfig.controller.proxy;
            
            if ((proxyConf == null) || (!agentConfig.controller.proxy.enabled)) return null;

            var proxy = new WebProxy(proxyConf.host, proxyConf.port);

            if ((proxyConf.authentication != null) && (proxyConf.authentication.enabled))
            {
                proxy.Credentials = new NetworkCredential(
                    proxyConf.authentication.user_name,
                    proxyConf.authentication.password,
                    proxyConf.authentication.domain);
            }

            return proxy;
        }


        /// <summary>
        /// Get the protocol to use
        /// </summary>
        private static string Protocol()
        {
            return agentConfig.controller.ssl ? "https" : "http";
        }
    }

    public class RestRequest
    {
        public string Url { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Verb { get; set; }
    }

 }
