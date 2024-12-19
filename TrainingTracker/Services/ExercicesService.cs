using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TrainingTracker.Models;
using Microsoft.Extensions.Logging;

namespace TrainingTracker.Services
{
    public interface IWorkoutExerciseService
    {
        Task<Set> CreateSetAsync(Set set);
        Task<List<Set>> GetSetsByExerciseIdAsync(long exerciseId);
        Task<WorkoutExercise> GetWorkoutExerciseByIdAsync(long workoutExerciseId);
        Task<List<WorkoutExercise>> GetAllWorkoutExercisesAsync();
        Task<List<WorkoutExercise>> GetWorkoutExercisesByWorkoutIdAsync(long workoutId);
        Task<WorkoutExercise> CreateWorkoutExerciseAsync(WorkoutExercise workoutExercise);
        Task<WorkoutExercise> UpdateWorkoutExerciseAsync(long id, WorkoutExercise workoutExercise);
        Task<bool> DeleteWorkoutExerciseAsync(long id);
        Task<Set> UpdateSetAsync(long id, Set updatedSet);
        Task<bool> DeleteSetAsync(long id);
        Task<List<Equipment>> GetAllEquipmentBySetIdAsync(long setId);
        Task<Equipment> CreateEquipmentAsync(Equipment equipment);
        Task<Equipment> UpdateEquipmentAsync(long id, Equipment updatedEquipment);
        Task<bool> DeleteEquipmentAsync(long id);
        Task<List<Equipment>> GetAllEquipmentAsync();
    }

    public class WorkoutExerciseService : IWorkoutExerciseService
    {
        private readonly MydbContext _context;
        private readonly ILogger<WorkoutExerciseService> _logger;

        public WorkoutExerciseService(ILogger<WorkoutExerciseService> logger)
        {
            _context = new MydbContext();
            _logger = logger;
        }

        public async Task<List<Set>> GetSetsByExerciseIdAsync(long exerciseId)
        {
            _logger.LogInformation($"Getting sets for exercise ID: {exerciseId}");
            try
            {
                var sets = await _context.Sets
                                         .Where(s => s.SWeId == exerciseId)
                                         .ToListAsync();

                _logger.LogInformation($"Found {sets.Count} sets for exercise ID: {exerciseId}");
                return sets;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving sets for exercise.");
                throw;
            }
        }
        // Обновление подхода
        public async Task<Set> UpdateSetAsync(long id, Set updatedSet)
        {
            try
            {
                _logger.LogInformation("Attempting to update Set with ID: {SetId}", id);

                // Проверяем, существует ли Set с указанным ID
                var existingSet = await _context.Sets.FindAsync(id);
                if (existingSet == null)
                {
                    _logger.LogWarning("Set with ID {SetId} not found.", id);
                    return null;
                }

                // Логируем старые значения Set
                _logger.LogInformation("Current Set details: SequenceNumber={CurrentSequenceNumber}, Effort={CurrentEffort}, RestTimeAfterSet={CurrentRestTimeAfterSet}",
                    existingSet.SSequenceNumber, existingSet.SEffort, existingSet.SRestTimeAfterSet);

                // Проверяем, существует ли WorkoutExercise с WeId, равным SWeId
                var relatedWorkoutExercise = await _context.WorkoutExercises
                    .FirstOrDefaultAsync(we => we.WeId == updatedSet.SWeId && we.WeWexerciseId == updatedSet.SWexerciseId);
                if (relatedWorkoutExercise == null)
                {
                    _logger.LogError("WorkoutExercise with ID {WorkoutExerciseId} not found.", updatedSet.SWeId);
                    throw new Exception($"WorkoutExercise with ID {updatedSet.SWeId} not found.");
                }

                // Проверяем, существует ли Exercise с EId, равным SWexerciseId
                var relatedExercise = await _context.Exercises.FindAsync(updatedSet.SWexerciseId);
                if (relatedExercise == null)
                {
                    _logger.LogError("Exercise with ID {ExerciseId} not found.", updatedSet.SWexerciseId);
                    throw new Exception($"Exercise with ID {updatedSet.SWexerciseId} not found.");
                }

                // Обновляем поля Set
                existingSet.SSequenceNumber = updatedSet.SSequenceNumber;
                existingSet.SEffort = updatedSet.SEffort;
                existingSet.SRestTimeAfterSet = updatedSet.SRestTimeAfterSet;
                existingSet.SComments = updatedSet.SComments;
                existingSet.SRepsDone = updatedSet.SRepsDone;
                existingSet.SWexerciseId = updatedSet.SWexerciseId;
                existingSet.SWeId = updatedSet.SWeId;

                // Логируем новые значения Set
                _logger.LogInformation("Updated Set details: SequenceNumber={UpdatedSequenceNumber}, Effort={UpdatedEffort}, RestTimeAfterSet={UpdatedRestTimeAfterSet}",
                    updatedSet.SSequenceNumber, updatedSet.SEffort, updatedSet.SRestTimeAfterSet);

                // Сохраняем изменения
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully updated Set with ID: {SetId}", id);

                return existingSet;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating Set with ID: {SetId}", id);
                throw;
            }
        }
        // Получение всех единиц инвентаря
        public async Task<List<Equipment>> GetAllEquipmentAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all equipment.");

