using System;
using System.IO;
using CommandLine;
using Kickstart.Commands;
using Kickstart.Interface;
using Serilog;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
namespace Kickstart
{
    public class Program
    {
        //private static readonly List<KSolutionGroup> _kickstartedSolutions =
        //  new List<KSolutionGroup> {new KSolutionGroup()};

        private static ILogger<Program> Logger { get; set; }

        private static int Main(string[] args)
        {

            /*
            var timeString = DateTime.Now.ToString("yyyyMMddHHmmss");
            var outputRootPath = Path.Combine(@"c:\temp\", $"Company_3_0_{timeString}");

            var service = new KickstartService();
            var solutionGroupList = service.GetSolutionGroupListFromCode();
            service.ExecuteAsync(outputRootPath, "Company.All.Sln", solutionGroupList);
            */
            try
            {
                var serviceProvider = InitServiceProvider();


                return Parser.Default.ParseArguments<MegaSolutionCreateOptions, MetaRepoCreateOptions, KickstartFromProtoOptions>(args)
                    .MapResult(
                        (MegaSolutionCreateOptions opts) =>
                        {
                            //opts.Environment = appEnvironment;
                            return ExecuteCommand(serviceProvider, opts);
                        },
                        (MetaRepoCreateOptions opts) =>
                        {
                            //opts.Environment = appEnvironment;
                            return ExecuteCommand(serviceProvider, opts);
                        },
                        (KickstartFromProtoOptions opts) =>
                        {
                            //opts.Environment = appEnvironment;
                            return ExecuteCommand(serviceProvider, opts);
                        },

                        errs => 1);
                    
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);
                return -1;
            }
        }
        private static int ExecuteCommand<TOptions>(IServiceProvider serviceProvider, TOptions options)
        {
            
            var commandResult = serviceProvider.GetService<ICommand<TOptions>>().Run(options);

            if (commandResult.IsSuccess)
            {
                Logger.LogDebug("Command completed successfully!");
                return 0;
            }
            
            Logger.LogError($"Command completed unsuccessfully: {commandResult.Message}");
            return 1;
        }

        private static IServiceProvider InitServiceProvider()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
               
                .Build();

            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(configuration)
                .AddLogging(configure => configure.SetMinimumLevel(LogLevel.Debug))
                .BuildServiceProvider();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            ConfigureLogger(loggerFactory, configuration);

            return serviceProvider;
        }

        private static void ConfigureLogger(ILoggerFactory loggerFactory, IConfigurationRoot configuration)
        {
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            loggerFactory.AddSerilog();

            Logger = loggerFactory.CreateLogger<Program>();
            Logger.LogInformation("Starting application");
        }

    }
}