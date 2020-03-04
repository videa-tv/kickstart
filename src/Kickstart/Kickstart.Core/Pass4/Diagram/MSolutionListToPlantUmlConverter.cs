using System.Collections.Generic;
using System.IO;
using System.Linq;
using Kickstart.Pass1.KModel;
using Kickstart.Pass2.CModel;
using Kickstart.Pass2.CModel.Code;
using Kickstart.Pass2.CModel.Proto;
using Kickstart.Pass2.GrpcServiceProject;
using Kickstart.Utility;

namespace Kickstart.Pass4.Diagram
{
    public class MSolutionListToPlantUmlConverter
    {
        public string PlantUml { get; set; }

        public CPlantUml Convert(List<KSolutionGroup> solutionGroups, bool addGroups = false)
        {
            var codeWriter = new CodeWriter();
            codeWriter.WriteLine("@startuml");
            var index = 1;
            foreach (var solutionGroup in solutionGroups)
            {
                if (addGroups)
                {
                    codeWriter.WriteLine($@"rectangle ""{solutionGroup.SolutionGroupName}""{{");
                    codeWriter.Indent();
                }
                foreach (var solutionM in solutionGroup.Solution)
                {
                    var solution = solutionM.GeneratedSolution;
                    codeWriter.WriteLine($@"package ""{solution.SolutionName}""{{");

                    codeWriter.Indent();

                    foreach (var project in solution.Project)
                        if (project.ProjectIs.HasFlag(CProjectIs.DataBase))
                        {
                            project.ProjectID = $"DB{index}";
                            codeWriter.WriteLine($@"database ""{project.ProjectShortName}"" as {project.ProjectID}");
                        }
                        else if (project.ProjectIs.HasFlag(CProjectIs.DataAccess))
                        {
                            project.ProjectID = $"DA{index}";
                            codeWriter.WriteLine(
                                $@"component ""{project.ProjectShortName}"" <<DAL>> as {project.ProjectID}");
                        }
                        else if (project.ProjectIs.HasFlag(CProjectIs.Grpc))
                        {
                            project.ProjectID = $"SVC{index}";
                            codeWriter.WriteLine(
                                $@"component ""{project.ProjectShortName}"" <<SVC>> as {project.ProjectID}");
                        }
                        else if (project.ProjectIs.HasFlag(CProjectIs.Integration) &&
                                 project.ProjectIs.HasFlag(CProjectIs.Client))
                        {
                            //SKIP
                        }
                    codeWriter.WriteLine(string.Empty);
                    //draw arrow from data access to DB
                    var dbProject = solution.Project.FirstOrDefault(p => p.ProjectIs.HasFlag(CProjectIs.DataBase));
                    var dataAccessProject =
                        solution.Project.FirstOrDefault(p => p.ProjectIs.HasFlag(CProjectIs.DataAccess));
                    var serviceProject = solution.Project.FirstOrDefault(p => p.ProjectIs.HasFlag(CProjectIs.Grpc));

                    //draw arrows between the projects in the solution
                    if (dbProject != null && dataAccessProject != null)
                        codeWriter.WriteLine($"{dataAccessProject.ProjectID} -[#green]down->  {dbProject.ProjectID}");
                    if (dataAccessProject != null && serviceProject != null)
                        codeWriter.WriteLine(
                            $"{serviceProject.ProjectID} -[#green]down->  {dataAccessProject.ProjectID}");
                    codeWriter.Unindent();
                    codeWriter.WriteLine("}");
                    index++;
                }


                //draw arrows between the services, cross solution, using the proto file ref

                if (addGroups)
                {
                    codeWriter.Unindent();
                    codeWriter.WriteLine("}");
                }
            }


            //add test clients to their own Group
            foreach (var solutionGroup in solutionGroups)
            foreach (var solutionM in solutionGroup.Solution)
            {
                var solution = solutionM.GeneratedSolution;
                var foundOne = false;
                var testGroupCodeWriter = new CodeWriter();
                if (addGroups)
                {
                    testGroupCodeWriter.WriteLine($@"rectangle ""Client {index}""{{");
                    testGroupCodeWriter.Indent();
                }
                foreach (var project in solution.Project)
                    if (project.ProjectIs.HasFlag(CProjectIs.Integration) &&
                        project.ProjectIs.HasFlag(CProjectIs.Client))
                    {
                        foundOne = true;
                        project.ProjectID = $"CLIENT{index}";
                        testGroupCodeWriter.WriteLine(
                            $@"component ""{project.ProjectShortName}"" <<CLIENT>> as {project.ProjectID}");
                    }
                if (addGroups)
                {
                    testGroupCodeWriter.Unindent();
                    testGroupCodeWriter.WriteLine("}");
                }
                if (foundOne)
                    codeWriter.WriteLine(testGroupCodeWriter.ToString());

                index++;
            }

            //draw arrows between the services, cross solution, using the proto file ref

            foreach (var solutionGroup2 in solutionGroups)
            foreach (var solution2 in solutionGroup2.Solution)
            foreach (var kProject in solution2.Project.Where(p => p is KGrpcIntegrationProject)
                .Select(p => p as KGrpcIntegrationProject))
            foreach (var protoRpcRef in kProject.ProtoRef)
            {
                var integrationServiceProject =
                    solution2.GeneratedSolution.Project.FirstOrDefault(p =>
                        p.ProjectIs.HasFlag(CProjectIs.Integration));

                var refProject = GetRefProject(solutionGroups, protoRpcRef);
                if (protoRpcRef.Direction.HasFlag(CProtoRpcRefDataDirection.Push))
                    codeWriter.WriteLine(
                        $"{integrationServiceProject.ProjectID} -> {refProject.ProjectID} : {FixupRpcName(protoRpcRef.RefRpcName)}()");
                else if (protoRpcRef.Direction.HasFlag(CProtoRpcRefDataDirection.Pull))
                    codeWriter.WriteLine(
                        $"{refProject.ProjectID} -[#blue]-> {integrationServiceProject.ProjectID} : {FixupRpcName(protoRpcRef.RefRpcName)}()");
                else if (protoRpcRef.Direction.HasFlag(CProtoRpcRefDataDirection.Trigger))
                    codeWriter.WriteLine(
                        $"{refProject.ProjectID} -[#pink]-> {integrationServiceProject.ProjectID} : {FixupRpcName(protoRpcRef.RefRpcName)}()");
                else if (protoRpcRef.Direction.HasFlag(CProtoRpcRefDataDirection.Undefined))
                    codeWriter.WriteLine(
                        $"'{refProject.ProjectID} -[#purple]-> {integrationServiceProject.ProjectID} : {FixupRpcName(protoRpcRef.RefRpcName)}()");
            }
            /*
                    foreach (var solutionGroup2 in solutionGroups)
                    {
                        foreach (var solution2 in solutionGroup2.Solution)
                        {
                            //included any Test Project that calls an external grpc 
                            foreach (var integrationServiceProject in solution2.GeneratedSolution.Project.Where(p => p.ProjectIs.HasFlag(SProjectIs.Integration)))
                            {
                                if (integrationServiceProject != null)
                                {

                                    foreach (var protoRpcRef in integrationServiceProject.GetProtoRpcRefs())
                                    {
                                        if (protoRpcRef.ProtoRpc.ProtoService.ProtoFile.OwnedByProject != integrationServiceProject)
                                        {
                                            //if (!string.IsNullOrEmpty(protoFileRef.ProtoFile.OwnedByProject.ProjectID) && !string.IsNullOrEmpty(integrationServiceProject.ProjectID))
                                            {
                                                if (protoRpcRef.Direction.HasFlag(SProtoRpcRefDataDirection.Push))
                                                {
                                                    //-0)->
                                                    codeWriter.WriteLine($"{integrationServiceProject.ProjectID} -> {protoRpcRef.ProtoRpc.ProtoService.ProtoFile.OwnedByProject.ProjectID} : {FixupRpcName(protoRpcRef.ProtoRpc.RpcName)}()");
                                                }
                                                else if (protoRpcRef.Direction.HasFlag(SProtoRpcRefDataDirection.Pull))
                                                {

                                                    codeWriter.WriteLine($"{protoRpcRef.ProtoRpc.ProtoService.ProtoFile.OwnedByProject.ProjectID} -[#blue]-> {integrationServiceProject.ProjectID} : {FixupRpcName(protoRpcRef.ProtoRpc.RpcName)}()");

                                                }
                                                else if (protoRpcRef.Direction.HasFlag(SProtoRpcRefDataDirection.Trigger))
                                                {

                                                    codeWriter.WriteLine($"{protoRpcRef.ProtoRpc.ProtoService.ProtoFile.OwnedByProject.ProjectID} -[#pink]-> {integrationServiceProject.ProjectID} : {FixupRpcName(protoRpcRef.ProtoRpc.RpcName)}()");

                                                }
                                                else if (protoRpcRef.Direction.HasFlag(SProtoRpcRefDataDirection.Undefined))
                                                {

                                                    codeWriter.WriteLine($"'{protoRpcRef.ProtoRpc.ProtoService.ProtoFile.OwnedByProject.ProjectID} -[#purple]-> {integrationServiceProject.ProjectID} : {FixupRpcName(protoRpcRef.ProtoRpc.RpcName)}()");

                                                }
                                            }
                                        }
                                    }
                                }
                            }


                        }
                    }*/
            codeWriter.WriteLine("@enduml");

            PlantUml = codeWriter.ToString();
            return new CPlantUml {Text = codeWriter.ToString()};
        }

