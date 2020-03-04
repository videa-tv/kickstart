#!/bin/bash

baseDir="$1"
configuration="$2"
outDir="$3"

fixPermissions() {
    chown -R 1000:1000 $outDir $baseDir
}

bye() {
	result=$?
	if [ "$result" != "0" ]; then
		echo "Build failed"
	fi
	fixPermissions
	exit $result
}

buildApp() {
	appPath=$1
	dotnet publish -c $configuration -f netcoreapp2.2 -o "$outDir/$2" $appPath
}

#Stop execution on any error
trap "bye" EXIT

set -e
echo Building apps

webappPath="$baseDir/##CompanyName##.##ProjectName##.Services"
buildApp "$webappPath/##CompanyName##.##ProjectName##.Services.csproj" "Service"

echo Copy database deployment scripts
test -d "$outDir/drop/##CompanyName##.##ProjectName##.Db" || mkdir -p "$outDir/drop/##CompanyName##.##ProjectName##.Db"
cp -r $baseDir/##CompanyName##.##ProjectName##.Db/migrations $outDir/drop/##CompanyName##.##ProjectName##.Db

# fix permissions on new files as otherwise only root will be able to access them
fixPermissions
