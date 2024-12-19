using Microsoft.EntityFrameworkCore;
using TrainingTracker;
using TrainingTracker.Models;
using TrainingTracker.Services;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<MydbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 40))));
builder.Services.AddMemoryCache();
//builder.Services.AddDbContext<MydbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
//);
// Add services to the container.
builder.Services.AddControllers();
// Register UserService for dependency injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IWorkoutExerciseService, WorkoutExerciseService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
// Инициализация данных в базе
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<MydbContext>();
    await InitializeDatabase(context);
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

// Метод для инициализации данных
async Task InitializeDatabase(MydbContext context)
{
    // Проверка, есть ли записи в таблице Exercises
    if (!context.Exercises.Any())
    {
        var exercises = new List<Exercise>
        {
            new Exercise { EId = 0, EName = "Push Up", EDescription = "Chest exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Easy" },
            new Exercise { EId = 1, EName = "Squat", EDescription = "Leg exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" },
            new Exercise { EId = 2, EName = "Pull Up", EDescription = "Back exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Hard" },
            new Exercise { EId = 3, EName = "Deadlift", EDescription = "Full body exercise", EEquipmentRequired = 1, ETechniqueDifficultyLevel = "Hard" },
            new Exercise { EId = 4, EName = "Lunge", EDescription = "Leg exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" },
            new Exercise { EId = 5, EName = "Bench Press", EDescription = "Chest exercise", EEquipmentRequired = 1, ETechniqueDifficultyLevel = "Hard" },
            new Exercise { EId = 6, EName = "Plank", EDescription = "Core exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Easy" },
            new Exercise { EId = 7, EName = "Bicep Curl", EDescription = "Arm exercise", EEquipmentRequired = 1, ETechniqueDifficultyLevel = "Medium" },
            new Exercise { EId = 8, EName = "Tricep Dip", EDescription = "Arm exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" }
        };


        foreach (var exercise in exercises)
        {
            if (!context.Exercises.Any(e => e.EId == exercise.EId))
            {
                context.Exercises.Add(exercise);
            }
        }

        await context.SaveChangesAsync();
    }
}

