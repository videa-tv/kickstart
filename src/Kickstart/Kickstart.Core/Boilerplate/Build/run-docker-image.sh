#!/bin/bash
cwd=$(dirname "$(realpath $0)")
path=$1
buildId=$2
canpush="$3"

source "$cwd/docker-image.sh"

versionWeb=$(head -n 1 $path/Web/version.txt)
versionService=$(head -n 1 $path/Service/version.txt)
imageNameWeb=registry.##companyname##.tv/##projectname##-web:$versionWeb-$buildId
imageNameService=registry.##companyname##.tv/##projectname##-service:$versionService-$buildId

build-docker-image $path/Web $imageNameWeb $canpush
build-docker-image $path/Service $imageNameService $canpush