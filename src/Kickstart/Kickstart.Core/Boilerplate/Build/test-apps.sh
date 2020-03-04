#!/bin/bash

baseDir="$1"
configuration="$2"
outDir="$3"

bye() {
	result=$?
	if [ "$result" != "0" ]; then
		echo "Testing failed"
	fi
	chown -R 1000:1000 $outDir $baseDir
	exit $result
}

testApp() {
	appPath=$1
	logFileName=$outDir/TestResults/$2.trx
	dotnet test --logger "trx;LogFileName=$logFileName" -f netcoreapp2.2 -c $configuration $appPath
}

fixPermissions() {
    chown -R 1000:1000 $outDir $baseDir
}

#Stop execution on any error
trap "bye" EXIT

set -e
echo Testing apps
testApp "$baseDir/##CompanyName##.##ProjectName##.Services.Tests/##CompanyName##.##ProjectName##.Services.Tests.csproj" "##ProjectName##ServiceTests"

# fix permissions on new files as otherwise only root will be able to access them
fixPermissions
