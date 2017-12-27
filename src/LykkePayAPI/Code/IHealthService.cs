using System;

namespace LykkePay.API.Code
{
    public interface IHealthService
    {
        // NOTE: These are example properties
        DateTime LastApiServiceStartedMoment { get; }
        TimeSpan LastApiServiceDuration { get; }
        TimeSpan MaxHealthyApiServiceDuration { get; }

        // NOTE: This method probably would stay in the real job, but will be modified
        string GetHealthViolationMessage();

        // NOTE: These are example methods
        void TraceApiServiceStarted();
        void TracePrServiceFailed();
    }
}