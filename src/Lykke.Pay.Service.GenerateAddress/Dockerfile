FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "Lykke.Pay.Service.GenerateAddress.dll"]
ARG source=.
WORKDIR /app
COPY $source .