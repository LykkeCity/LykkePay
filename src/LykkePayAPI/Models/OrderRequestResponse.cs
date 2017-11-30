using System;
using LykkePay.API.Code;

namespace LykkePay.API.Models
{
    public class OrderRequestResponse
    {
        public string Timestamp { get; set; }
        public string Address { get; set; }
        public string OrderId { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public double RecommendedFee { get; set; }
        public double TotalAmount { get; set; }
        public double ExchangeRate { get; set; }



        public OrderRequestResponse(Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest request, double exchangeRate)
        {

            Timestamp = request.TransactionDetectionTime.ToUnixFormat();
            Address = request.SourceAddress;
            OrderId = request.OrderId;
            Currency = request.ExchangeAssetId;
            Amount = request.Amount ?? 0;
            RecommendedFee = 0;
            TotalAmount = Amount + RecommendedFee;
            ExchangeRate = exchangeRate;
        }
    }
}