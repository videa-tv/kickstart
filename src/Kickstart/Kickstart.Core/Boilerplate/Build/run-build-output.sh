#!/bin/bash

cwd=$(dirname "$(realpath $0)")
binariesPath=$1
buildId=$2

docker run --rm \
  -e buildId=$buildId \
  -e BUILD_SOURCEVERSION="$BUILD_SOURCEVERSION" \
  -e BUILD_REPOSITORY_URI="$BUILD_REPOSITORY_URI" \
  -e BUILD_BUILDURI="$BUILD_BUILDURI" \
  -v $(pwd):/src \
  -v $binariesPath:/output \
  registry.##companyname##.tv/dotnet-build:2.6.0 /src/build/build-output.sh

