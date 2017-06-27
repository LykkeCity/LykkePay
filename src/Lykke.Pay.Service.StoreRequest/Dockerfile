FROM microsoft/aspnetcore:1.1.2
ENTRYPOINT ["dotnet", "Lykke.Pay.Service.StoreRequest.dll"]
ARG source=.
WORKDIR /app
COPY $source .