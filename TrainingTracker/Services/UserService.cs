using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TrainingTracker.Models;

namespace TrainingTracker.Services
{
    public interface IUserService
    {
        Task<List<User>> GetAllUsersAsync();
        Task<User> GetUserByIdAsync(long id);
        Task<User> AddUserAsync(User user);
        Task<User> UpdateUserAsync(User user);
        Task<bool> DeleteUserAsync(long id);
        Task<List<Workout>> GetWorkoutsByUserIdAsync(long userId);
    }

    public class UserService : IUserService
    {
        private readonly MydbContext _context;
        private readonly ILogger<UserService> _logger;
        private readonly ILogger<WorkoutService> _loggerW;
        private readonly ILogger<WorkoutExerciseService> _loggerWE;
        private readonly IMemoryCache _cache;
        private readonly WorkoutService _workoutService; // Добавляем зависимость

        public UserService(IMemoryCache cache, ILogger<UserService> logger, ILogger<WorkoutService> loggerW, ILogger<WorkoutExerciseService> loggerWE)
        {
            // Создаем новый экземпляр MydbContext
            _context = new MydbContext();
            _logger = logger;
            _loggerW = loggerW;
            _loggerWE = loggerWE;
            _cache = cache;
            _workoutService = new WorkoutService(_cache, _loggerW, _loggerWE);
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            _logger.LogInformation("Getting all users.");
            var users = await _context.Users.ToListAsync();
            _logger.LogInformation("Retrieved {UserCount} users.", users.Count);
            return users;
        }

        public async Task<User> GetUserByIdAsync(long id)
        {
            _logger.LogInformation("Getting user with ID {UserId}.", id);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UsId == id);
            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", id);
            }
            else
            {
                _logger.LogInformation("Retrieved user with ID {UserId}.", id);
            }
            return user;
        }

        public async Task<User> AddUserAsync(User user)
        {
            _logger.LogInformation("Adding user with ID {UserId}.", user.UsId);
            var existingUser = await _context.Users.FindAsync(user.UsId);
            if (existingUser != null)
            {
                _logger.LogWarning("User with ID {UserId} already exists.", user.UsId);
                return null; // Указываем, что пользователь уже существует
            }
            if (string.IsNullOrWhiteSpace(user.UsPassword))
            {
                _logger.LogError("Password is required for user with ID {UserId}.", user.UsId);
                throw new ArgumentException("Password is required.");
            }

            var salt = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(user.UsPassword)));
            using (var sha256 = SHA256.Create())
            {
                var combined = Encoding.UTF8.GetBytes(salt + user.UsPassword);
                var hash = sha256.ComputeHash(combined);
                user.UsPassword = Convert.ToBase64String(hash);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User with ID {UserId} added successfully.", user.UsId);
            return user;
        }

        public async Task<User> UpdateUserAsync(User user)
        {
            _logger.LogInformation("Updating user with ID {UserId}.", user.UsId);
            var existingUser = await _context.Users.FindAsync(user.UsId);
            if (existingUser == null)
            {
                _logger.LogWarning("User with ID {UserId} not found.", user.UsId);
                return null;
            }

            // Обновляем данные пользователя
            existingUser.UsName = user.UsName;
            existingUser.UsLogin = user.UsLogin;
            existingUser.UsEmail = user.UsEmail;
            existingUser.UsAge = user.UsAge;
            existingUser.UsHeight = user.UsHeight;
            existingUser.UsCurrWeight = user.UsCurrWeight;
            existingUser.UsStartWeight = user.UsStartWeight;
            existingUser.UsStartDate = user.UsStartDate;

            // Проверяем, изменился ли пароль
            if (!string.IsNullOrWhiteSpace(user.UsPassword) && user.UsPassword != existingUser.UsPassword)
            {
                var salt = Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(user.UsPassword)));
                using (var sha256 = SHA256.Create())
                {
                    var combined = Encoding.UTF8.GetBytes(salt + user.UsPassword);
                    var hash = sha256.ComputeHash(combined);
                    existingUser.UsPassword = Convert.ToBase64String(hash);
                }
                _logger.LogInformation("Password for user with ID {UserId} updated.", user.UsId);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("User with ID {UserId} updated successfully.", user.UsId);
            return existingUser;
        }

        public async Task<bool> DeleteUserAsync(long id)
        {
            //using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Deleting user with ID {UserId}.", id);
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found.", id);
                    return false;
                }

                var relatedWorkouts = _context.Workouts.Where(w => w.WUserId == user.UsId).AsNoTracking().ToList();

                _logger.LogInformation("Found {WorkoutCount} workouts related to user with ID {UserId}.", relatedWorkouts.Count, id);

                foreach (var workout in relatedWorkouts)
                {
                    _logger.LogInformation("Deleting workout with ID {WorkoutId} for user {UserId}.", workout.WId, id);
                    await _workoutService.DeleteWorkoutAsync(workout.WId); 
                }
                // Удаляем пользователя
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                _logger.LogInformation("User with ID {UserId} deleted successfully.", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user with ID {UserId}.", id);
                throw;
            }
        }

        public async Task<List<Workout>> GetWorkoutsByUserIdAsync(long userId)
        {
            _logger.LogInformation("Getting workouts for user with ID {UserId}.", userId);
            var workouts = await _context.Workouts
                .Include(w => w.WUser)  // Включаем пользователя
                .Where(w => w.WUserId == userId) // Фильтруем по userId
                .ToListAsync(); // Асинхронно извлекаем список тренировок

            _logger.LogInformation("Retrieved {WorkoutCount} workouts for user with ID {UserId}.", workouts.Count, userId);
            return workouts;
        }
    }
}
