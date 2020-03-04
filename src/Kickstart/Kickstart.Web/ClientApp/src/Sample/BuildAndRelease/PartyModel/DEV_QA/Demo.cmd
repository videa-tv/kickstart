set curdir=%cd%

rem grpc-client-cli  -service company.KickstartBuild.services.types.KickstartBuildService -method CreateBuildDefinition -i CreateBuildDefinition.json  -d 60 localhost:50095

rem grpc-client-cli  -service company.KickstartBuild.services.types.KickstartBuildService -method QueueBuild -i QueueBuild.json   -d 60 localhost:50095

grpc-client-cli  -service company.KickstartBuild.services.types.KickstartBuildService -method CreateReleaseDefinition -i CreateReleaseDefinition.json  -d 60 localhost:50095

grpc-client-cli  -service company.KickstartBuild.services.types.KickstartBuildService -method CreateRelease -i CreateRelease.json  -d 60 localhost:50095

rem grpc-client-cli  -service company.KickstartBuild.services.types.KickstartBuildService -method DeployRelease -i DeployRelease.json  -d 60 localhost:50095






