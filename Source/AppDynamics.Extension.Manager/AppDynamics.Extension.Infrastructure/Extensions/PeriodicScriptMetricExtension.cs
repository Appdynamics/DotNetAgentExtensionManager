using AppDynamics.Extension.SDK;
using AppDynamics.Extension.SDK.Model;
using AppDynamics.Infrastructure.Framework.Extension;
using AppDynamics.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace AppDynamics.Infrastructure.Extensions
{
    public class PeriodicScriptMetricExtension: AExtensionBase
    {
        private string _scriptPath = "";
        private string _lineFormat = "";

        public override void Initialize()
        {
            // optional- using current class as logger
            logger = NLog.LogManager.GetLogger(ResourceStrings.ExtensionFrameworkNamespace + "." + ExtensionName);

            if (!Parameters.TryGetValue("_path", out _scriptPath))
            {
                throw new ExtensionFrameworkException("Script path is not defined.");
            }
            else if (!System.IO.File.Exists(_scriptPath))
            {
                throw new ExtensionFrameworkException(String.Format("Could not locate script file @{0}", _scriptPath));
            }

            // checking for optional parameter- line format
            if (!Parameters.TryGetValue("LineFormat", out _lineFormat))
            {
                _lineFormat = ResourceStrings.LineFormatScriptExtension;
            }

            _lineFormat = convertFormatToRegExPattern(_lineFormat);

            if (logger.IsDebugEnabled)
                logger.Debug(String.Format("Using Line Format = {0}", _lineFormat));
        }

        public override void Stop()
        {
            logger.Info(String.Format("Stopped-{0}", this.ExtensionName));
        }

        public override bool Execute()
        {
            if (logger.IsTraceEnabled)
                logger.Trace(String.Format("Executing-{0}", this.ExtensionName));

            // execute and capture metric/instance values
            try
            {
                executeScript();

                return true;
            }
            catch (ExtensionFrameworkException ex)
            {
                logger.Error(String.Format("Error while executing {0}", ExtensionName), ex);
                return false;
            }
        }

        private void executeScript()
        {
            string error = "";

            string output = FileHelper.GetOutputofScript(_scriptPath, out error);

            if (!"".Equals(error))
            {
                throw new ExtensionFrameworkException(error);
            }

            foreach (string strLine in output.Split(new char[] { '\n' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    string metricLine = strLine.Trim('"', '\r');

                    if (logger.IsTraceEnabled)
                        logger.Trace(String.Format("MetricLine for {0} is {1}....", ExtensionName, metricLine));

                    parseMetricValue(metricLine);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Could not fill value for instances, check metric names are correct.");
                    // Not throwing, check for next value
                }
            }
        }

        private void parseMetricValue(string strLine)
        {

            #region Old Way Commented- To be deleted later
            //string[] strArr = metricLine.Split('|', ',', '=');
            //if (strArr.Length == 4)
            //{
            //    string metricName = strArr[0];

            //    string instanceName = strArr[1];

            //    string value = strArr[3];

            //    fillValueInInstances(metricName, instanceName, value);
            //}
            //else
            //{
            //    if (logger.IsDebugEnabled)
            //        logger.Debug("incorrect array received length={0} value= {1}", strArr.Length, strArr.ToString());
            //}
            #endregion

            // reg ex way to support dynamic format
            Match match = Regex.Match(strLine, _lineFormat, RegexOptions.IgnoreCase);

            string metricName = match.Groups["MetricName"].Value.Trim();
            string instanceName = match.Groups["InstanceName"].Value.Trim();
            string value = match.Groups["Value"].Value.Trim();

            if ("".Equals(metricName) || "".Equals(instanceName) || "".Equals(value))
            {
                logger.Warn("Incorrect format received: {0}", strLine);
            }
            else
            {
                fillValueInInstances(metricName, instanceName, value);
            }
        }

        private void fillValueInInstances(string metricName, string instanceName, string value)
        {
            if (logger.IsTraceEnabled)
                logger.Trace(String.Format("ExtName={0}, metricName={1}, instanceName={2}, value={3}",
                    ExtensionName, metricName, instanceName, value));

            long d = 0;
            value = value.Trim('"');
            if (!long.TryParse(value, out d))
            {
                logger.Warn("Metric out value is {0}" + value);
            }


            // LINQ approach, throws exception sometimes
            var ee = base.ListExtensionInstance
                                        .Where(ins => ins.InstanceName.ToLower().Equals(instanceName.ToLower()))
                                        .SelectMany(ins => ins.ListExtensionMetrics)
                                        .Where(m => m.MetricName.ToLower().Equals(metricName.ToLower()))
                                        .Single();

            ee.Value = d;
            
            //// Double loop approach
            //foreach (ExtensionInstance instance in ListExtensionInstance)
            //{
            //    if (instance.InstanceName.ToLower().Equals(instanceName.ToLower()))
            //    {
            //        foreach (ExtensionMetric eM in instance.ListExtensionMetrics)
            //        {
            //            if (eM.MetricName.ToLower().Equals(metricName.ToLower()))
            //            {
            //                eM.Value = d;
            //                break;
            //            }
            //        }
            //        break;
            //    }
            //}



        }

        private string convertFormatToRegExPattern(string format)
        {
            // #MetricName#|#InstanceName#, value = #Value#
            // (?<MetricName>.*)\|(?<InstanceName>.*), value = (?<Value>\d{1,})

            string regExPattern = format;

            // replacing regex related chars with \<chars>
            char[] specialChars = { '\\', '.', '$', '^', '{', '[', '(', '|', ')', '*', '+', '?' };

            if (regExPattern.IndexOfAny(specialChars) >= 0)
            {
                foreach (char c in specialChars)
                {
                    regExPattern = regExPattern.Replace(c.ToString(), "\\" + c);
                }
            }

            regExPattern = regExPattern.Replace("#MetricName#", "(?<MetricName>.*)")
                                        .Replace("#InstanceName#", "(?<InstanceName>.*)")
                                        .Replace("#Value#", "(?<Value>\\d{1,})");

            return regExPattern;
        }

    }
}
