﻿namespace Lykke.Pay.Common
{
    public enum TransferError
    {
        TRANSACTION_NOT_CONFIRMED,
        INVALID_AMOUNT,
        INVALID_ADDRESS,
        INTERNAL_ERROR
    }

    public enum PaymentError
    {
        TRANSACTION_NOT_CONFIRMED,
        TRANSACTION_NOT_DETECTED,
        AMOUNT_BELOW,
        AMOUNT_ABOVE,
        PAYMENT_EXPIRED

    }
}