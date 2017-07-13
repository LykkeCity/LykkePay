using System;
using Lykke.Job.BitcoinTransactionAggregator.Core.Services;

namespace Lykke.Job.BitcoinTransactionAggregator.Services
{
    public class HealthService : IHealthService
    {
        // NOTE: These are example properties
        public DateTime LastBbServiceStartedMoment { get; private set; }
        public TimeSpan LastBbServiceDuration { get; private set; }
        public TimeSpan MaxHealthyBbServiceDuration { get; }

        // NOTE: These are example properties
        private bool WasLastBbServiceFailed { get; set; }
        private bool WasLastBbServiceCompleted { get; set; }
        private bool WasClientsBbServiceEverStarted { get; set; }

        // NOTE: When you change parameters, don't forget to look in to JobModule

        public HealthService(TimeSpan maxHealthyBbServiceDuration)
        {
            MaxHealthyBbServiceDuration = maxHealthyBbServiceDuration;
        }

        // NOTE: This method probably would stay in the real job, but will be modified
        public string GetHealthViolationMessage()
        {
            if (WasLastBbServiceFailed)
            {
                return "Last BbService was failed";
            }

            if (!WasLastBbServiceCompleted && !WasLastBbServiceFailed && !WasClientsBbServiceEverStarted)
            {
                return "Waiting for first BbService execution started";
            }

            if (!WasLastBbServiceCompleted && !WasLastBbServiceFailed && WasClientsBbServiceEverStarted)
            {
                return $"Waiting {DateTime.UtcNow - LastBbServiceStartedMoment} for first BbService execution completed";
            }

            if (LastBbServiceDuration > MaxHealthyBbServiceDuration)
            {
                return $"Last BbService was lasted for {LastBbServiceDuration}, which is too long";
            }
            return null;
        }

        // NOTE: These are example methods
        public void TraceBbServiceStarted()
        {
            LastBbServiceStartedMoment = DateTime.UtcNow;
            WasClientsBbServiceEverStarted = true;
        }

        public void TraceBbServiceCompleted()
        {
            LastBbServiceDuration = DateTime.UtcNow - LastBbServiceStartedMoment;
            WasLastBbServiceCompleted = true;
            WasLastBbServiceFailed = false;
        }

        public void TraceBbServiceFailed()
        {
            WasLastBbServiceCompleted = false;
            WasLastBbServiceFailed = true;
        }

        public void TraceBooStarted()
        {
            // TODO: See BbService
        }

        public void TraceBooCompleted()
        {
            // TODO: See BbService
        }

        public void TraceBooFailed()
        {
            // TODO: See BbService
        }
    }
}