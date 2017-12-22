namespace Lykke.Pay.Common
{
    public enum OrderError
    {
        TRANSACTION_NOT_CONFIRMED,
        TRANSACTION_NOT_DETECTED,
        AMOUNT_BELOW,
        AMOUNT_ABOVE,
        PAYMENT_EXPIRED
    }
}