using System;
using System.Globalization;
using Common.Log;
using Lykke.Core;
using Lykke.Pay.Common;
using PayFee = Lykke.Pay.Service.StoreRequest.Client.Models.PayFee;

namespace LykkePay.API.Models
{
    public class OrderRequest
    {
        private readonly ILog _log;
        public OrderRequest(ILog log)
        {
            _log = log;
        }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string ExchangeCurrency { get; set; }
        public string SuccessUrl { get; set; }
        public string ErrorUrl { get; set; }
        public string ProgressUrl { get; set; }
        public string OrderId { get; set; }
        public Markup Markup { get; set; }

        public Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest GetRequest(string merchantId)
        {
            if (string.IsNullOrEmpty(Currency) || string.IsNullOrEmpty(Amount))
            {
                return null;
            }

            if (string.IsNullOrEmpty(ExchangeCurrency))
            {
                ExchangeCurrency = "BTC";
            }

                try
            {
                var provider = CultureInfo.InvariantCulture;
                return new Lykke.Pay.Service.StoreRequest.Client.Models.OrderRequest
                {
                    MerchantId = merchantId,
                    Amount = Math.Round(double.Parse(Amount, provider), 8),
                    AssetPair = $"{ExchangeCurrency}{Currency}",
                    AssetId = Currency,
                    ExchangeAssetId = ExchangeCurrency,
                    OrderId = OrderId,
                    Markup = Markup == null ? new PayFee(0, 0, 0) : new PayFee
                    {
                        FixedFee = Markup.FixedFee,
                        Percent = Markup.Percent,
                        Pips = Markup.Pips
                    },
                    SuccessUrl = SuccessUrl,
                    ErrorUrl = ErrorUrl,
                    ProgressUrl = ProgressUrl,
                    MerchantPayRequestNotification = MerchantPayRequestNotification.Nothing.ToString(),
                    MerchantPayRequestStatus = MerchantPayRequestStatus.New.ToString(),
                    RequestId = Guid.NewGuid().ToString(),
                    TransactionWaitingTime = DateTime.Now.AddMinutes(11).RepoDateStr()

                };

            }
            catch (Exception e)
            {
                _log.WriteErrorAsync(nameof(OrderRequest), nameof(GetRequest), e).GetAwaiter().GetResult();
                return null;
            }

            
        }
    }
}