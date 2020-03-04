FROM registry.company.tv/dotnet:2.2.1-runtime

ENV LANG en_US.UTF-8
ENV LANGUAGE en_US.UTF-8

ARG GIT_COMMIT=unkown
ARG BUILD_NUMBER=unkown

LABEL git-commit=$GIT_COMMIT \
	  build-number=$BUILD_NUMBER

ARG source=.
WORKDIR /app
COPY $source .

ENTRYPOINT ["dotnet", "Kickstart.Build.Services.dll"]
