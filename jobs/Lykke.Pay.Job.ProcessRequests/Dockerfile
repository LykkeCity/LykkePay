FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "Lykke.Pay.Job.ProcessRequests.dll"]
ARG source=.
WORKDIR /app
COPY $source .