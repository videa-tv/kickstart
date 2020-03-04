#!/bin/bash

platform='unknown'
unamestr=$(uname)
if [[ "$unamestr" == "Linux" ]]; then
   platform='linux_x64'
elif [[ "$unamestr" == 'Darwin' ]]; then
   platform='macosx_x64'
fi

GRPC_TOOLS=~/.nuget/packages/grpc.tools/1.18.0/tools/$platform
PROTOBUF_TOOLS=~/.nuget/packages/google.protobuf.tools/3.6.1/tools

IN=Proto
OUTPUT=Proto/Generated

mkdir -p $OUTPUT

$GRPC_TOOLS/protoc -I./$IN:$PROTOBUF_TOOLS --csharp_out $OUTPUT $IN/*.proto --grpc_out $OUTPUT --plugin=protoc-gen-grpc=$GRPC_TOOLS/grpc_csharp_plugin