                // Извлекаем все записи из таблицы Equipment
                var equipmentList = await _context.Equipment.ToListAsync();

                // Логируем, если найдено оборудование
                if (equipmentList.Any())
                {
                    _logger.LogInformation("Successfully fetched {EquipmentCount} equipment items.", equipmentList.Count);
                }
                else
                {
                    _logger.LogWarning("No equipment found.");
                }

                return equipmentList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching all equipment.");
                throw;
            }
        }

        public async Task<WorkoutExercise> GetWorkoutExerciseByIdAsync(long workoutExerciseId)
        {
            _logger.LogInformation($"Getting workout exercise with ID: {workoutExerciseId}");
            try
            {
                var workoutExercise = await _context.WorkoutExercises
                    .Include(we => we.WeWexercise)
                    .Include(we => we.WeWorkout)
                    .FirstOrDefaultAsync(we => we.WeId == workoutExerciseId);

                if (workoutExercise == null)
                {
                    _logger.LogWarning($"Workout exercise with ID: {workoutExerciseId} not found.");
                }
                return workoutExercise;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving workout exercise by ID.");
                throw;
            }
        }
        // Удаление подхода
        public async Task<bool> DeleteSetAsync(long id)
        {
            try
            {
                _logger.LogInformation("Attempting to delete Set with ID: {SetId}", id);

                var set = await _context.Sets.FindAsync(id);

                if (set == null)
                {
                    _logger.LogWarning("Set with ID {SetId} not found.", id);
                    return false;
                }

                // Находим все связанные Equipment
                var relatedEquipment = _context.Equipment.Where(eq => eq.EqSetId == set.SId).ToList();

                // Логируем, если инвентарь связан с этим подходом
                if (relatedEquipment.Any())
                {
                    _logger.LogInformation("Found {EquipmentCount} equipment items associated with Set ID {SetId}.", relatedEquipment.Count, id);
                }
                else
                {
                    _logger.LogInformation("No equipment found for Set ID {SetId}.", id);
                }

                // Удаляем каждую единицу Equipment
                foreach (var equipment in relatedEquipment)
                {
                    _logger.LogInformation("Deleting Equipment with ID: {EquipmentId}", equipment.EqId);
                    await DeleteEquipmentAsync(equipment.EqId);
                }

                // Удаляем сам Set
                _context.Sets.Remove(set);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Successfully deleted Set with ID: {SetId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting Set with ID: {SetId}", id);
                throw;
            }
        }

        public async Task<List<WorkoutExercise>> GetAllWorkoutExercisesAsync()
        {
            _logger.LogInformation("Getting all workout exercises.");
            try
            {
                var workoutExercises = await _context.WorkoutExercises
                    .Include(we => we.WeWexercise)
                    .Include(we => we.WeWorkout)
                    .ToListAsync();

                _logger.LogInformation($"Found {workoutExercises.Count} workout exercises.");
                return workoutExercises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all workout exercises.");
                throw;
            }
        }

        public async Task<List<WorkoutExercise>> GetWorkoutExercisesByWorkoutIdAsync(long workoutId)
        {
            _logger.LogInformation($"Getting workout exercises for workout ID: {workoutId}");
            try
            {
                var workoutExercises = await _context.WorkoutExercises
                    .Include(we => we.WeWexercise)
                    .Include(we => we.WeWorkout)
                    .Where(we => we.WeWorkoutId == workoutId)
                    .ToListAsync();

                _logger.LogInformation($"Found {workoutExercises.Count} workout exercises for workout ID: {workoutId}");
                return workoutExercises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving workout exercises for workout.");
                throw;
            }
        }

        public async Task<WorkoutExercise> CreateWorkoutExerciseAsync(WorkoutExercise workoutExercise)
        {
            _logger.LogInformation("Creating new workout exercise.");
            try
            {
                var workout = await _context.Workouts.FindAsync(workoutExercise.WeWorkoutId);
                if (workout == null)
                {
                    throw new Exception($"Workout with ID {workoutExercise.WeWorkoutId} not found.");
                }

                var wexercise = await _context.WorkoutExercises
                    .FirstOrDefaultAsync(we => we.WeId == workoutExercise.WeId);

                if (wexercise != null)
                {
                    throw new Exception($"WorkoutExercise with ID {workoutExercise.WeId} already exists.");
                }

                var exercise = await _context.Exercises.FindAsync(workoutExercise.WeWexerciseId);
                if (exercise == null)
                {
                    throw new Exception($"Exercise with ID {workoutExercise.WeWexerciseId} not found.");
                }

                workoutExercise.WeWorkout = workout;
                workoutExercise.WeWexercise = exercise;

                _context.WorkoutExercises.Add(workoutExercise);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created new workout exercise with ID: {workoutExercise.WeId}");
                return workoutExercise;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating workout exercise.");
                throw;
            }
        }

        public async Task<Set> CreateSetAsync(Set set)
        {
            _logger.LogInformation("Creating new set.");
            try
            {
                var existingSet = await _context.Sets.FindAsync(set.SId);
                if (existingSet != null)
                {
                    throw new Exception($"Set with ID {set.SId} already exists.");
                }

                var workoutExercise = await _context.WorkoutExercises
                    .FirstOrDefaultAsync(we => we.WeId == set.SWeId && we.WeWexerciseId == set.SWexerciseId);

                if (workoutExercise == null)
                {
                    throw new Exception($"WorkoutExercise with Workout ID {set.SWeId} and Exercise ID {set.SWexerciseId} not found.");
                }

                set.WorkoutExercise = workoutExercise;
                _context.Sets.Add(set);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created new set with ID: {set.SId}");
                return set;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating set.");
                throw;
            }
        }

        public async Task<WorkoutExercise> UpdateWorkoutExerciseAsync(long id, WorkoutExercise workoutExercise)
        {
            _logger.LogInformation($"Updating workout exercise with ID: {id}");
            try
            {
                var existingWorkoutExercise = await _context.WorkoutExercises
                    .FirstOrDefaultAsync(we => we.WeId == workoutExercise.WeId);

                if (existingWorkoutExercise == null)
                {
                    _logger.LogWarning($"Workout exercise with ID: {workoutExercise.WeId} not found.");
                    return null;
                }

                var relatedWorkout = await _context.Workouts.FindAsync(workoutExercise.WeWorkoutId);
                if (relatedWorkout == null)
                {
                    throw new Exception($"Workout with ID {workoutExercise.WeWorkoutId} not found.");
                }

                var relatedExercise = await _context.Exercises.FindAsync(workoutExercise.WeWexerciseId);
                if (relatedExercise == null)
                {
                    throw new Exception($"Exercise with ID {workoutExercise.WeWexerciseId} not found.");
                }

                existingWorkoutExercise.WeWorkoutId = workoutExercise.WeWorkoutId;
                existingWorkoutExercise.WeWexerciseId = workoutExercise.WeWexerciseId;
                existingWorkoutExercise.WeRestTimeAfterExercise = workoutExercise.WeRestTimeAfterExercise;
                existingWorkoutExercise.WeSequenceNumber = workoutExercise.WeSequenceNumber;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated workout exercise with ID: {workoutExercise.WeId}");
                return existingWorkoutExercise;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating workout exercise.");
                throw;
            }
        }

        public async Task<bool> DeleteWorkoutExerciseAsync(long id)
        {
            _logger.LogInformation($"Deleting workout exercise with ID: {id}");
            try
            {
                var workoutExercise = await _context.WorkoutExercises.AsNoTracking()
                    .FirstOrDefaultAsync(we => we.WeId == id);

                if (workoutExercise == null)
                {
                    _logger.LogWarning($"Workout exercise with ID: {id} not found.");
                    return false;
                }

                var relatedSets = _context.Sets.Where(set => set.SWeId == workoutExercise.WeId).ToList();
                foreach (var set in relatedSets)
                {
                    await DeleteSetAsync(set.SId);
                }

                _context.WorkoutExercises.Remove(workoutExercise);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted workout exercise with ID: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting workout exercise.");
                throw;
            }
        }

        public async Task<List<Equipment>> GetAllEquipmentBySetIdAsync(long setId)
        {
            _logger.LogInformation($"Getting all equipment for set ID: {setId}");
            try
            {
                var equipmentList = await _context.Equipment
                    .Where(e => e.EqSetId == setId)
                    .ToListAsync();

                _logger.LogInformation($"Found {equipmentList.Count} equipment items for set ID: {setId}");
                return equipmentList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving equipment by set ID.");
                throw;
            }
        }

        public async Task<Equipment> CreateEquipmentAsync(Equipment equipment)
        {
            _logger.LogInformation("Creating new equipment.");
            try
            {
                var existingEquipment = await _context.Equipment.FindAsync(equipment.EqId);

                if (existingEquipment != null)
                {
                    _logger.LogWarning($"Equipment with ID: {equipment.EqId} is already exists.");
                    return null;
                }
                var set = await _context.Sets.FindAsync(equipment.EqSetId);
                if (set == null)
                {
                    _logger.LogWarning($"Set with ID {equipment.EqSetId} not found.");
                }

                _context.Equipment.Add(equipment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Created new equipment with ID: {equipment.EqId}");
                return equipment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating equipment.");
                throw;
            }
        }

        public async Task<Equipment> UpdateEquipmentAsync(long id, Equipment updatedEquipment)
        {
            _logger.LogInformation($"Updating equipment with ID: {id}");
            try
            {
                var existingEquipment = await _context.Equipment.FindAsync(id);

                if (existingEquipment == null)
                {
                    _logger.LogWarning($"Equipment with ID: {id} not found.");
                    return null;
                }
                var set = await _context.Sets.FindAsync(updatedEquipment.EqSetId);
                if (set == null)
                {
                    _logger.LogWarning($"Set with ID {updatedEquipment.EqSetId} not found.");
                    return null;
                }
                existingEquipment.EqName = updatedEquipment.EqName;
                existingEquipment.EqDescription = updatedEquipment.EqDescription;
                existingEquipment.EqWeight = updatedEquipment.EqWeight;
                existingEquipment.EqSetId = updatedEquipment.EqSetId;

                await _context.SaveChangesAsync();

                _logger.LogInformation($"Updated equipment with ID: {id}");
                return existingEquipment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating equipment.");
                throw;
            }
        }

        public async Task<bool> DeleteEquipmentAsync(long id)
        {
            _logger.LogInformation($"Deleting equipment with ID: {id}");
            try
            {
                var equipment = await _context.Equipment.FindAsync(id);

                if (equipment == null)
                {
                    _logger.LogWarning($"Equipment with ID: {id} not found.");
                    return false;
                }
                _context.Equipment.Remove(equipment);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Deleted equipment with ID: {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting equipment.");
                throw;
            }
        }
    }
}
