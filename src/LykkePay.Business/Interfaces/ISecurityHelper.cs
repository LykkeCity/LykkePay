namespace LykkePay.Business.Interfaces
{
    public interface ISecurityHelper
    {
        SecurityErrorType CheckRequest(BaseRequest request);

        SecurityErrorType CheckRequest(string strToSign, string merchantId, string sign);
    }
}
