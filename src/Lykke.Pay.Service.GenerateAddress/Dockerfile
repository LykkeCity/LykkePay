FROM microsoft/aspnetcore:1.1.2
ENTRYPOINT ["dotnet", "Lykke.Pay.Service.GenerateAddress.dll"]
ARG source=.
WORKDIR /app
COPY $source .