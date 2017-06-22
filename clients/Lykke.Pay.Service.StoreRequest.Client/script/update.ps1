cd ../
iwr https://storerequest-dev.lykkex.net/swagger/v1/swagger.json -o StoreRequest.json
autorest --input-file=MarketProfileService.json --csharp --namespace=Lykke.Pay.Service.StoreRequest.Client --output-folder=./