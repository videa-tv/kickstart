using Kickstart.GroupService.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kickstart.GroupService
{
    public class PartyServiceFromPartyTemplateBuilder : IServiceFromTemplateBuilder
    {
        IDirectoryService _directoryService;
        IFindAndDeleteService _findAndDeleteService;
        IFindAndRenameService _findAndRenameService;
        IFindAndReplaceService _findAndReplaceService;

        public PartyServiceFromPartyTemplateBuilder(
            IDirectoryService directoryService, 
            IFindAndDeleteService findAndDeleteService,
            IFindAndRenameService findAndRenameService,
            IFindAndReplaceService findAndReplaceService)
        {
            _directoryService = directoryService;
            _findAndDeleteService = findAndDeleteService;
            _findAndRenameService = findAndRenameService;
            _findAndReplaceService = findAndReplaceService;

        }
        public void BuildService(BuilderOptions builderOptions)
        {
            var destDir = builderOptions.DestTemplatePath;
            if (Directory.Exists(destDir))
            {
                Directory.Delete(destDir, true);
            }
            
            _directoryService.DirectoryCopy(@"C:\Source\Repos\party-model", destDir, true);
            //FilesFindAndDelete(destDir, "PartyType", true);
            //FilesFindAndDelete(destDir, "RelationshipType", true);
            var projectName = "Company.Link";

            _findAndRenameService.DirectoryFindAndRename(destDir, "Company.PartyModel", projectName, true);
            _findAndRenameService.DirectoryFindAndRename(destDir, "PartyModel", "ProgramScheduleStations", true);

            _findAndRenameService.FilesFindAndRename(destDir, "Company.PartyModel", projectName, true);
            _findAndRenameService.FilesFindAndRename(destDir, "PartyModel", "ProgramScheduleStations", true);

            _findAndRenameService.FilesFindAndRename(destDir, "Parties", "ProgramSchedules", true);


            _findAndRenameService.FilesFindAndRename(destDir, "PartyFrom", "Station", true);

            _findAndRenameService.FilesFindAndRename(destDir, "PartyTo", "ProgramSchedule", true);

            _findAndRenameService.FilesFindAndRename(destDir, "PartyType", "ProgramScheduleType", true);

            //FilesFindAndRename(destDir, "party", "program_schedule", true);

            _findAndReplaceService.FileContentFindAndReplace(destDir, "Company.PartyModel", projectName, true);

            _findAndReplaceService.FileContentFindAndReplace(destDir, "PartyModel", "ProgramScheduleStations", true);
            _findAndReplaceService.FileContentFindAndReplace(destDir, "PARTYMODEL", "PROGRAMSCHEDULESTATIONS", true);


            _findAndReplaceService.FileContentFindAndReplace(destDir, "PartyFrom", "Station", true);

            _findAndReplaceService.FileContentFindAndReplace(destDir, "PartyTo", "ProgramSchedule", true);

            _findAndReplaceService.FileContentFindAndReplace(destDir, "PartyFromExternal", "StationExternal", true);
            _findAndReplaceService.FileContentFindAndReplace(destDir, "PartyToExternal", "ProgramScheduleExternal", true);

            _findAndReplaceService.FileContentFindAndReplace(destDir, "Parties", "ProgramSchedules", true);
            _findAndReplaceService.FileContentFindAndReplace(destDir, "parties", "programSchedules", true, new[] { "sql", "esql" });

            _findAndReplaceService.FileContentFindAndReplace(destDir, "PartyType", "ProgramScheduleType", true);

            //FileContentFindAndReplace(destDir, "Relationship", "", true);

            _findAndReplaceService.FileContentFindAndReplace(destDir, "party", "program_schedule", true, null, new string[] { "sql", "esql" });

        }
    }
}
