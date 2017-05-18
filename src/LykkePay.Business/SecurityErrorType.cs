namespace LykkePay.Business
{
    public enum SecurityErrorType
    {
        Ok,
        MerchantUnknown,
        SignEmpty,
        SignIncorrect,
        OutOfDate
    }
}
