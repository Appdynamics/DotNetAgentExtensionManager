using AppDynamics.Extension.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Extensions
{
    public class EmptyExtension: AExtensionBase
    {
        public override bool Execute()
        {
            throw new NotImplementedException();
        }

        public override void Initialize()
        {
            throw new NotImplementedException();
        }

        public override void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
