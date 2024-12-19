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
        // Метод для удаления всех записей UsersWeekStatisticsTotal
        [HttpDelete("DeleteAllUsersWeekStatistics")]
        public async Task<IActionResult> DeleteAllUsersWeekStatistics()
        {
            try
            {
                await _statisticsService.DeleteAllUsersWeekStatisticsAsync();
                return Ok(new { message = "All UsersWeekStatisticsTotal records deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while deleting records: {ex.Message}" });
            }
        }

        // Метод для получения всех записей UsersWeekStatisticsTotal
        [HttpGet("GetAllUsersWeekStatistics")]
        public async Task<IActionResult> GetAllUsersWeekStatistics()
        {
            try
            {
                var statistics = await _statisticsService.GetAllUsersWeekStatisticsAsync();
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"An error occurred while fetching records: {ex.Message}" });
            }
        }
    }

}
