using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Common.Log;
using Lykke.Common.Entities.Wallets;
using Lykke.Job.BitcoinTransactionAggregator.Core;
using Lykke.Job.BitcoinTransactionAggregator.Core.Services;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Newtonsoft.Json;

namespace Lykke.Job.BitcoinTransactionAggregator.Services
{
    public class BitcoinBroadcast : IBitcoinBroadcast, IStartable
    {
        private readonly ILog _log;
        private readonly AppSettings.BitcoinTransactionAggregatorSettings _settings;
        private  RabbitMqPublisher <WalletMqModel> _publisher;

        public BitcoinBroadcast(AppSettings.BitcoinTransactionAggregatorSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;
        }

        public async Task BroadcastMessage(WalletMqModel wallets)
        {
            if (wallets != null)
            {
                await _publisher.ProduceAsync(wallets);
            }
        }

        public void Start()
        {
            _publisher = new RabbitMqPublisher<WalletMqModel>(new RabbitMqPublisherSettings
            {
                ConnectionString = _settings.WalletBroadcastRabbit.ConnectionString,
                ExchangeName = _settings.WalletBroadcastRabbit.ExchangeName
            }).SetPublishStrategy(new DefaultFnoutPublishStrategy("", true))
                .SetSerializer(new WalletBradcastSerializer())
                .SetLogger(_log);
        }


    }

    public class WalletBradcastSerializer : IRabbitMqSerializer<WalletMqModel>
    {
        public byte[] Serialize(WalletMqModel model)
        {
            string json = JsonConvert.SerializeObject(model);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
