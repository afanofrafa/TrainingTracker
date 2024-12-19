using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TrainingTracker.Models;
using TrainingTracker.Services;

namespace TrainingTracker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        // Конструктор с внедрением зависимости через интерфейс IUserService
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // Получить всех пользователей
        [HttpGet("GetAllUsers")]
        public async Task<ActionResult<List<User>>> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("GetUserById/{id}")]
        public async Task<ActionResult<User>> GetUserById(long id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();

            return Ok(user);
        }


        // Создать нового пользователя
        [HttpPost("AddUser")]
        public async Task<ActionResult<User>> AddUser(User user)
        {
            var newUser = await _userService.AddUserAsync(user);
            if (newUser == null)
            {
                return Conflict(new { message = "A user with this ID already exists." });
            }
            return CreatedAtAction(nameof(GetUserById), new { id = newUser.UsId }, newUser);
        }

        // Обновить пользователя
        [HttpPut("UpdateUser/{id}")]
        public async Task<ActionResult<User>> UpdateUser(long id, User user)
        {
            if (id != user.UsId) return BadRequest("ID пользователя не совпадает");

            var updatedUser = await _userService.UpdateUserAsync(user);
            if (updatedUser == null) return NotFound();

            return Ok(updatedUser);
        }

        // Удалить пользователя
        [HttpDelete("DeleteUser/{id}")]
        public async Task<ActionResult> DeleteUser(long id)
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success) return NotFound();

            return NoContent();
        }
        [HttpGet("GetWorkoutsByUserId/{userId}")]
        public async Task<ActionResult<List<Workout>>> GetWorkoutsByUserId(long userId)
        {
            var workouts = await _userService.GetWorkoutsByUserIdAsync(userId);

            if (workouts == null || workouts.Count == 0)
            {
                return NotFound(new { message = "No workouts found for the user." });
            }

            return Ok(workouts);
        }
    }
}


