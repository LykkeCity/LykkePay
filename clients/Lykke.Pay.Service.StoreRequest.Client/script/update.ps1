cd ../
iwr http://pay-store-request.lykke-pay.svc.cluster.local/swagger/v1/swagger.json -o StoreRequest.json
autorest --input-file=StoreRequest.json --csharp --namespace=Lykke.Pay.Service.StoreRequest.Client --output-folder=./