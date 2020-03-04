using System;
using System.Collections.Generic;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Logging;

namespace Kickstart.Pass4
{
    public class SolutionBuilder
    {
        public bool Compile(string projectFileName, string logfile)
        {
            Environment.SetEnvironmentVariable("MSBuildSDKsPath", @"C:\Program Files\dotnet\sdk\2.0.0\Sdks");
            Environment.SetEnvironmentVariable("VSINSTALLDIR",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise");
            Environment.SetEnvironmentVariable("VisualStudioVersion", @"15.00");
            Environment.SetEnvironmentVariable("MSBUILD_EXE_PATH",
                @"C:\Program Files (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\MSBuild.exe");
            var pc = new ProjectCollection();
            var GlobalProperty = new Dictionary<string, string>();
            GlobalProperty.Add("Configuration", "Debug");
            GlobalProperty.Add("Platform", "Any CPU");

            var BuidlRequest = new BuildRequestData(projectFileName, GlobalProperty, null, new[] {"Build"}, null);

            var buildParams = new BuildParameters(pc)
            {
                // DefaultToolsVersion = "12.0"
                Loggers = new[] {new ConsoleLogger()}
            };
            var result = BuildManager.DefaultBuildManager.Build(buildParams, BuidlRequest);

            return result.OverallResult == BuildResultCode.Success;
        }
    }
}