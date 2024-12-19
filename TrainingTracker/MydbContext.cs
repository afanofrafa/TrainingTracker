using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using TrainingTracker.Models;

namespace TrainingTracker;

public partial class MydbContext : DbContext
{
    public MydbContext()
    {
        //Initialize(this);
    }
    private async Task Initialize(MydbContext context)
    {
        // Проверяем, есть ли уже записи в таблице упражнений
        if (!context.Exercises.Any())
        {
            var exercises = new List<Exercise>
            {
                new Exercise { EName = "Push Up", EDescription = "Chest exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Easy" },
                new Exercise { EName = "Squat", EDescription = "Leg exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" },
                new Exercise { EName = "Pull Up", EDescription = "Back exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Hard" },
                new Exercise { EName = "Deadlift", EDescription = "Full body exercise", EEquipmentRequired = 1, ETechniqueDifficultyLevel = "Hard" },
                new Exercise { EName = "Lunge", EDescription = "Leg exercise", EEquipmentRequired = 0, ETechniqueDifficultyLevel = "Medium" }
            };

            foreach (var exercise in exercises)
            {
                if (!context.Exercises.Any(e => e.EName == exercise.EName)) // Проверка на существование
                {
                    context.Exercises.Add(exercise);
                }
            }

            await context.SaveChangesAsync();
        }

    }
    public MydbContext(DbContextOptions<MydbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Set> Sets { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UsersWeekStatisticsTotal> UsersWeekStatisticsTotals { get; set; }

    public virtual DbSet<WeekExercisesStatistic> WeekExercisesStatistics { get; set; }

    public virtual DbSet<WeekUserStatistic> WeekUserStatistics { get; set; }

    public virtual DbSet<Workout> Workouts { get; set; }

    public virtual DbSet<WorkoutExercise> WorkoutExercises { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=127.0.0.1;port=3306;user=root;password=Apcehn2004;database=mydb", ServerVersion.Parse("8.0.40-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb3_general_ci")
            .HasCharSet("utf8mb3");

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EqId).HasName("PRIMARY");

            entity.ToTable("equipment");

            entity.HasIndex(e => e.EqSetId, "fk_Equipment_Set1_idx");

            entity.Property(e => e.EqId)
                .ValueGeneratedNever()
                .HasColumnName("eq_id");
            entity.Property(e => e.EqDescription)
                .HasColumnType("text")
                .HasColumnName("eq_description");
            entity.Property(e => e.EqName)
                .HasMaxLength(45)
                .HasColumnName("eq_name");
            entity.Property(e => e.EqSetId).HasColumnName("eq_set_id");
            entity.Property(e => e.EqWeight).HasColumnName("eq_weight");

            entity.HasOne(d => d.EqSet).WithMany(p => p.Equipment)
                .HasForeignKey(d => d.EqSetId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Equipment_Set1");
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.EId).HasName("PRIMARY");

            entity.ToTable("exercise");

            entity.Property(e => e.EId)
                .ValueGeneratedNever()
                .HasColumnName("e_id");
            entity.Property(e => e.EDescription)
                .HasColumnType("text")
                .HasColumnName("e_description");
            entity.Property(e => e.EEquipmentRequired).HasColumnName("e_ equipment_required");
            entity.Property(e => e.EName)
                .HasMaxLength(255)
                .HasColumnName("e_name");
            entity.Property(e => e.ETechniqueDifficultyLevel)
                .HasMaxLength(50)
                .HasColumnName(" e_technique_difficulty_level");
        });

