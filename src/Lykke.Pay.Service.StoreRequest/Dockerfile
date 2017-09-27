FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "Lykke.Pay.Service.StoreRequest.dll"]
ARG source=.
WORKDIR /app
COPY $source .