        private CProject GetRefProject(List<KSolutionGroup> solutionGroups, KProtoRef protoRpcRef)
        {
            foreach (var proj in protoRpcRef.RefSolution.GeneratedSolution.Project)
            {
                var protoFile = proj.GetProtoFile(protoRpcRef.RefServiceName);
                if (protoFile != null)
                    return proj;
            }
            return null;
        }


        private string FixupRpcName(string rpcName)
        {
            if (rpcName.StartsWith("Get"))
                rpcName = rpcName.Replace("Get", "Get\\n");
            if (rpcName.StartsWith("Is"))
                rpcName = rpcName.Replace("Is", "Is\\n");

            if (rpcName.StartsWith("Check"))
                rpcName = rpcName.Replace("Check", "Check\\n");
            if (rpcName.StartsWith("Create"))
                rpcName = rpcName.Replace("Create", "Create\\n");
            if (rpcName.StartsWith("Update"))
                rpcName = rpcName.Replace("Update", "Update\\n");
            if (rpcName.StartsWith("Dequeue"))
                rpcName = rpcName.Replace("Dequeue", "Dequeue\\n");

            if (rpcName.StartsWith("Read"))
                rpcName = rpcName.Replace("Read", "Read\\n");

            if (rpcName.StartsWith("Bulk"))
                rpcName = rpcName.Replace("Bulk", "Bulk\\n");

            if (rpcName.StartsWith("Save"))
                rpcName = rpcName.Replace("Save", "Save\\n");
            if (rpcName.StartsWith("Commit"))
                rpcName = rpcName.Replace("Commit", "Commit\\n");
            if (rpcName.Contains("Rate"))
                rpcName = rpcName.Replace("Rate", "Rate\\n");
            return rpcName;
        }

        public void Save(string outputPath)
        {
            var path = Path.GetDirectoryName(outputPath);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText(outputPath, PlantUml);
        }
    }
}