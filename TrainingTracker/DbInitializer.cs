using TrainingTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using TrainingTracker;
namespace TrainingTracker
{
    public static class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, MydbContext context)
        {
            // Проверяем, есть ли уже упражнения в базе данных
            if (context.Exercises.Any())
            {
                return; // Если упражнения уже существуют, ничего не делаем
            }

            // Если упражнений нет, добавляем их
            var exercises = new List<Exercise>
        {
            new Exercise { EName = "Push-up", EDescription = "A basic upper body strength exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Easy" },
            new Exercise { EName = "Squat", EDescription = "A lower body exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" },
            new Exercise { EName = "Deadlift", EDescription = "A compound strength exercise", EEquipmentRequired = 1, ETechniqueDifficultyLevel = "Hard" },
            new Exercise { EName = "Plank", EDescription = "A core exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" },
            new Exercise { EName = "Lunges", EDescription = "A lower body exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" }
        };

            // Добавляем упражнения в базу данных
            await context.Exercises.AddRangeAsync(exercises);
            await context.SaveChangesAsync();
        }
    }
}

