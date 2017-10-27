// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Bitcoint.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class CommitmentBroadcastInfo
    {
        /// <summary>
        /// Initializes a new instance of the CommitmentBroadcastInfo class.
        /// </summary>
        public CommitmentBroadcastInfo()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the CommitmentBroadcastInfo class.
        /// </summary>
        /// <param name="type">Possible values include: 'Valid',
        /// 'Revoked'</param>
        public CommitmentBroadcastInfo(System.Guid? commitmentId = default(System.Guid?), string transactionHash = default(string), System.DateTime? date = default(System.DateTime?), string type = default(string), decimal? clientAmount = default(decimal?), decimal? hubAmount = default(decimal?), decimal? realClientAmount = default(decimal?), decimal? realHubAmount = default(decimal?), string penaltyTransactionHash = default(string))
        {
            CommitmentId = commitmentId;
            TransactionHash = transactionHash;
            Date = date;
            Type = type;
            ClientAmount = clientAmount;
            HubAmount = hubAmount;
            RealClientAmount = realClientAmount;
            RealHubAmount = realHubAmount;
            PenaltyTransactionHash = penaltyTransactionHash;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "commitmentId")]
        public System.Guid? CommitmentId { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "transactionHash")]
        public string TransactionHash { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "date")]
        public System.DateTime? Date { get; set; }

        /// <summary>
        /// Gets or sets possible values include: 'Valid', 'Revoked'
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "clientAmount")]
        public decimal? ClientAmount { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "hubAmount")]
        public decimal? HubAmount { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "realClientAmount")]
        public decimal? RealClientAmount { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "realHubAmount")]
        public decimal? RealHubAmount { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "penaltyTransactionHash")]
        public string PenaltyTransactionHash { get; set; }

    }
}
