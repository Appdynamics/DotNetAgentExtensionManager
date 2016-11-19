using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using AppDynamics.Extension.SDK.Model;
using AppDynamics.Extension.SDK.Model.XML;
using AppDynamics.Infrastructure.Framework.Extension.Handlers;
using AppDynamics.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace AppDynamics.Infrastructure.Framework.Extension
{
    public class ExtensionLoader
    {
        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private static List<ExtensionContainer> _extensions;

        public void CreateExtensions()
        {
            // scan through folder "extensions" and load all extensions
            string error = "";

            List<IExtension> listExtensions = GetAllAvailableExtensions(out error);

            if (listExtensions != null && listExtensions.Count > 0)
            {
                _logger.Info(String.Format("Found {0} extensions.", listExtensions.Count));

                _extensions = new List<ExtensionContainer>();

                foreach (IExtension ext in listExtensions)
                {
                    ExtensionContainer extCont = new ExtensionContainer(ext);

                    _extensions.Add(extCont);

                    if (_logger.IsDebugEnabled)
                        _logger.Debug(String.Format("Found {0} extensions.", listExtensions.Count));

                }
            }
            else
            {
                _logger.Info("Couldn't find any extensions in extensions directory.");
            }
        }

        public void StartExtensions()
        {
            _logger.Info("Starting extensions...");

            if (_extensions != null)
            {
                foreach (var extension in _extensions)
                {
                    var extName = extension.extensionName;
                    try
                    {
                        _logger.Info("Starting extension- {0}", extName);

                        extension.Start();
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Failed to start extension {0}", extName);
                    }

                }
            }
            _logger.Info("Started extensions");
        }

        public void StopExtensions()
        {
            _logger.Info("Stopping extensions...");

            if (_extensions != null)
            {
                foreach (var extension in _extensions)
                {
                    var extName = extension.extensionName;

                    try
                    {
                        _logger.Info("Stopping extension {0}", extName);

                        // need to make sure all resources are released.
                        extension.Stop();

                        _logger.Info("Stopped extension {0}", extName);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Failed to stop extension {0}", extName), ex);
                    }

                }
            }
            _logger.Info("Stopped extensions");
        }

        /// <summary>
        /// Returns all available extension that can be loaded. Record error in out param
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static List<IExtension> GetAllAvailableExtensions(out string errorMsg)
        {
            List<IExtension> listExtensions = null;

            errorMsg = "";

            string extensionDir = ResourceStrings.ExtensionDirPath;

            if (Directory.Exists(extensionDir))
            {
                listExtensions = new List<IExtension>();

                if (_logger.IsTraceEnabled)
                    _logger.Trace("Found extension directory...");

                foreach (string extNamewithPath in Directory.GetDirectories(extensionDir))
                {
                    string xmlFilePath = extNamewithPath + Path.DirectorySeparatorChar+ ResourceStrings.ExtensionXmlName;

                    string extLocalName = FileHelper.GetDirNamefromPath(extNamewithPath);

                    if (_logger.IsTraceEnabled)
                        _logger.Trace(String.Format("Found extension {0} ...", extLocalName));

                    if (File.Exists(xmlFilePath))
                    {
                        try
                        {
                            ExtensionXMLHandler.ValidateExtensionXML(xmlFilePath);

                            IExtension ext = ExtensionXMLHandler.GetExtObjectFromXML(xmlFilePath);

                            if (ext == null)
                                throw new ExtensionFrameworkException("Could not initialize extension from xml", false);

                            // If name is not set from xml, take directory name
                            if (String.IsNullOrWhiteSpace(ext.ExtensionName))
                                ext.ExtensionName = extLocalName;

                            // Hack: Setting xml file path #scriptextension
                            if (ext.Parameters == null)
                                ext.Parameters = new Dictionary<string, string>();

                            ext.Parameters.Add(ResourceStrings.XMLParamName, xmlFilePath);


                            listExtensions.Add(ext);
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "Could not create extension {0}", extLocalName);
                        }
                    }
                    else
                    {
                        errorMsg += "No extension xml found at @" + xmlFilePath;
                    }
                }
            }
            else
            {
                errorMsg= String.Format("Extension Dir doesn't exists.@{0}", extensionDir);
                _logger.Info(errorMsg);
            }
            return listExtensions;
        }

        /// <summary>
        /// Depricated-- uses xml deserilization to get the object..
        /// </summary>
        /// <param name="errorMsg"></param>
        /// <returns></returns>
        public static List<ExtensionXML> GetAllExtensionXMLObjects(out string errorMsg)
        {
            List<ExtensionXML> listExtensionXML = null;

            errorMsg = "";

            string extensionDir = ResourceStrings.ExtensionDirPath;

            if (Directory.Exists(extensionDir))
            {
                listExtensionXML = new List<ExtensionXML>();

                foreach (string extNamewithPath in Directory.GetDirectories(extensionDir))
                {
                    string xmlFilePath = extNamewithPath + Path.DirectorySeparatorChar + ResourceStrings.ExtensionXmlName;

                    if (File.Exists(xmlFilePath))
                    {
                        try
                        {
                            string extLocalName = FileHelper.GetDirNamefromPath(extNamewithPath);

                            ExtensionXML obj = ExtensionXMLHandler.GetExtensionXMlObject(xmlFilePath);

                            // If name is not set from xml, take directory name
                            if (String.IsNullOrWhiteSpace(obj.name))
                                obj.name = extLocalName;

                            _logger.Info(String.Format("extension object deserialized from extension.xml for {0}", extLocalName));

                            listExtensionXML.Add(obj);
                        }
                        catch (Exception ex)
                        {
                            errorMsg += String.Format("- Could not parse extension.xml @{0}", xmlFilePath);
                            _logger.Error(ex, errorMsg);
                        }
                    }
                    else
                    {
                        _logger.Warn(String.Format("Extension.xml not found @{0}", extNamewithPath));
                    }
                }
            }
            else
            {
                errorMsg = "Extension Directory not found.";
            }
            return listExtensionXML;
        }
    }
}
