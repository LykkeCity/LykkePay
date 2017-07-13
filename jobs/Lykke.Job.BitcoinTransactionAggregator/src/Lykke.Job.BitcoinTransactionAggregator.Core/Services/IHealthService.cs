using System;

namespace Lykke.Job.BitcoinTransactionAggregator.Core.Services
{
    public interface IHealthService
    {
        // NOTE: These are example properties
        DateTime LastBbServiceStartedMoment { get; }
        TimeSpan LastBbServiceDuration { get; }
        TimeSpan MaxHealthyBbServiceDuration { get; }

        // NOTE: This method probably would stay in the real job, but will be modified
        string GetHealthViolationMessage();

        // NOTE: These are example methods
        void TraceBbServiceStarted();
        void TraceBbServiceCompleted();
        void TraceBbServiceFailed();
       
    }
}