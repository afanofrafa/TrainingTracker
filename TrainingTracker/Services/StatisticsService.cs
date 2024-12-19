using Microsoft.EntityFrameworkCore;
using TrainingTracker.Models;

namespace TrainingTracker.Services
{
    public interface IStatisticsService
    {
        Task<UsersWeekStatisticsTotal> GetAggregatedStatisticsAsync(int exerciseId);
        Task DeleteAllUsersWeekStatisticsAsync();
        Task<List<UsersWeekStatisticsTotal>> GetAllUsersWeekStatisticsAsync();
    }

    public class StatisticsService : IStatisticsService
    {
        private readonly MydbContext _context;
        private int uwsId = 1;
        public StatisticsService()
        {
            _context = new MydbContext();
        }
        // Удаление всех записей UsersWeekStatisticsTotal
        public async Task DeleteAllUsersWeekStatisticsAsync()
        {
            var allRecords = await _context.UsersWeekStatisticsTotals.ToListAsync();
            _context.UsersWeekStatisticsTotals.RemoveRange(allRecords);
            await _context.SaveChangesAsync();
        }

        // Получение всех записей UsersWeekStatisticsTotal
        public async Task<List<UsersWeekStatisticsTotal>> GetAllUsersWeekStatisticsAsync()
        {
            return await _context.UsersWeekStatisticsTotals.ToListAsync();
        }
        public async Task<UsersWeekStatisticsTotal> GetAggregatedStatisticsAsync(int exerciseId)
        {
            bool exerciseExists = await _context.Exercises.AnyAsync(e => e.EId == exerciseId);
            if (!exerciseExists)
                return null;
            // Получаем текущую дату
            DateTime currentDate = DateTime.UtcNow;

            // Вычисляем дату неделю назад
            DateTime weekAgo = currentDate.AddDays(-7);

            // Преобразуем в DateOnly для дальнейшего использования в поле UwsWeekStart
            DateOnly currentDateOnly = DateOnly.FromDateTime(currentDate);
            DateOnly weekAgoDateOnly = DateOnly.FromDateTime(weekAgo);
            // Удаляем старые записи, если они существуют
            var existingRecords = await _context.UsersWeekStatisticsTotals
                .Where(uws => uws.UwsWeekStart == weekAgoDateOnly && uws.UwsExerciseId == exerciseId)
                .ToListAsync();

            if (existingRecords.Any())
            {
                var recordWithMinId = existingRecords.OrderBy(record => record.UwsId).FirstOrDefault();
                if (recordWithMinId != null) {
                    uwsId = recordWithMinId.UwsId;
                    if (uwsId < 1)
                        uwsId = 1;
                }
                _context.UsersWeekStatisticsTotals.RemoveRange(existingRecords);
                await _context.SaveChangesAsync();
            }
            else
            {
                var maxId = await _context.UsersWeekStatisticsTotals.MaxAsync(record => (int?)record.UwsId) ?? 0;
                uwsId = maxId + 1;
            }
            // Инициализация агрегированных данных
            long totalWexerciseNum = 0;
            long totalSetsNum = 0;
            long totalEffort = 0;
            long totalRepsNum = 0;
            long totalRestTimeBtwSetsSec = 0;
            long totalRestTimeAfterExercSec = 0;
            double totalWeightLifted = 0;
            HashSet<long> uniqueUserIds = new HashSet<long>();

            // Работа с WorkoutExercises
            var workoutExercises = await _context.WorkoutExercises
                .Where(we => we.WeWexerciseId == exerciseId)
                .ToListAsync();

            if (workoutExercises.Any())
            {
                totalWexerciseNum = workoutExercises.Count;

                // Суммируем время отдыха после упражнений
                totalRestTimeAfterExercSec = workoutExercises.Sum(we => we.WeRestTimeAfterExercise?.Second ?? 0);
            }
            
            // Работа с Workouts
            var workoutIds = workoutExercises.Select(we => we.WeWorkoutId).Distinct();
            var workouts = await _context.Workouts
                .Where(w => workoutIds.Contains(w.WId) && w.WDate >= weekAgoDateOnly && w.WDate <= currentDateOnly)
                .ToListAsync();

            if (workouts.Any())
            {
                // Уникальные пользователи
                uniqueUserIds.UnionWith(workouts.Select(w => w.WUserId));
            }

            // Работа с Sets
            var workoutExerciseIds = workoutExercises.Select(we => we.WeId).Distinct();
            var sets = await _context.Sets
                .Where(s => workoutExerciseIds.Contains(s.SWeId))
                .ToListAsync();

            if (sets.Any())
            {
                totalSetsNum = sets.Count;
                totalEffort = sets.Sum(s => s.SEffort ?? 0);
                totalRepsNum = sets.Sum(s => s.SRepsDone ?? 0);
                totalRestTimeBtwSetsSec = sets.Sum(s => s.SRestTimeAfterSet?.Second ?? 0);
            }

            // Работа с Equipment
            var setIds = sets.Select(s => s.SId).Distinct();
            var equipment = await _context.Equipment
                .Where(eq => setIds.Contains(eq.EqSetId))
                .ToListAsync();

            if (equipment.Any())
            {
                totalWeightLifted = equipment.Sum(eq => eq.EqWeight ?? 0);
            }

            // Получаем количество уникальных пользователей
            int uniqueUsersCount = uniqueUserIds.Count;
            // Формирование результата
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
                UwsWeekStart = weekAgoDateOnly,
                UwsId = uwsId
            };
            await _context.UsersWeekStatisticsTotals.AddAsync(aggregatedStatistics);
            await _context.SaveChangesAsync();
            return aggregatedStatistics;
        }
    }

}
