using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Workout;
using DiplomaFit.Model.Entities;

namespace DiplomaFit.Service.WorkoutService
{
    public class WorkoutPlanService
    {
        private readonly ExerciseRepository _exerciseRepository;
        private readonly WorkoutSplitMlClientService _workoutSplitMlClientService;
        private readonly Random _random = new();

        public WorkoutPlanService(
            ExerciseRepository exerciseRepository,
            WorkoutSplitMlClientService workoutSplitMlClientService)
        {
            _exerciseRepository = exerciseRepository;
            _workoutSplitMlClientService = workoutSplitMlClientService;
        }

        public async Task<WeeklyWorkoutPlanDto> GenerateWeeklyPlanAsync(
            WorkoutPlanRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (request.DaysPerWeek <= 0)
                return GenerateRestOnlyPlan();

            var split = await _workoutSplitMlClientService.PredictSplitAsync(request, cancellationToken);

            var plan = new WeeklyWorkoutPlanDto
            {
                SplitName = split.SplitName,
                DaysPerWeek = request.DaysPerWeek
            };

            switch (split.SplitName)
            {
                case "PushPullLegs":
                    GeneratePushPullLegs(plan, request);
                    break;

                case "UpperLower":
                    GenerateUpperLower(plan, request);
                    break;

                case "FullBody":
                default:
                    GenerateFullBody(plan, request);
                    break;
            }

            while (plan.Days.Count < 7)
            {
                plan.Days.Add(CreateRestDay(plan.Days.Count + 1));
            }

            return plan;
        }

        private WeeklyWorkoutPlanDto GenerateRestOnlyPlan()
        {
            var plan = new WeeklyWorkoutPlanDto
            {
                SplitName = "RestOnly",
                DaysPerWeek = 0
            };

            for (int i = 1; i <= 7; i++)
            {
                plan.Days.Add(CreateRestDay(i));
            }

            return plan;
        }

        private void GeneratePushPullLegs(WeeklyWorkoutPlanDto plan, WorkoutPlanRequestDto request)
        {
            var pattern = new[] { "Push", "Pull", "Legs", "Rest" };
            FillPlanByPattern(plan, request, pattern);
        }

        private void GenerateUpperLower(WeeklyWorkoutPlanDto plan, WorkoutPlanRequestDto request)
        {
            var pattern = new[] { "Upper", "Lower", "Rest" };
            FillPlanByPattern(plan, request, pattern);
        }

        private void GenerateFullBody(WeeklyWorkoutPlanDto plan, WorkoutPlanRequestDto request)
        {
            var pattern = new[] { "FullBody", "Rest" };
            FillPlanByPattern(plan, request, pattern);
        }

        private void FillPlanByPattern(
            WeeklyWorkoutPlanDto plan,
            WorkoutPlanRequestDto request,
            string[] pattern)
        {
            int workoutDaysUsed = 0;
            int dayIndex = 1;
            int patternIndex = 0;

            while (dayIndex <= 7)
            {
                var dayType = pattern[patternIndex % pattern.Length];

                if (dayType == "Rest" || workoutDaysUsed >= request.DaysPerWeek)
                {
                    plan.Days.Add(CreateRestDay(dayIndex));
                }
                else
                {
                    plan.Days.Add(CreateWorkoutDay(dayIndex, dayType, request));
                    workoutDaysUsed++;
                }

                dayIndex++;
                patternIndex++;
            }
        }

        private WorkoutDayDto CreateWorkoutDay(
            int dayIndex,
            string dayType,
            WorkoutPlanRequestDto request)
        {
            var exercises = GetExercisesForDay(dayType, request);

            return new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = dayType,
                Exercises = exercises
                    .Select(MapToWorkoutExerciseDto)
                    .ToList()
            };
        }

        private WorkoutDayDto CreateRestDay(int dayIndex)
        {
            return new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Rest"
            };
        }

        private List<Exercise> GetExercisesForDay(
            string dayType,
            WorkoutPlanRequestDto request)
        {
            var exercises = _exerciseRepository
                .GetAll()
                .Where(e => e.MinExperienceLevel <= request.Experience)
                .ToList();

            if (request.EquipmentLevel == 0)
            {
                exercises = exercises
                    .Where(e => e.IsHomeFriendly)
                    .ToList();
            }

            exercises = dayType switch
            {
                "Push" => exercises
                    .Where(e => e.PushPullCategory == "Push")
                    .ToList(),

                "Pull" => exercises
                    .Where(e => e.PushPullCategory == "Pull")
                    .ToList(),

                "Legs" => exercises
                    .Where(e => e.PushPullCategory == "Legs")
                    .ToList(),

                "Upper" => exercises
                    .Where(e => e.PushPullCategory == "Push" || e.PushPullCategory == "Pull")
                    .ToList(),

                "Lower" => exercises
                    .Where(e => e.PushPullCategory == "Legs")
                    .ToList(),

                "FullBody" => exercises,

                _ => exercises
            };

            return exercises
                .OrderBy(_ => _random.Next())
                .Take(6)
                .ToList();
        }

        private WorkoutExerciseDto MapToWorkoutExerciseDto(Exercise exercise)
        {
            return new WorkoutExerciseDto
            {
                ExerciseId = exercise.ExerciseId,
                NameHu = exercise.NameHu,
                Sets = exercise.DefaultSets,
                RepsLow = exercise.DefaultRepsLow,
                RepsHigh = exercise.DefaultRepsHigh
            };
        }
    }
}
