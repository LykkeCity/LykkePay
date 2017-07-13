using System;
using System.Threading.Tasks;
using Lykke.Job.BitcoinTransactionAggregator.Core.Services;
using Lykke.JobTriggers.Triggers.Attributes;

namespace Lykke.Job.BitcoinTransactionAggregator.TriggerHandlers
{
    // NOTE: This is the trigger handlers class example
    public class GeneralHandlers
    {
        private readonly IBitcoitBlocksHandler _bitcoitBlocksHandler;
        private readonly IHealthService _healthService;

        // NOTE: The object is instantiated using DI container, so registered dependencies are injects well
        public GeneralHandlers(IBitcoitBlocksHandler bitcoitBlocksHandler, IHealthService healthService)
        {
            _bitcoitBlocksHandler = bitcoitBlocksHandler;
            _healthService = healthService;
        }


        [TimerTrigger("00:00:10")]
        public async Task TimeTriggeredHandler()
        {
            try
            {

                _healthService.TraceBbServiceStarted();

                await _bitcoitBlocksHandler.ProcessAsync();

                _healthService.TraceBbServiceCompleted();
            }
            catch(Exception e)
            {
                _healthService.TraceBbServiceFailed();
            }

        }

       
    }
}