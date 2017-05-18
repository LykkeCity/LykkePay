namespace LykkePay.Business.Interfaces
{
    public interface ISecurityHelper
    {
        SecurityErrorType CheckRequest(BaseRequest request);
    }
}
