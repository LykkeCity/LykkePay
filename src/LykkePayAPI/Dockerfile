FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "LykkePay.API.dll"]
ARG source=.
WORKDIR /app
COPY $source .
