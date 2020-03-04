#!/bin/bash

outDir=$1
echo "sources dir: $(pwd)"
echo "output dir: $outDir"
echo "nuget cache dir: $(readlink -f ~/.nuget)"

docker run --rm -v $(pwd):/src -v $outDir:/output -v "$(readlink -f ~/.nuget)":/root/.nuget registry.##companyname##.tv/dotnet-build:2.6.0 /src/build/test-apps.sh "/src/test" "Release" "/output"
