using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using TrainingTracker.Models;
using TrainingTracker.Services;

namespace TrainingTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkoutExercisesController : ControllerBase
    {
        private readonly IWorkoutExerciseService _workoutExerciseService;

        public WorkoutExercisesController(IWorkoutExerciseService workoutExerciseService)
        {
            _workoutExerciseService = workoutExerciseService;
        }
        // Получение всех подходов по ID упражнения
        [HttpGet("GetSetsByExerciseId/{exerciseId}")]
        public async Task<ActionResult<List<Set>>> GetSetsByExerciseId(long exerciseId)
        {
            try
            {
                var sets = await _workoutExerciseService.GetSetsByExerciseIdAsync(exerciseId);

                if (sets == null || !sets.Any())
                {
                    return NotFound(new { message = $"No sets found for exercise ID {exerciseId}." });
                }

                return Ok(sets);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        // Создание подхода
        [HttpPost("CreateSet")]
        public async Task<ActionResult<Set>> CreateSet([FromBody] Set set)
        {
            if (set == null)
            {
                return BadRequest("Set is required.");
            }

            try
            {
                var createdSet = await _workoutExerciseService.CreateSetAsync(set);
                return CreatedAtAction(nameof(CreateSet), new { id = createdSet.SId }, createdSet);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // Получить упражнение по ID
        [HttpGet("GetWorkoutExerciseById/{id}")]
        public async Task<ActionResult<WorkoutExercise>> GetWorkoutExerciseById(long id)
        {
            var workoutExercise = await _workoutExerciseService.GetWorkoutExerciseByIdAsync(id);
            if (workoutExercise == null)
            {
                return NotFound();
            }
            return Ok(workoutExercise);
        }

        // Получить все упражнения
        [HttpGet("GetAllWorkoutExercises")]
        public async Task<ActionResult<IEnumerable<WorkoutExercise>>> GetAllWorkoutExercises()
        {
            var workoutExercises = await _workoutExerciseService.GetAllWorkoutExercisesAsync();
            return Ok(workoutExercises);
        }

        [HttpGet("GetWorkoutExercisesByWorkoutId/{workoutId}")]
        public async Task<ActionResult<IEnumerable<WorkoutExercise>>> GetWorkoutExercisesByWorkoutId(long workoutId)
        {
            var workoutExercises = await _workoutExerciseService.GetWorkoutExercisesByWorkoutIdAsync(workoutId);
            if (workoutExercises == null || workoutExercises.Count == 0)
            {
                return NotFound(new { message = "No exercises found for this workout." });
            }
            return Ok(workoutExercises);
        }

        // Создать упражнение для тренировки
        [HttpPost("CreateWorkoutExercise")]
        public async Task<ActionResult<WorkoutExercise>> CreateWorkoutExercise([FromBody] WorkoutExercise workoutExercise)
        {
            if (workoutExercise == null)
            {
                return BadRequest("WorkoutExercise is required.");
            }

            try
            {
                var createdWorkoutExercise = await _workoutExerciseService.CreateWorkoutExerciseAsync(workoutExercise);
                return CreatedAtAction(nameof(GetWorkoutExerciseById), new { id = createdWorkoutExercise.WeId }, createdWorkoutExercise);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // Обновить упражнение в тренировке
        [HttpPut("UpdateWorkoutExercise/{id}")]
        public async Task<ActionResult<WorkoutExercise>> UpdateWorkoutExercise(long id, [FromBody] WorkoutExercise workoutExercise)
        {
            var updatedWorkoutExercise = await _workoutExerciseService.UpdateWorkoutExerciseAsync(id, workoutExercise);
            if (updatedWorkoutExercise == null)
            {
                return NotFound();
            }

            return Ok(updatedWorkoutExercise);
        }

        // Удалить упражнение из тренировки
        [HttpDelete("DeleteWorkoutExercise/{id}")]
        public async Task<IActionResult> DeleteWorkoutExercise(long id)
        {
            var result = await _workoutExerciseService.DeleteWorkoutExerciseAsync(id);
            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
        // Обновление подхода
        [HttpPut("UpdateSet/{id}")]
        public async Task<IActionResult> UpdateSet(long id, [FromBody] Set updatedSet)
        {
            if (id != updatedSet.SId)
            {
                return BadRequest("Set ID in the route does not match ID in the body.");
            }

            try
            {
                var set = await _workoutExerciseService.UpdateSetAsync(id, updatedSet);
                if (set == null)
                {
                    return NotFound($"Set with ID {id} not updated.");
                }
                return Ok(set);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Удаление подхода
        [HttpDelete("DeleteSet/{id}")]
        public async Task<IActionResult> DeleteSet(long id)
        {
            try
            {
                var success = await _workoutExerciseService.DeleteSetAsync(id);
                if (!success)
                {
                    return NotFound($"Set with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Получение всех единиц инвентаря по ID подхода
        [HttpGet("GetAllEquipmentBySetId/{setId}")]
        public async Task<IActionResult> GetAllEquipmentBySetId(long setId)
        {
            try
            {
                var equipmentList = await _workoutExerciseService.GetAllEquipmentBySetIdAsync(setId);
                if (equipmentList == null || equipmentList.Count == 0)
                {
                    return NotFound($"No equipment found for set ID {setId}");
                }
                return Ok(equipmentList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("GetAllEquipment")]
        public async Task<IActionResult> GetAllEquipment()
        {
            try
            {
                // Получаем все Equipment из сервиса
                var equipmentList = await _workoutExerciseService.GetAllEquipmentAsync();

                if (equipmentList == null || equipmentList.Count == 0)
                {
                    return NotFound("No equipment found.");
                }

                return Ok(equipmentList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // Создание единицы инвентаря
        [HttpPost("CreateEquipment")]
        public async Task<IActionResult> CreateEquipment([FromBody] Equipment equipment)
        {
            try
            {
                var createdEquipment = await _workoutExerciseService.CreateEquipmentAsync(equipment);
                if (createdEquipment == null)
                {
                    return NotFound($"Equipment with ID {equipment.EqId} is already exists.");
                }
                return CreatedAtAction(nameof(GetAllEquipmentBySetId), new { setId = createdEquipment.EqSetId }, createdEquipment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Обновление единицы инвентаря
        [HttpPut("UpdateEquipment/{id}")]
        public async Task<IActionResult> UpdateEquipment(long id, [FromBody] Equipment updatedEquipment)
        {
            if (id != updatedEquipment.EqId)
            {
                return BadRequest("Equipment ID in the route does not match ID in the body.");
            }

            try
            {
                var equipment = await _workoutExerciseService.UpdateEquipmentAsync(id, updatedEquipment);
                if (equipment == null)
                {
                    return NotFound($"Equipment with ID {id} not updated.");
                }
                return Ok(equipment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // Удаление единицы инвентаря
        [HttpDelete("DeleteEquipmentById/{id}")]
        public async Task<IActionResult> DeleteEquipment(long id)
        {
            try
            {
                var success = await _workoutExerciseService.DeleteEquipmentAsync(id);
                if (!success)
                {
                    return NotFound($"Equipment with ID {id} not found.");
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
