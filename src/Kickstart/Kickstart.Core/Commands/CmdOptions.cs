using CommandLine;

namespace Kickstart.Commands
{
    [Verb("register-app", HelpText = "Register app")]
    public class MetaRepoCreateOptions
    {
        //[Option("environment",HelpText = "Environment", Required = false)]
        // this doesn't work in TFS as it cannot send ampty strings for some reason in TFS task groups
        // so we will set it differently
        public string Environment { get; set; }

        [Option("app-code", HelpText = "Application code", Required = true)]
        public string AppCode { get; set; }

        [Option("description", HelpText = "Application description", Required = false)]
        public string Description { get; set; }

        [Option("base-url", HelpText = "Application base url", Required = true)]
        public string BaseUrl { get; set; }
    }

    [Verb("register-menu", HelpText = "Register menu for app")]
    public class MegaSolutionCreateOptions
    {
        //[Option("environment", HelpText = "Environment", Required = false)]
        // this doesn't work in TFS as it cannot send ampty strings for some reason in TFS task groups
        // so we will set it differently
        public string Environment { get; set; }

        [Option("app-code", HelpText = "Application code which menu is registered for", Required = true)]
        public string AppCode { get; set; }

        [Option("file", HelpText = "Name of the json file with menu items", Required = true)]
        public string JsonFile { get; set; }
    }
}
