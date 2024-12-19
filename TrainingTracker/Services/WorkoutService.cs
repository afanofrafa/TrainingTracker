using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainingTracker.Models;

namespace TrainingTracker.Services
{
    public interface IWorkoutService
    {
        Task<Workout> GetWorkoutByIdAsync(long id);
        Task<List<Workout>> GetAllWorkoutsAsync();
        Task<Workout> CreateWorkoutAsync(Workout workout);
        Task<Workout> UpdateWorkoutAsync(long id, Workout workout);
        Task<bool> DeleteWorkoutAsync(long id);
        Task<List<WorkoutExercise>> GetWorkoutExercisesByWorkoutIdAsync(long workoutId);
    }

    public class WorkoutService : IWorkoutService
    {
        private readonly MydbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<WorkoutService> _logger;
        private readonly ILogger<WorkoutExerciseService> _loggerWE;
        private readonly WorkoutExerciseService _exerciseService; // Добавляем зависимость

        public WorkoutService(IMemoryCache cache, ILogger<WorkoutService> logger, ILogger<WorkoutExerciseService> loggerWE)
        {
            _context = new MydbContext();
            _cache = cache;
            _logger = logger;
            _loggerWE = loggerWE;
            _exerciseService = new WorkoutExerciseService(_loggerWE);
        }

        public async Task<Workout> GetWorkoutByIdAsync(long id)
        {
            _logger.LogInformation("Attempting to get workout with ID {WorkoutId}.", id);

            // Проверяем, если данные есть в кэше
            if (_cache.TryGetValue(id, out Workout cachedWorkout))
            {
                _logger.LogInformation("Returning workout with ID {WorkoutId} from cache.", id);
                return cachedWorkout;
            }

            try
            {
                // Получаем тренировку из базы данных
                var workout = await _context.Workouts.FindAsync(id);

                if (workout == null)
                {
                    _logger.LogWarning("Workout with ID {WorkoutId} not found.", id);
                    return null;
                }

                // Кэшируем результат для повторных запросов
                _cache.Set(id, workout, TimeSpan.FromMinutes(10));
                _logger.LogInformation("Workout with ID {WorkoutId} retrieved and cached.", id);

                return workout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving workout with ID {WorkoutId}.", id);
                throw;
            }
        }

        public async Task<List<Workout>> GetAllWorkoutsAsync()
        {
            _logger.LogInformation("Attempting to get all workouts.");

            // Проверка кэша для всех тренировок
            if (_cache.TryGetValue("all_workouts", out List<Workout> cachedWorkouts))
            {
                _logger.LogInformation("Returning all workouts from cache.");
                return cachedWorkouts;
            }

            try
            {
                var workouts = await _context.Workouts.ToListAsync();

                // Кэшируем все тренировки
                _cache.Set("all_workouts", workouts, TimeSpan.FromMinutes(10));
                _logger.LogInformation("All workouts retrieved and cached.");

                return workouts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all workouts.");
                throw;
            }
        }

        public async Task<Workout> UpdateWorkoutAsync(long id, Workout workout)
        {
            _logger.LogInformation("Attempting to update workout with ID {WorkoutId}.", id);

            try
            {
                var existingWorkout = await _context.Workouts.FindAsync(id);

                if (existingWorkout == null)
                {
                    _logger.LogWarning("Workout with ID {WorkoutId} not found.", id);
                    return null;
                }

                if (workout.WStartTime.HasValue && workout.WEndTime.HasValue)
                {
                    TimeSpan duration = workout.WEndTime.Value - workout.WStartTime.Value;
                    if (duration.TotalDays < 1)
                    {
                        workout.WTotalDuration = TimeOnly.FromTimeSpan(duration);
                    }
                    else
                    {
                        workout.WTotalDuration = TimeOnly.MaxValue;
                    }
                }
                else
                {
                    workout.WTotalDuration = null;
                }
                existingWorkout.WStartTime = workout.WStartTime;
                existingWorkout.WEndTime = workout.WEndTime;
                existingWorkout.WDate = workout.WDate;
                existingWorkout.WTotalDuration = workout.WTotalDuration;
                existingWorkout.WSequenceNumber = workout.WSequenceNumber;

                await _context.SaveChangesAsync();

                // Кэшируем обновленную тренировку
                _cache.Set(id, existingWorkout, TimeSpan.FromMinutes(10));

                // Обновляем кэш всех тренировок
                if (_cache.TryGetValue("all_workouts", out List<Workout> cachedWorkouts))
                {
                    // Находим индекс тренировки в списке
                    var index = cachedWorkouts.FindIndex(w => w.WId == id);
                    if (index != -1)
                    {
                        // Заменяем старую тренировку на обновленную
                        cachedWorkouts[index] = existingWorkout;
                    }
                    else
                    {
                        // Если тренировка не найдена, добавляем её (на всякий случай)
                        cachedWorkouts.Add(existingWorkout);
                    }

                    _cache.Set("all_workouts", cachedWorkouts, TimeSpan.FromMinutes(10));
                    _logger.LogInformation("Workout with ID {WorkoutId} updated in cache.", id);
                }

                return existingWorkout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating workout with ID {WorkoutId}.", id);
                throw;
            }
        }

