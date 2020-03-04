using System;
using System.Collections.Generic;
using System.Text;

namespace Kickstart.Build.Services.Model
{
    public class Environment
    {
        public int EnvironmentIdentifier { get; set; }
        public string EnvironmentName { get; set; }

        public EnvironmentTag EnvironmentTag { get; set; }

        public string TAG
        {
            get
            {
                var t = Enum.GetName(typeof(Model.EnvironmentTag), EnvironmentTag).ToLower();

                return t.ToUpper();

            }
        }

        public string tag
        {
            get
            {
                var t = Enum.GetName(typeof(Model.EnvironmentTag), EnvironmentTag).ToLower();

                return t.ToLower();

            }
        }
    }


    public enum EnvironmentTag
    {
        Dev,
        QA,
        Alpha,
        Prod,
        Uat,
        Demo
    }
}
