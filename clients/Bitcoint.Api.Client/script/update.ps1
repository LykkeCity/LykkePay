cd ../
iwr http://13.93.116.252/swagger/v1/swagger.json -o Bitcoint.Api.Client.json
autorest --input-file=Bitcoint.Api.Client.json --csharp --namespace=Bitcoint.Api.Client --output-folder=./