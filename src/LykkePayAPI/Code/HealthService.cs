using System;

namespace LykkePay.API.Code
{
    public class HealthService : IHealthService
    {
        // NOTE: These are example properties
        public DateTime LastApiServiceStartedMoment { get; private set; }
        public TimeSpan LastApiServiceDuration { get; private set; }
        public TimeSpan MaxHealthyApiServiceDuration { get; }
        // NOTE: These are example properties
        private bool WasLastApiServiceFailed { get; set; }

        private bool WasApiServiceEverStarted { get; set; }


        // NOTE: When you change parameters, don't forget to look in to JobModule

        public HealthService(TimeSpan maxHealthyPrServiceDuration)
        {
            MaxHealthyApiServiceDuration = maxHealthyPrServiceDuration;
        }

        // NOTE: This method probably would stay in the real job, but will be modified
        public string GetHealthViolationMessage()
        {
            if (WasLastApiServiceFailed)
            {
                return "Last Lykke Pay Api was failed";
            }

          

            if (LastApiServiceDuration > MaxHealthyApiServiceDuration)
            {
                return $"Last PrService was lasted for {LastApiServiceDuration}, which is too long";
            }
            return null;
        }

        // NOTE: These are example methods
        public void TraceApiServiceStarted()
        {
            LastApiServiceStartedMoment = DateTime.UtcNow;
            WasApiServiceEverStarted = true;
        }

        
        public void TracePrServiceFailed()
        {
            WasLastApiServiceFailed = true;
        }

       
    }
}