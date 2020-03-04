using Kickstart.GroupService.Model;
using Kickstart.TemplateConverterService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Kickstart.GroupService
{
    class Program
    {
        static void Main(string[] args)
        {
            
            var serviceProvider = new ServiceCollection()
                .AddLogging(builder => { builder.AddConsole(); })
                .AddSingleton<IDirectoryService, DirectoryService>()
                .AddSingleton<IFindAndDeleteService, FindAndDeleteService>()
                .AddSingleton<IFindAndRenameService, FindAndRenameService>()
                .AddSingleton<IFindAndReplaceService, FindAndReplaceService>()
                .AddTransient<IServiceFromTemplateBuilder, PartyServiceFromPartyTemplateBuilder>()
                .BuildServiceProvider();


            var builderOptions = new BuilderOptions()
            {
                DestTemplatePath = @"C:\temp\Output\"
            };
            var destDir = builderOptions.DestTemplatePath;
            if (Directory.Exists(destDir))
            {
                Directory.Delete(destDir, true);
            }

            var builderService = serviceProvider.GetService<SampleBuilder>();
           
            builderService.BuildService(builderOptions);

        }

    }
}
