using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace AppDynamics.Infrastructure.Framework.Extension
{
    /// <summary>
    /// Exception used to be thrown within extension project.
    /// </summary>
    [Serializable()]
    public class ExtensionFrameworkException : Exception
    {

        #region Public Constructors
        // No empty constructor to avoid using it normally

        public ExtensionFrameworkException(string message, Exception innerException)
            : base(message, innerException)
        {

        }

        public ExtensionFrameworkException(string message)
            : base(message)
        {
            NeedStackTrace = false;
        }

        public ExtensionFrameworkException(string message, bool needStackTrace)
            : base(message)
        {
            NeedStackTrace = needStackTrace;
        }

        public ExtensionFrameworkException(string resourceName, object validationErrors)
        {
            this.ResourceName = resourceName;
            this.ValidationErrors = validationErrors;
        }

        #endregion

        #region public properties
        public string ResourceName { get; private set; }

        public object ValidationErrors { get; private set; }
        
        public bool NeedStackTrace { get; private set; }
        #endregion 

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        protected ExtensionFrameworkException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            this.ResourceName = info.GetString("ExtensionFrameworkException.ResourceName");
            this.ValidationErrors = info.GetValue("ExtensionFrameworkException.ValidationErrors", typeof(object));
        }

        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ExtensionFrameworkException.ResourceName", this.ResourceName);

            // Note: if "List<T>" isn't serializable you may need to work out another
            //       method of adding your list, this is just for show...
            info.AddValue("ExtensionFrameworkException.ValidationErrors", this.ValidationErrors, typeof(IList<string>));
        }

    }
}
