using System;
using System.Net;
using Lykke.Job.BitcoinTransactionAggregator.Core.Services;
using Lykke.Job.BitcoinTransactionAggregator.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.SwaggerGen.Annotations;

namespace Lykke.Job.BitcoinTransactionAggregator.Controllers
{
    [Route("api/[controller]")]
    public class IsAliveController : Controller
    {
        private readonly IHealthService _healthService;

        public IsAliveController(IHealthService healthService)
        {
            _healthService = healthService;
        }

        /// <summary>
        /// Checks service is alive
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [SwaggerOperation("IsAlive")]
        [ProducesResponseType(typeof(IsAliveResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public IActionResult Get()
        {
            var healthViloationMessage = _healthService.GetHealthViolationMessage();
            if (healthViloationMessage != null)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, new ErrorResponse
                {
                    ErrorMessage = $"Job is unhealthy: {healthViloationMessage}"
                });
            }

            // NOTE: Feel free to extend IsAliveResponse, to display job-specific health status
            return Ok(new IsAliveResponse
            {
                Version = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationVersion,
                Env = Environment.GetEnvironmentVariable("ENV_INFO"),

                // NOTE: Health status information example: 
                LastBbHandlerStartedMoment = _healthService.LastBbServiceStartedMoment,
                LastBbHandlerDuration = _healthService.MaxHealthyBbServiceDuration,
                MaxHealthyFooDuration = _healthService.MaxHealthyBbServiceDuration
            });
        }
    }
}