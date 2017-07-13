using System.Threading.Tasks;

namespace Lykke.Job.BitcoinTransactionAggregator.Core.Services
{
    // NOTE: This is job service interface example
    public interface IBitcoitBlocksHandler
    {
        Task ProcessAsync();
    }
}