using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Lykke.Common.Entities.Wallets;

namespace Lykke.Job.BitcoinTransactionAggregator.Core.Services
{
    public interface IBitcoinBroadcast
    {
        Task BroadcastMessage(WalletMqModel wallets);

    }
}
