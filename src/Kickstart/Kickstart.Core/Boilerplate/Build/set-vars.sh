#!/bin/bash

path=$1

versionService=$(head -n 1 $path/Service/version.txt)
echo "##vso[task.setvariable variable=serviceImageName]##projectname##-svc:$versionService"
