cd ../
iwr https://paygenaddr-dev.lykkex.net/swagger/v1/swagger.json -o GenAddress.json
autorest --input-file=GenAddress.json --csharp --namespace=Lykke.Pay.Service.GenerateAddress.Client --output-folder=./