        modelBuilder.Entity<Set>(entity =>
        {
            entity.HasKey(e => e.SId).HasName("PRIMARY");

            entity.ToTable("set");

            entity.HasIndex(e => new { e.SWexerciseId, e.SWeId }, "fk_Set_Workout_Exercise1_idx");

            entity.Property(e => e.SId)
                .ValueGeneratedNever()
                .HasColumnName("s_id");
            entity.Property(e => e.SComments)
                .HasColumnType("text")
                .HasColumnName("s_comments");
            entity.Property(e => e.SEffort).HasColumnName("s_effort");
            entity.Property(e => e.SRepsDone).HasColumnName("s_reps_done");
            entity.Property(e => e.SRestTimeAfterSet)
                .HasColumnType("time")
                .HasColumnName("s_rest_time_after_set");
            entity.Property(e => e.SSequenceNumber).HasColumnName("s_sequence_number");
            entity.Property(e => e.SWeId).HasColumnName("s_we_id");
            entity.Property(e => e.SWexerciseId).HasColumnName("s_wexercise_id");

            entity.HasOne(d => d.WorkoutExercise).WithMany(p => p.Sets)
                .HasForeignKey(d => new { d.SWexerciseId, d.SWeId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Set_Workout_Exercise1");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UsId).HasName("PRIMARY");

            entity.ToTable("user");

            entity.Property(e => e.UsId)
                .ValueGeneratedNever()
                .HasColumnName("us_id");
            entity.Property(e => e.UsAge).HasColumnName("us_age");
            entity.Property(e => e.UsCurrWeight).HasColumnName("us_curr_weight");
            entity.Property(e => e.UsEmail)
                .HasMaxLength(45)
                .HasColumnName("us_email");
            entity.Property(e => e.UsHeight).HasColumnName("us_height");
            entity.Property(e => e.UsLogin)
                .HasMaxLength(45)
                .HasColumnName("us_login");
            entity.Property(e => e.UsName)
                .HasMaxLength(45)
                .HasColumnName("us_name");
            entity.Property(e => e.UsPassword)
                .HasMaxLength(45)
                .HasColumnName("us_password");
            entity.Property(e => e.UsStartDate).HasColumnName("us_start_date");
            entity.Property(e => e.UsStartWeight).HasColumnName("us_start_weight");
        });

        modelBuilder.Entity<UsersWeekStatisticsTotal>(entity =>
        {
            entity.HasKey(e => new { e.UwsExerciseId, e.UwsId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("users_week_statistics_total");

            entity.HasIndex(e => e.UwsExerciseId, "fk_Users_Week_Statistics_Exercise1_idx");

            entity.Property(e => e.UwsExerciseId).HasColumnName("uws_exercise_id");
            entity.Property(e => e.UwsId).HasColumnName("uws_id");
            entity.Property(e => e.UwsRepsNum).HasColumnName("uws_reps_num");
            entity.Property(e => e.UwsRestTimeAfterExercSec).HasColumnName("uws_rest_time_after_exerc_sec");
            entity.Property(e => e.UwsRestTimeBtwSetsSec).HasColumnName("uws_rest_time_btw_sets_sec");
            entity.Property(e => e.UwsSetsNum).HasColumnName("uws_sets_num");
            entity.Property(e => e.UwsTotalEffort).HasColumnName("uws_total_effort");
            entity.Property(e => e.UwsUsersHaveDoneNum).HasColumnName("uws_users_have_done_num");
            entity.Property(e => e.UwsWeekStart).HasColumnName("uws_week_start");
            entity.Property(e => e.UwsWeightLifted).HasColumnName("uws_weight_lifted");
            entity.Property(e => e.UwsWexerciseNum).HasColumnName("uws_wexercise_num");

            entity.HasOne(d => d.UwsExercise).WithMany(p => p.UsersWeekStatisticsTotals)
                .HasForeignKey(d => d.UwsExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Users_Week_Statistics_Exercise1");
        });

        modelBuilder.Entity<WeekExercisesStatistic>(entity =>
        {
            entity.HasKey(e => new { e.WesWkstId, e.WesExerciseId, e.WesId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0, 0 });

            entity.ToTable("week_exercises_statistics");

            entity.HasIndex(e => e.WesExerciseId, "fk_Week_Exercises_Statistics_Exercise1_idx");

            entity.HasIndex(e => e.WesWkstId, "fk_Week_Exercises_Statistics_Week_User_Statistics1_idx");

            entity.Property(e => e.WesWkstId).HasColumnName("wes_wkst_id");
            entity.Property(e => e.WesExerciseId).HasColumnName("wes_exercise_id");
            entity.Property(e => e.WesId).HasColumnName("wes_id");
            entity.Property(e => e.WesRestTimeAfterExerc)
                .HasColumnType("time")
                .HasColumnName("wes_rest_time_after_exerc");
            entity.Property(e => e.WesRestTimeBtwSetsTotal)
                .HasColumnType("time")
                .HasColumnName("wes_rest_time_btw_sets_total");
            entity.Property(e => e.WesSetsNum).HasColumnName("wes_sets_num");
            entity.Property(e => e.WesTotalEffort).HasColumnName("wes_total_effort");
            entity.Property(e => e.WesTotalReps).HasColumnName("wes_total_reps");
            entity.Property(e => e.WesTotalWeightLifted).HasColumnName("wes_total_weight_lifted");
            entity.Property(e => e.WesUsersWeekStatisticsId).HasColumnName("wes_users_week_statistics_id");
            entity.Property(e => e.WesWeekStart).HasColumnName("wes_week_start");

            entity.HasOne(d => d.WesExercise).WithMany(p => p.WeekExercisesStatistics)
                .HasForeignKey(d => d.WesExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Week_Exercises_Statistics_Exercise1");

            entity.HasOne(d => d.WesWkst).WithMany(p => p.WeekExercisesStatistics)
                .HasForeignKey(d => d.WesWkstId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Week_Exercises_Statistics_Week_User_Statistics1");
        });

        modelBuilder.Entity<WeekUserStatistic>(entity =>
        {
            entity.HasKey(e => e.WkstId).HasName("PRIMARY");

            entity.ToTable("week_user_statistics");

            entity.HasIndex(e => e.WkstUsId, "fk_Week_User_Statistics_User1_idx");

            entity.Property(e => e.WkstId)
                .ValueGeneratedNever()
                .HasColumnName("wkst_id");
            entity.Property(e => e.WkstEndUserWeight).HasColumnName("wkst_end_user_weight");
            entity.Property(e => e.WkstExercisesNumber).HasColumnName("wkst_exercises_number");
            entity.Property(e => e.WkstRestTimeBtwExercTotal)
                .HasColumnType("time")
                .HasColumnName("wkst_rest_time_btw_exerc_total");
            entity.Property(e => e.WkstRestTimeBtwSetsTotal)
                .HasColumnType("time")
                .HasColumnName("wkst_rest_time_btw_sets_total");
            entity.Property(e => e.WkstSequenceNumber).HasColumnName("wkst_sequence_number");
            entity.Property(e => e.WkstStartUserWeight).HasColumnName("wkst_start_user_weight");
            entity.Property(e => e.WkstTotalEffort).HasColumnName("wkst_total_effort");
            entity.Property(e => e.WkstTotalReps).HasColumnName("wkst_total_reps");
            entity.Property(e => e.WkstTotalSets).HasColumnName("wkst_total_sets");
            entity.Property(e => e.WkstTotalWeightLifted).HasColumnName("wkst_total_weight_lifted");
            entity.Property(e => e.WkstTrainingDurationTotal)
                .HasColumnType("time")
                .HasColumnName("wkst_training_duration_total");
            entity.Property(e => e.WkstTrainingNumber).HasColumnName("wkst_training_number");
            entity.Property(e => e.WkstUsId).HasColumnName("wkst_us_id");
            entity.Property(e => e.WkstWeekStart).HasColumnName("wkst_week_start");

            entity.HasOne(d => d.WkstUs).WithMany(p => p.WeekUserStatistics)
                .HasForeignKey(d => d.WkstUsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Week_User_Statistics_User1");
        });

        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(e => e.WId).HasName("PRIMARY");

            entity.ToTable("workout");

            entity.HasIndex(e => e.WUserId, "fk_Workout_User_idx");

            entity.Property(e => e.WId)
                .ValueGeneratedNever()
                .HasColumnName("w_id");
            entity.Property(e => e.WDate).HasColumnName("w_date");
            entity.Property(e => e.WEndTime)
                .HasColumnType("time")
                .HasColumnName("w_end_time");
            entity.Property(e => e.WSequenceNumber).HasColumnName("w_sequence_number");
            entity.Property(e => e.WStartTime)
                .HasColumnType("time")
                .HasColumnName("w_start_time");
            entity.Property(e => e.WTotalDuration)
                .HasColumnType("time")
                .HasColumnName("w_total_duration");
            entity.Property(e => e.WUserId).HasColumnName("w_user_id");

            entity.HasOne(d => d.WUser).WithMany(p => p.Workouts)
                .HasForeignKey(d => d.WUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Workout_User");
        });

        modelBuilder.Entity<WorkoutExercise>(entity =>
        {
            entity.HasKey(e => new { e.WeWexerciseId, e.WeId })
                .HasName("PRIMARY")
                .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

            entity.ToTable("workout_exercise");

            entity.HasIndex(e => e.WeWorkoutId, "fk_Exercise_Workout1_idx");

            entity.HasIndex(e => e.WeWexerciseId, "fk_Workout_Exercise_Exercise1_idx");

            entity.Property(e => e.WeWexerciseId).HasColumnName("we_wexercise_id");
            entity.Property(e => e.WeId).HasColumnName("we_id");
            entity.Property(e => e.WeRestTimeAfterExercise)
                .HasColumnType("time")
                .HasColumnName("we_rest_time_after_exercise");
            entity.Property(e => e.WeSequenceNumber).HasColumnName("we_sequence_number");
            entity.Property(e => e.WeWorkoutId).HasColumnName("we_workout_id");

            entity.HasOne(d => d.WeWexercise).WithMany(p => p.WorkoutExercises)
                .HasForeignKey(d => d.WeWexerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Workout_Exercise_Exercise1");

            entity.HasOne(d => d.WeWorkout).WithMany(p => p.WorkoutExercises)
                .HasForeignKey(d => d.WeWorkoutId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_Exercise_Workout1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
