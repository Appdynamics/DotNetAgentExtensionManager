using AppDynamics.Infrastructure;
using AppDynamics.Infrastructure.Framework.Extension;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //testEncryption();
            //DisplayEventLogProperties();
            runExtension();

            //Console.ReadLine();
        }

        static void DisplayEventLogProperties()
        {
            // Iterate through the current set of event log files,
            // displaying the property settings for each file.

            EventLog[] eventLogs = EventLog.GetEventLogs();
            foreach (EventLog e in eventLogs)
            {
                try
                {

                    Console.WriteLine();
                    Console.WriteLine("{0}:", e.LogDisplayName);
                    Console.WriteLine("  Log name = \t\t {0}", e.Log);
                    Console.WriteLine("  Number of event log entries = {0}", e.Entries.Count.ToString());
                }
                catch (Exception) { }
            }
        }

        static void runExtension()
        {
            ExtensionLoader exeLoader = new ExtensionLoader();

            exeLoader.CreateExtensions();

            Console.WriteLine("Extensions created- now starting.");

            exeLoader.StartExtensions();

            Console.WriteLine("Extensions running- Press Enter to stop.");
            Console.ReadLine();

            exeLoader.StopExtensions();

            Console.WriteLine("Extensions stopped- Press Enter to exit.");
            Console.ReadLine();
        }
        static void testEncryption()
        {
            string pwd = Console.ReadLine();
            
            var cp = new AppDynamics.Infrastructure.Framework.Extension.Providers.BasicCryptoProvider();

            string encrypted = cp.EncryptString(pwd);
            Console.WriteLine("Basic->"+encrypted);

            var cp1 = new AppDynamics.Infrastructure.Framework.Extension.Providers.BasicCryptoProvider();
            Console.WriteLine(cp1.DecryptString(encrypted));


            var cp3 = new AppDynamics.Infrastructure.Framework.Extension.Providers.RijndaelCryptoProvider();

            string encrypted1 = cp3.EncryptString(pwd);
            Console.WriteLine("++++++++++++++++++++++++++");
            Console.WriteLine("Rijindale->" + encrypted1);

            var cp4 = new AppDynamics.Infrastructure.Framework.Extension.Providers.RijndaelCryptoProvider();
            Console.WriteLine(cp4.DecryptString(encrypted1));


            Console.Read();
        }


        static void Main1(string[] args)
        {
            string extensionsPath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "extensions";

            if (Directory.Exists(extensionsPath))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(extensionsPath);

                foreach (string extName in Directory.GetDirectories(extensionsPath))
                {
                    string xmlFilePath = extName + Path.DirectorySeparatorChar + "extension.xml";

                    if (File.Exists(xmlFilePath))
                    {
                        // load XML
                        XmlDocument extensionXML = new XmlDocument();

                        extensionXML.Load(xmlFilePath);

                        XmlNode root = extensionXML.DocumentElement;

                        XmlNode path = root.FirstChild;
                        string dllName = path.InnerText;

                        string extType = root.Attributes["type"].Value;

                        var assemly = Assembly.LoadFrom(extName + Path.DirectorySeparatorChar + dllName);
                        
                        List<string> t = new List<string>();

                        foreach (Type type in assemly.GetTypes().
                            Where(pextType => pextType.GetInterfaces().Contains(typeof(AppDynamics.Extension.SDK.IExtension))))
                        {
                            t.Add(type.ToString());
                        }
                        
                    }
                }

            }
            else
            {
                throw new ExtensionFrameworkException("No extensions present. Extensions folder doesn't exixts.");
            }
        }

        //private static string convertFormatToRegExPattern(string format)
        //{
        //    // #MetricName#|#InstanceName#, value = #Value#
        //    // (?<MetricName>.*)\|(?<InstanceName>.*), value = (?<Value>\d{1,})

        //    string regExPattern = format;

        //    // replacing regex related chars with \<chars>
        //    char[] specialChars = { '\\', '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?' };

        //    if (regExPattern.IndexOfAny(specialChars) >= 0)
        //    {
        //        foreach (char c in specialChars)
        //        {
        //            regExPattern = regExPattern.Replace(c.ToString(), "\\" + c);
        //        }
        //    }

        //    regExPattern = regExPattern.Replace("#MetricName#", "(?<MetricName>.*)")
        //                                .Replace("#InstanceName#", "(?<InstanceName>.*)")
        //                                .Replace("#Value#", "(?<Value>\\d{1,})");

        //    return regExPattern;
        //}
    }
}
