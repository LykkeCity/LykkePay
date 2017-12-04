using System.Runtime.Serialization;

namespace LykkePay.API.Models
{
    [DataContract]
    public class InvoiceLink
    {
        [DataMember(Name = "rel")]
        public string Reltion { get; set; }
        [DataMember(Name = "link")]
        public string Link { get; set; }
        [DataMember(Name = "method")]
        public string Method { get; set; }
    }
}