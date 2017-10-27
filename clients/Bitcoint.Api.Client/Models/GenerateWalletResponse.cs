// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace Bitcoint.Api.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    public partial class GenerateWalletResponse
    {
        /// <summary>
        /// Initializes a new instance of the GenerateWalletResponse class.
        /// </summary>
        public GenerateWalletResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the GenerateWalletResponse class.
        /// </summary>
        public GenerateWalletResponse(string address = default(string), string pubKey = default(string), string tag = default(string))
        {
            Address = address;
            PubKey = pubKey;
            Tag = tag;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "pubKey")]
        public string PubKey { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "tag")]
        public string Tag { get; set; }

    }
}
