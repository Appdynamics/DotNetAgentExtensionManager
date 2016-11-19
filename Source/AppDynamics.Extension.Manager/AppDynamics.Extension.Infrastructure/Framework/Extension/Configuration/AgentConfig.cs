using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using NLog;
using AppDynamics.Infrastructure;

namespace AppDynamics.Infrastructure.Framework.Extension.Configuration
{
    public class AgentConfig
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private static string GetConfigFilePath()
        {
            var configLocation = ResourceStrings.AgentConfigFullPath;

            if (File.Exists(configLocation))
            {
                return configLocation;
            }

            throw new ExtensionFrameworkException(String.Format("Unable to find agent configuration file @", configLocation));
        }

        private static AppDynamicsAgentType LoadAgentConfiguration(XmlReader configFileReader)
        {
            AppDynamicsAgentType config;

            try
            {
                var settings = new XmlReaderSettings
                {
                    CloseInput = true
                };

                using (var reader = XmlReader.Create(configFileReader, settings))
                {
                    var serializer = new XmlSerializer(typeof(AppDynamicsAgentType));
                    config = (AppDynamicsAgentType)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new ExtensionFrameworkException(String.Format("Unable to deserialize the agent configuration"), ex);
            }

            return config;
        }


        public static AppDynamicsAgentType LoadAgentConfiguration()
        {
            var configFile = GetConfigFilePath();
            Logger.Info("Reading configuration file: " + configFile);

            using (XmlReader reader = new XmlTextReader(configFile))
            {
                return LoadAgentConfiguration(reader);
            }
        }

        //public static string GetUsername(AppDynamicsAgentType config)
        //{
        //    var acc = config.controller.account;
        //    return (acc != null) ? @"singularity-agent@" + acc.name : "singularity-agent@customer1";
        //}

        //public static string GetPassword(AppDynamicsAgentType config)
        //{
        //    var acc = config.controller.account;
        //    return (acc != null) ? acc.password : @"SJ5b2m7d1$354";
        //}
    }
}

//public void Send(byte[] data, string url, WebProxy proxy = null)
//        {
//            ServicePoint sp = ServicePointManager.FindServicePoint(new Uri(url));
//            sp.ReceiveBufferSize = 32000;

//            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
//            request.Method = (data == null || data.Length == 0) ? "GET" : "POST";

//            request.PreAuthenticate = true;
//            request.Timeout = 60000;
//            request.ReadWriteTimeout = 60000;
//            if (proxy != null)
//            {
//                request.Proxy = proxy;
//            }

//            string auth = "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(GetUserName() + ":" + GetPassword()));
//            request.Headers.Add("Authorization", auth);
//            request.PreAuthenticate = true;

//            request.ContentLength = data.Length;

//            if (data.Length > 0)
//            {
//                using (Stream postStream = request.GetRequestStream())
//                {
//                    postStream.Write(data, 0, data.Length);
//                    postStream.Close();
//                }
//            }

//            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
//            {
//                HttpStatus = response.StatusCode;
//                using (Stream resp = response.GetResponseStream())
//                {
//                    ProcessResponse(resp);
//                }
//            }
//        }