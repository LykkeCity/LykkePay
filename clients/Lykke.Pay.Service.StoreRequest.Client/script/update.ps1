cd ../
iwr http://localhost:3006/swagger/v1/swagger.json -o StoreRequest.json
autorest --input-file=StoreRequest.json --csharp --namespace=Lykke.Pay.Service.StoreRequest.Client --output-folder=./