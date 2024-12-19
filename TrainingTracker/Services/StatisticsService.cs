using Microsoft.EntityFrameworkCore;
using TrainingTracker.Models;

namespace TrainingTracker.Services
{
    public interface IStatisticsService
    {
        Task<UsersWeekStatisticsTotal> GetAggregatedStatisticsAsync(int exerciseId);
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly MydbContext _context;

        public StatisticsService()
        {
            _context = new MydbContext();
        }

        public async Task<UsersWeekStatisticsTotal> GetAggregatedStatisticsAsync(int exerciseId)
        {
            // Получаем текущую дату
            DateTime currentDate = DateTime.UtcNow;

            // Вычисляем дату неделю назад
            DateTime weekAgo = currentDate.AddDays(-7);

            // Преобразуем в DateOnly для дальнейшего использования в поле UwsWeekStart
            DateOnly currentDateOnly = DateOnly.FromDateTime(currentDate);
            DateOnly weekAgoDateOnly = DateOnly.FromDateTime(weekAgo);

            // Основной запрос для выборки статистики по упражнению за последнюю неделю
            var result = await (from we in _context.WorkoutExercises
                                join w in _context.Workouts on we.WeWorkoutId equals w.WId
                                join s in _context.Sets on we.WeId equals s.SWeId
                                join eq in _context.Equipment on s.SId equals eq.EqSetId
                                where we.WeWexerciseId == exerciseId
                                && w.WDate >= weekAgoDateOnly && w.WDate <= currentDateOnly
                                select new
                                {
                                    workout = w,
                                    workoutExercise = we,
                                    set = s,
                                    equipment = eq
                                }).ToListAsync();

            if (!result.Any())
                return null;

            long totalWexerciseNum = 0;
            long totalSetsNum = 0;
            long totalEffort = 0;
            long totalRepsNum = 0;
            long totalRestTimeBtwSetsSec = 0;
            long totalRestTimeAfterExercSec = 0;
            double totalWeightLifted = 0;
            HashSet<long> uniqueUserIds = new HashSet<long>();

            foreach (var item in result)
            {
                var workout = item.workout;
                var workoutExercise = item.workoutExercise;
                var set = item.set;
                var equipment = item.equipment;

                // Считаем количество уникальных WorkoutExercise
                if (workoutExercise != null)
                {
                    totalWexerciseNum += 1;
                }

                // Суммируем количество Set
                if (set != null)
                {
                    totalSetsNum += 1;
                    totalEffort += set.SEffort ?? 0;
                    totalRepsNum += set.SRepsDone ?? 0;
                    totalRestTimeBtwSetsSec += (set.SRestTimeAfterSet?.Second ?? 0);
                }

                // Суммируем время отдыха после упражнения
                if (workoutExercise != null)
                {
                    totalRestTimeAfterExercSec += (workoutExercise.WeRestTimeAfterExercise?.Second ?? 0);
                }

                // Суммируем веса из Equipment
                if (equipment != null)
                {
                    totalWeightLifted += equipment.EqWeight ?? 0;
                }

                // Добавляем уникальные ID пользователей
                if (workout != null)
                {
                    uniqueUserIds.Add(workout.WUserId);
                }
            }

            // Получаем количество уникальных пользователей
            int uniqueUsersCount = uniqueUserIds.Count;

            var aggregatedStatistics = new UsersWeekStatisticsTotal
            {
                UwsExerciseId = exerciseId,
                UwsWexerciseNum = totalWexerciseNum,
                UwsSetsNum = totalSetsNum,
                UwsUsersHaveDoneNum = uniqueUsersCount,
                UwsTotalEffort = totalEffort,
                UwsRepsNum = totalRepsNum,
                UwsRestTimeBtwSetsSec = totalRestTimeBtwSetsSec,
                UwsRestTimeAfterExercSec = totalRestTimeAfterExercSec,
                UwsWeightLifted = totalWeightLifted,
                UwsWeekStart = weekAgoDateOnly
            };

            return aggregatedStatistics;
        }
    }

}
