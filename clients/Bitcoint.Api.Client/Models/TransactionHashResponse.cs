// Code generated by Microsoft (R) AutoRest Code Generator 1.1.0.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace Bitcoint.Api.Client.Models
{
    using Bitcoint.Api;
    using Bitcoint.Api.Client;
    using Newtonsoft.Json;
    using System.Linq;

    public partial class TransactionHashResponse
    {
        /// <summary>
        /// Initializes a new instance of the TransactionHashResponse class.
        /// </summary>
        public TransactionHashResponse()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the TransactionHashResponse class.
        /// </summary>
        public TransactionHashResponse(string transactionHash = default(string))
        {
            TransactionHash = transactionHash;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "transactionHash")]
        public string TransactionHash { get; set; }

    }
}
