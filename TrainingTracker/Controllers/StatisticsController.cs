using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Services;

namespace TrainingTracker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("GetAggregatedStatistics/{exerciseId}")]
        public async Task<IActionResult> GetAggregatedStatistics(int exerciseId)
        {
            var statistics = await _statisticsService.GetAggregatedStatisticsAsync(exerciseId);

            if (statistics == null)
            {
                return NotFound(new { message = "No data found for the specified exercise." });
            }

            return Ok(statistics);
        }
    }

}
