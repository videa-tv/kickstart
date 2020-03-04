@rem enter this directory

chdir Proto
mkdir Generated

cd /d %~dp0


SET GRPC_TOOLS=%USERPROFILE%\.nuget\packages\Grpc.Tools\1.18.0\tools\windows_x64\
SET PROTOBUF_TOOLS=%USERPROFILE%\.nuget\packages\Google.Protobuf.Tools\3.6.1\tools

SET IN=Proto
SET OUTPUT=Proto/Generated

FOR /f "tokens=*" %%G IN ('dir %IN% /b ^| findstr proto') DO %GRPC_TOOLS%\protoc -I.\%IN%\;%PROTOBUF_TOOLS% --csharp_out %OUTPUT% %IN%/%%G --grpc_out %OUTPUT% --plugin=protoc-gen-grpc=%GRPC_TOOLS%\grpc_csharp_plugin.exe
FOR /f "tokens=*" %%G IN ('dir %IN% /b ^| findstr proto') DO %GRPC_TOOLS%\protoc -I.\%IN%\;%PROTOBUF_TOOLS%  --descriptor_set_out=%OUTPUT% %IN%/%%G.dso

rem pause
