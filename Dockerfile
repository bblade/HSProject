FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY HSProject/bin/Release/net8.0/publish/ ./
EXPOSE 5200
ENTRYPOINT ["dotnet", "HSProject.dll"]