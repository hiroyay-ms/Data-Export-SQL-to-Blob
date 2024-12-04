FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /api

COPY ./bin/Publish .

ENTRYPOINT [ "dotnet", "WebApiProj.dll" ]