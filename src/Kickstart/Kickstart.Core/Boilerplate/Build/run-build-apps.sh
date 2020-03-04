#!/bin/bash

outDir=$1
echo "sources dir: $(pwd)"
echo "output dir: $outDir"
echo "nuget cache dir: $(readlink -f ~/.nuget)"
echo "npm cache dir: $(readlink -f ~/.npm)"
echo "yarn cache dir: $(readlink -f ~/.cache)"

docker run --rm \
    -v $(pwd):/src \
    -v $outDir:/output \
    -v "$(readlink -f ~/.nuget)":/root/.nuget \
    -v "$(readlink -f ~/.npm)":/root/.npm \
    -v "$(readlink -f ~/.cache)":/root/.cache \
    registry.##companyname##.tv/dotnet-build:2.6.0 /src/build/build-apps.sh "/src/src" "Release" "/output"
