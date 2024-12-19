using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Models;
using TrainingTracker.Services;

namespace TrainingTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutsController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;

        public WorkoutsController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        // Получить тренировку по ID
        [HttpGet("GetWorkoutById/{id}")]
        public async Task<ActionResult<Workout>> GetWorkoutById(long id)
        {
            var workout = await _workoutService.GetWorkoutByIdAsync(id);

            if (workout == null)
            {
                return NotFound();
            }

            return Ok(workout);
        }

        // Получить все тренировки
        [HttpGet("GetAllWorkouts")]
        public async Task<ActionResult<IEnumerable<Workout>>> GetAllWorkouts()
        {
            var workouts = await _workoutService.GetAllWorkoutsAsync();
            return Ok(workouts);
        }

        // Обновить тренировку
        [HttpPut("UpdateWorkout/{id}")]
        public async Task<ActionResult<Workout>> UpdateWorkout(long id, Workout workout)
        {
            var updatedWorkout = await _workoutService.UpdateWorkoutAsync(id, workout);

            if (updatedWorkout == null)
            {
                return NotFound();
            }

            return Ok(updatedWorkout);
        }

        // Удалить тренировку
        [HttpDelete("DeleteWorkout/{id}")]
        public async Task<IActionResult> DeleteWorkout(long id)
        {
            var result = await _workoutService.DeleteWorkoutAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        // Создать новую тренировку
        [HttpPost("CreateWorkout")]
        public async Task<ActionResult<Workout>> CreateWorkout([FromBody] Workout workout)
        {
            if (workout.WUserId <= 0)
            {
                return BadRequest("WUserId is required.");
            }

            try
            {
                var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);

                return CreatedAtAction(nameof(GetWorkoutById), new { id = createdWorkout.WId }, createdWorkout);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("GetWorkoutExercisesByWorkoutId/{workoutId}")]
        public async Task<ActionResult<List<WorkoutExercise>>> GetWorkoutExercisesByWorkoutId(long workoutId)
        {
            var workoutExercises = await _workoutService.GetWorkoutExercisesByWorkoutIdAsync(workoutId);

            if (workoutExercises == null || workoutExercises.Count == 0)
            {
                return NotFound(new { message = "No exercises found for the workout." });
            }

            return Ok(workoutExercises);
        }
    }
}