        public async Task<bool> DeleteWorkoutAsync(long id)
        {
            _logger.LogInformation("Attempting to delete workout with ID {WorkoutId}.", id);
            try
            {
                var workout = await _context.Workouts.FindAsync(id);

                if (workout == null)
                {
                    _logger.LogWarning("Workout with ID {WorkoutId} not found.", id);
                    return false;
                }

                var relatedWorkoutExercises = _context.WorkoutExercises
                    .Where(we => we.WeWorkoutId == workout.WId).AsNoTracking()
                    .ToList();

                foreach (var workoutExercise in relatedWorkoutExercises)
                {
                    _logger.LogInformation("Deleting workout exercise with ID {WorkoutExerciseId} related to workout with ID {WorkoutId}.", workoutExercise.WeId, id);
                    await _exerciseService.DeleteWorkoutExerciseAsync(workoutExercise.WeId);
                }
                var _workout = await _context.Workouts.FindAsync(id);
                if (_workout == null)
                {
                    _logger.LogWarning("_Workout with ID {WorkoutId} not found.", id);
                    return false;
                }
                // Удаляем сам Workout
                
                _context.Workouts.Remove(_workout);
                await _context.SaveChangesAsync();

                // Удаляем кэш
                _cache.Remove("all_workouts"); // Удаляем кэш всех тренировок
                _cache.Remove(id); // Удаляем кэш удаленной тренировки

                _logger.LogInformation("Workout with ID {WorkoutId} deleted successfully.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting workout with ID {WorkoutId}.", id);
                throw;
            }
        }

        public async Task<Workout> CreateWorkoutAsync(Workout workout)
        {
            _logger.LogInformation("Attempting to create workout with ID {WorkoutId}.", workout.WId);

            try
            {
                // Проверяем, существует ли тренировка с указанным WId
                var existingWorkout = await _context.Workouts.FindAsync(workout.WId);
                if (existingWorkout != null)
                {
                    _logger.LogError("Workout with ID {WorkoutId} already exists.", workout.WId);
                    throw new Exception($"Workout with ID {workout.WId} already exists.");
                }

                // Проверяем, существует ли пользователь с указанным WUserId
                var user = await _context.Users.FindAsync(workout.WUserId);
                if (user == null)
                {
                    _logger.LogError("User with ID {UserId} not found for workout with ID {WorkoutId}.", workout.WUserId, workout.WId);
                    throw new Exception($"User with ID {workout.WUserId} not found.");
                }

                if (workout.WStartTime.HasValue && workout.WEndTime.HasValue)
                {
                    TimeSpan duration = workout.WEndTime.Value - workout.WStartTime.Value;
                    if (duration.TotalDays < 1)
                    {
                        workout.WTotalDuration = TimeOnly.FromTimeSpan(duration);
                    }
                    else
                    {
                        workout.WTotalDuration = TimeOnly.MaxValue;
                    }
                }
                else
                {
                    workout.WTotalDuration = null;
                }

                user.Workouts.Add(workout);

                // Сохраняем тренировку
                _context.Workouts.Add(workout);
                await _context.SaveChangesAsync();

                // Кэшируем новую тренировку
                _cache.Set(workout.WId, workout, TimeSpan.FromMinutes(10));

                // Обновляем кэш всех тренировок
                if (_cache.TryGetValue("all_workouts", out List<Workout> cachedWorkouts))
                {
                    cachedWorkouts.Add(workout);
                    _cache.Set("all_workouts", cachedWorkouts, TimeSpan.FromMinutes(10)); // Обновляем список тренировок в кэше
                }

                _logger.LogInformation("Workout with ID {WorkoutId} created successfully.", workout.WId);

                return workout;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating workout with ID {WorkoutId}.", workout.WId);
                throw;
            }
        }

        public async Task<List<WorkoutExercise>> GetWorkoutExercisesByWorkoutIdAsync(long workoutId)
        {
            _logger.LogInformation("Attempting to get workout exercises for workout with ID {WorkoutId}.", workoutId);

            try
            {
                var workoutExercises = await _context.WorkoutExercises
                    .Include(we => we.WeWexercise)  // Включаем информацию о упражнении
                    .Where(we => we.WeWorkoutId == workoutId)  // Фильтруем по ID тренировки
                    .ToListAsync();

                _logger.LogInformation("Retrieved {ExerciseCount} workout exercises for workout with ID {WorkoutId}.", workoutExercises.Count, workoutId);

                return workoutExercises;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving workout exercises for workout with ID {WorkoutId}.", workoutId);
                throw;
            }
        }

        private void ClearCache(long? workoutId = null)
        {
            _cache.Remove("all_workouts"); // Удаляем общий кэш всех тренировок

            if (workoutId.HasValue)
            {
                _cache.Remove(workoutId.Value); // Удаляем кэш конкретной тренировки, если ID указан
            }
        }
    }
}
