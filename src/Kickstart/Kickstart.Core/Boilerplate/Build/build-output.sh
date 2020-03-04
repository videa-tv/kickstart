#!/bin/bash

cwd=$(dirname "$(realpath $0)")

path=/output
dropPath=$path/drop

versionWeb=$(head -n 1 $path/Web/version.txt)
versionService=$(head -n 1 $path/Service/version.txt)
pgWegImage=registry.##companyname##.tv/##projectname##-web:$versionWeb-$buildId
pgServicesImage=registry.##companyname##.tv/##projectname##-service:$versionService-$buildId

echo "Write build-output.json to $dropPath/build-output.json"
echo "BUILD_SOURCEVERSION: $BUILD_SOURCEVERSION"
echo "BUILD_REPOSITORY_URI: $BUILD_REPOSITORY_URI"
echo "BUILD_BUILDURI: $BUILD_BUILDURI"

mkdir $dropPath

cat <<EOF > $dropPath/build-output.json
{
    "buildUrl": "http://tfs.##companyname##.int/tfs/##CompanyName##/##CompanyName##%20Git/_build/index?buildId=$buildId",
    "sourceVersion": "$BUILD_SOURCEVERSION",
    "branch": "$BUILD_SOURCEBRANCH",
    "images": {
        "##projectname##-web": "$pgWegImage",
        "##projectname##-service": "$pgServicesImage"
    }
}
EOF

export imageNameWeb=$pgWegImage
envsubst '$imageNameWeb' < $cwd/##projectname##web.json > $dropPath/##projectname##web.json

export imageNameService=$pgServicesImage
envsubst '$imageNameService' < $cwd/##projectname##service.json > $dropPath/##projectname##service.json

chown -R 1000:1000 /output

