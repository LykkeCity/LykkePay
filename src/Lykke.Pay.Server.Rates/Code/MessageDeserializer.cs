using System.Text;
using Lykke.Common.Entities.Pay;
using Newtonsoft.Json;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Pay.Service.Rates.Code
{
    public class MessageDeserializer : IMessageDeserializer<PairRate>
    {
        public PairRate Deserialize(byte[] data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                 DateTimeZoneHandling = DateTimeZoneHandling.Utc // treat datetime as Utc
            };

            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<PairRate>(json, settings);
        }
    }
}
