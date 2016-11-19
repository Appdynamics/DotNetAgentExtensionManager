using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model.Enumeration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension
{
    public class ExtensionActivator
    {

        private static NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        public static IExtension CreateExtensionInstance(string executionPath, string extensionType, 
            string executionType, string executionMode, string xmlFilePath)
        {
            IExtension extobj = null;

            Type type = getTypeStr(executionPath, executionType, xmlFilePath);

            if (type != null)
            {
                // Let this throw exception, if can not create object of type
                extobj = (IExtension)System.Activator.CreateInstance(type);
            }

            if (extobj != null)
            {
                extobj.ExecutionMode = (ExecutionMode)Enum.Parse(typeof(ExecutionMode), executionMode, true);
                extobj.ExecutionType = (ExecutionType)Enum.Parse(typeof(ExecutionType), executionType, true);
                extobj.Type = (ExtensionType)Enum.Parse(typeof(ExtensionType), extensionType, true);
            }
            else
            {
                throw new ExtensionFrameworkException(String.Format(
                    "Could not create object of type {0}", executionPath));
            }

            return extobj;
        }

        /// <summary>
        /// Returns type string for creating extensions both internal and external.
        /// </summary>
        /// <param name="executionPath"></param>
        /// <param name="extensionType"></param>
        /// <param name="executionType"></param>
        /// <param name="executionMode"></param>
        /// <returns></returns>
        private static Type getTypeStr(string executionPath, string executionType, string xmlFilePath)
        {
            Type type = null;

            if (executionType.Equals(ExecutionType.SCRIPT.ToString(), StringComparison.CurrentCultureIgnoreCase))
            {
                // Only for script execution, coz execution path contains script path not dll pointer 
                // #scriptExtension 
                _logger.Trace(String.Format("changing execution path for script extension-{0}", executionPath));

                executionPath = ExecutionType.SCRIPT.ToString().ToLower();

            }

            string typeString = "";
            if (!ResourceStrings.InternalExtensions.TryGetValue(executionPath, out typeString))
            {
                // for external extension, execution path should be equal to type string.
                typeString = executionPath;

                // WIP: need to make sure that the dlls present in extension directory are loaded
                // Without dlls loaded, activator will fail to create object
                type = loadAssemblies(xmlFilePath, executionPath);
            }
            else
            {
                type = Type.GetType(typeString);
            }


            return type;
        }

        private static Type loadAssemblies(string xmlPath, string executionPath)
        {
            Type type = null;

            string directoryPath = xmlPath.Replace(ResourceStrings.ExtensionXmlName, "");

            if (Directory.Exists(directoryPath))
            {
                // #IMP- Loading all assemblies in directory 
                foreach (string fileName in Directory.GetFiles(directoryPath, "*.dll", SearchOption.TopDirectoryOnly))
                {
                    
                    System.Reflection.Assembly a = System.Reflection.Assembly.LoadFrom(fileName);

                    foreach (Type t in a.GetTypes())
                    {
                        if (t.Name.Equals(executionPath, StringComparison.InvariantCultureIgnoreCase) 
                         || t.FullName.Equals(executionPath, StringComparison.InvariantCultureIgnoreCase))
                        {
                            type = t;
                            break;
                        }
                    }
                }
            }
            return type;
        }



    }
}
