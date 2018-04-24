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
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Make a request to the AppDynamics REST API
        /// </summary>
        /// <param name="value">RestRequest</param>
        /// <returns>Response</returns>
        public string Request(object value)
        {
            var restParams = (RestRequest)value;

            #region to enable tls1.2, probably not needed with .net 4.5

            if (_logger.IsDebugEnabled)
                _logger.Debug("Making request to post event");

            verifyTLS12();
            #endregion

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

            request.ContentType = "application/x-www-form-urlencoded";

            byte[] byteArray = Encoding.UTF8.GetBytes(restParams.Body);
            request.ContentLength = byteArray.Length;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(byteArray, 0, byteArray.Length);

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var dataStream = response.GetResponseStream())
                    {
                        if (dataStream == null) return string.Empty;

                        var reader = new StreamReader(dataStream);

                        return reader.ReadToEnd();
                    }
                }
            }
        }

        private static void verifyTLS12()
        {
            if (agentConfig.controller.ssl && agentConfig.controller.enable_tls12)
            {
                if (_logger.IsDebugEnabled)
                    _logger.Debug("Using security protocol TLS1.2- " + ServicePointManager.SecurityProtocol);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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

        public string Body { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Verb { get; set; }
    }

 }
