FROM microsoft/dotnet:2.0.5-runtime

ENV LANG en_US.UTF-8
ENV LANGUAGE en_US.UTF-8

ENTRYPOINT ["dotnet", "Company.Kickstart.Services.dll"]
ARG source=.
WORKDIR /app
COPY $source .
