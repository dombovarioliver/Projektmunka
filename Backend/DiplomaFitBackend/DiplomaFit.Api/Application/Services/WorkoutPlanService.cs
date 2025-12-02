using DiplomaFit.Api.Application.Dtos.Workout;
using DiplomaFit.Api.Application.Interfaces;
using DiplomaFit.Api.Domain.Entities;
using DiplomaFit.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Api.Application.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly AppDbContext _dbContext;
        private readonly IMlWorkoutSplitClient _mlClient;
        private readonly Random _random = new();

        public WorkoutPlanService(AppDbContext dbContext, IMlWorkoutSplitClient mlClient)
        {
            _dbContext = dbContext;
            _mlClient = mlClient;
        }

        public async Task<WeeklyWorkoutPlanDto> GenerateWeeklyPlanAsync(
            WorkoutPlanRequestDto request,
            CancellationToken ct = default)
        {
            // Ha 0 nap / hét – csak pihenőnapok, ML-t sem hívjuk
            if (request.DaysPerWeek <= 0)
            {
                var restOnly = new WeeklyWorkoutPlanDto
                {
                    SplitName = "RestOnly",
                    DaysPerWeek = 0
                };

                for (int i = 1; i <= 7; i++)
                {
                    restOnly.Days.Add(CreateRestDay(i));
                }

                return restOnly;
            }

            // 1) ML-ből kérünk splitet
            var split = await _mlClient.PredictSplitAsync(request, ct);

            var plan = new WeeklyWorkoutPlanDto
            {
                SplitName = split.SplitName,
                DaysPerWeek = request.DaysPerWeek
            };

            // 2) Split logika
            switch (split.SplitName)
            {
                case "PushPullLegs":
                    GeneratePplSplit(plan, request);
                    break;
                case "UpperLower":
                    GenerateUpperLowerSplit(plan, request);
                    break;
                case "FullBody":
                default:
                    GenerateFullBodySplit(plan, request);
                    break;
            }

            // Ha valamiért kevesebb mint 7 napot tettünk bele, töltsük fel Resttel
            for (int i = plan.Days.Count + 1; i <= 7; i++)
            {
                plan.Days.Add(CreateRestDay(i));
            }

            return plan;
        }

        // --- SPLIT GENERÁLÓK ---

        /// <summary>
        /// Push–Pull–Legs split:
        /// P – Pu – L – Rest, majd ismétlődik, max. DaysPerWeek edzésnappal.
        /// </summary>
        private void GeneratePplSplit(WeeklyWorkoutPlanDto plan, WorkoutPlanRequestDto request)
        {
            int targetWorkouts = Math.Clamp(request.DaysPerWeek, 0, 6);
            int workoutsUsed = 0;
            int dayIndex = 1;

            while (dayIndex <= 7)
            {
                // PUSH
                if (workoutsUsed < targetWorkouts && dayIndex <= 7)
                {
                    plan.Days.Add(CreatePushDay(dayIndex++, request));
                    workoutsUsed++;
                }
                else if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                    continue;
                }

                // PULL
                if (workoutsUsed < targetWorkouts && dayIndex <= 7)
                {
                    plan.Days.Add(CreatePullDay(dayIndex++, request));
                    workoutsUsed++;
                }
                else if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                    continue;
                }

                // LEGS
                if (workoutsUsed < targetWorkouts && dayIndex <= 7)
                {
                    plan.Days.Add(CreateLegsDay(dayIndex++, request));
                    workoutsUsed++;
                }
                else if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                    continue;
                }

                // PPL blokk után kötelező pihenő
                if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                }
            }
        }

        /// <summary>
        /// Upper–Lower split:
        /// Upper – Lower – Rest, majd ismétlődik, max. DaysPerWeek edzésnappal.
        /// (Így minden UL blokk után 1 pihenőnap, a hét végén lesz extra Rest.)
        /// </summary>
        private void GenerateUpperLowerSplit(WeeklyWorkoutPlanDto plan, WorkoutPlanRequestDto request)
        {
            int targetWorkouts = Math.Clamp(request.DaysPerWeek, 0, 6);
            int workoutsUsed = 0;
            int dayIndex = 1;

            while (dayIndex <= 7)
            {
                // UPPER
                if (workoutsUsed < targetWorkouts && dayIndex <= 7)
                {
                    plan.Days.Add(CreateUpperDay(dayIndex++, request));
                    workoutsUsed++;
                }
                else if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                    continue;
                }

                // LOWER
                if (workoutsUsed < targetWorkouts && dayIndex <= 7)
                {
                    plan.Days.Add(CreateLowerDay(dayIndex++, request));
                    workoutsUsed++;
                }
                else if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                    continue;
                }

                // UL blokk után legalább egy pihenő
                if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                }
            }
        }

        /// <summary>
        /// Full body split:
        /// FullBody – Rest ciklus, max. DaysPerWeek edzésnappal.
        /// Minden teljes testes nap után legalább 1 pihenőnap.
        /// </summary>
        private void GenerateFullBodySplit(WeeklyWorkoutPlanDto plan, WorkoutPlanRequestDto request)
        {
            int targetWorkouts = Math.Clamp(request.DaysPerWeek, 0, 4);
            int workoutsUsed = 0;
            int dayIndex = 1;

            while (dayIndex <= 7)
            {
                // FULL BODY
                if (workoutsUsed < targetWorkouts && dayIndex <= 7)
                {
                    plan.Days.Add(CreateFullBodyDay(dayIndex++, request));
                    workoutsUsed++;
                }
                else if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                    continue;
                }

                // utána kötelező pihenőnap
                if (dayIndex <= 7)
                {
                    plan.Days.Add(CreateRestDay(dayIndex++));
                }
            }
        }

        // --- NAPI SÉMÁK ---

        private WorkoutDayDto CreateRestDay(int dayIndex) =>
            new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Rest",
                Exercises = new List<WorkoutExerciseDto>()
            };

        private WorkoutDayDto CreatePushDay(int dayIndex, WorkoutPlanRequestDto request)
        {
            var day = new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Push"
            };

            // 1. Két mellnyomás (sima/döntött, gép, stb.) – PRIORITÁS: compound press
            var pressPicked = PickChestPresses(2, request);
            if (!pressPicked.Any())
            {
                // ha valamiért még sincs compound chest, essünk vissza izolációkra
                var chestFallback = PickExercises("chest", "Push", request, isCompound: false, count: 2)
                    .ToList();
                pressPicked = chestFallback;
            }
            day.Exercises.AddRange(pressPicked);

            // --- a további rész maradhat, amit korábban írtunk ---
            // 2. 1–2 tricepsz
            var trisAll = PickExercises("triceps", "Push", request, isCompound: false, count: 3)
                .ToList();
            var trisPicked = TrimList(trisAll, minCount: 1, maxCount: 2);
            day.Exercises.AddRange(trisPicked);

            // 3. Elülső váll
            var frontDelt = PickExercises("shoulders", "Push", request,
                isCompound: false, count: 2, subgroup: "shoulders_front")
                .FirstOrDefault();
            if (frontDelt != null)
                day.Exercises.Add(frontDelt);

            // 4. Oldalsó váll – FIGYELEM: CSV-ben shoulders_side az érték!
            var sideDelt = PickExercises("shoulders", "Push", request,
                isCompound: false, count: 2, subgroup: "shoulders_side")
                .FirstOrDefault();
            if (sideDelt != null)
                day.Exercises.Add(sideDelt);

            return day;
        }

        private WorkoutDayDto CreatePullDay(int dayIndex, WorkoutPlanRequestDto request)
        {
            var day = new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Pull"
            };

            // 1. Lehúzás / húzódzkodás – vertikális húzás (pl. lat pull, pull-up)
            var verticalBack = _dbContext.Exercises.AsNoTracking()
                .Where(e => e.PrimaryMuscleGroup == "back"
                            && e.PushPullCategory == "Pull"
                            && e.Pattern == "vertical_pull"
                            && e.DifficultyLevel <= request.Experience)
                .ToList();

            verticalBack = ApplyEquipmentFilter(verticalBack, request);
            var verticalPick = verticalBack
                .OrderBy(_ => _random.Next())
                .Take(1)
                .Select(ToWorkoutExerciseDto)
                .FirstOrDefault();
            if (verticalPick != null)
                day.Exercises.Add(verticalPick);

            // 2. Evezés – horizontális húzás
            var horizontalBack = _dbContext.Exercises.AsNoTracking()
                .Where(e => e.PrimaryMuscleGroup == "back"
                            && e.PushPullCategory == "Pull"
                            && e.Pattern == "horizontal_pull"
                            && e.DifficultyLevel <= request.Experience)
                .ToList();

            horizontalBack = ApplyEquipmentFilter(horizontalBack, request);
            var horizontalPick = horizontalBack
                .OrderBy(_ => _random.Next())
                .Take(1)
                .Select(ToWorkoutExerciseDto)
                .FirstOrDefault();
            if (horizontalPick != null)
                day.Exercises.Add(horizontalPick);

            // 3. 1–2 bicepsz
            var bicepsAll = PickExercises("biceps", "Pull", request, isCompound: false, count: 3)
                .ToList();
            var bicepsPicked = TrimList(bicepsAll, minCount: 1, maxCount: 2);
            day.Exercises.AddRange(bicepsPicked);

            // 4. 1 hátsó váll
            var rearDelt = PickExercises("shoulders", "Pull", request, isCompound: false, count: 2, subgroup: "shoulders_rear")
                .FirstOrDefault();
            if (rearDelt != null)
                day.Exercises.Add(rearDelt);

            return day;
        }

        private WorkoutDayDto CreateLegsDay(int dayIndex, WorkoutPlanRequestDto request)
        {
            var day = new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Legs"
            };

            BuildLegTemplate(day, request);
            return day;
        }

        private WorkoutDayDto CreateUpperDay(int dayIndex, WorkoutPlanRequestDto request)
        {
            var day = new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Upper"
            };

            // 1–2 hát
            var backComp = PickExercises("back", "Pull", request, isCompound: true, count: 2).ToList();
            var backIso = PickExercises("back", "Pull", request, isCompound: false, count: 2).ToList();
            var backAll = backComp.Concat(backIso).ToList();
            backAll = TrimList(backAll, minCount: 1, maxCount: 2);
            day.Exercises.AddRange(backAll);

            // 1–2 mell – PRIORITÁS: compound press
            var chestPresses = PickChestPresses(2, request);
            if (!chestPresses.Any())
            {
                var chestFallback = PickExercises("chest", "Push", request, isCompound: false, count: 2)
                    .ToList();
                chestPresses = TrimList(chestFallback, minCount: 1, maxCount: 2);
            }
            else
            {
                chestPresses = TrimList(chestPresses, minCount: 1, maxCount: 2);
            }
            day.Exercises.AddRange(chestPresses);

            // 1 tricepsz
            var tri = PickExercises("triceps", "Push", request, isCompound: false, count: 2)
                .FirstOrDefault();
            if (tri != null)
                day.Exercises.Add(tri);

            // 1 bicepsz
            var bi = PickExercises("biceps", "Pull", request, isCompound: false, count: 2)
                .FirstOrDefault();
            if (bi != null)
                day.Exercises.Add(bi);

            // 1–1 váll (első, oldalsó, hátsó)
            var frontDelt = PickExercises("shoulders", "Push", request,
                isCompound: false, count: 2, subgroup: "shoulders_front")
                .FirstOrDefault();
            var sideDelt = PickExercises("shoulders", "Push", request,
                isCompound: false, count: 2, subgroup: "shoulders_side") // fontos: side, nem lateral
                .FirstOrDefault();
            var rearDelt = PickExercises("shoulders", "Pull", request,
                isCompound: false, count: 2, subgroup: "shoulders_rear")
                .FirstOrDefault();

            if (frontDelt != null) day.Exercises.Add(frontDelt);
            if (sideDelt != null) day.Exercises.Add(sideDelt);
            if (rearDelt != null) day.Exercises.Add(rearDelt);

            return day;
        }

        private WorkoutDayDto CreateLowerDay(int dayIndex, WorkoutPlanRequestDto request)
        {
            var day = new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "Lower"
            };

            BuildLegTemplate(day, request);
            return day;
        }

        private WorkoutDayDto CreateFullBodyDay(int dayIndex, WorkoutPlanRequestDto request)
        {
            var day = new WorkoutDayDto
            {
                DayIndex = dayIndex,
                DayType = "FullBody"
            };

            // 1 hát
            var back = PickExercises("back", "Pull", request, isCompound: true, count: 1).ToList();
            if (!back.Any())
            {
                back = PickExercises("back", "Pull", request, isCompound: false, count: 1).ToList();
            }

            // 1 mell
            var chest = PickExercises("chest", "Push", request, isCompound: true, count: 1).ToList();
            if (!chest.Any())
            {
                chest = PickExercises("chest", "Push", request, isCompound: false, count: 1).ToList();
            }

            // 1 összetett láb (guggolás/felhúzás jelleg – a CSV-ben lévő compound legs)
            var legs = PickExercises("legs", "Legs", request, isCompound: true, count: 1).ToList();

            // 1 tricepsz
            var tri = PickExercises("triceps", "Push", request, isCompound: false, count: 1).ToList();

            // 1 bicepsz
            var bi = PickExercises("biceps", "Pull", request, isCompound: false, count: 1).ToList();

            // 1–1 váll (első, oldalsó, hátsó)
            var frontDelt = PickExercises("shoulders", "Push", request, isCompound: false, count: 1, subgroup: "shoulders_front")
                .ToList();
            var sideDelt = PickExercises("shoulders", "Push", request, isCompound: false, count: 1, subgroup: "shoulders_side")
                .ToList();
            var rearDelt = PickExercises("shoulders", "Pull", request, isCompound: false, count: 1, subgroup: "shoulders_rear")
                .ToList();

            day.Exercises.AddRange(back);
            day.Exercises.AddRange(chest);
            day.Exercises.AddRange(legs);
            day.Exercises.AddRange(tri);
            day.Exercises.AddRange(bi);
            day.Exercises.AddRange(frontDelt);
            day.Exercises.AddRange(sideDelt);
            day.Exercises.AddRange(rearDelt);

            return day;
        }

        private List<WorkoutExerciseDto> PickChestPresses(int maxCount, WorkoutPlanRequestDto request)
        {
            // 1) Alap query az adatbázisban
            var query = _dbContext.Exercises.AsNoTracking()
                .Where(e => e.PrimaryMuscleGroup == "chest"
                            && e.PushPullCategory == "Push"
                            && e.IsCompound);

            // 2) Home-only szűrés
            if (request.EquipmentLevel == 0)
            {
                query = query.Where(e => e.IsHomeFriendly);
            }

            // 3) Ne legyen túl nehéz a tapasztalati szinthez képest
            query = query.Where(e => e.DifficultyLevel <= request.Experience + 1);

            // 4) LEKÉRÉS A DB-BŐL – INNENTŐL MEMÓRIA
            var candidates = query.ToList();

            if (!candidates.Any())
                return new List<WorkoutExerciseDto>();

            // 5) Véletlen sorrend + limitálás MEMÓRIÁBAN
            var picked = candidates
                .OrderBy(_ => _random.Next())
                .Take(maxCount)
                .Select(ToWorkoutExerciseDto)
                .ToList();

            return picked;
        }

        private void BuildLegTemplate(WorkoutDayDto day, WorkoutPlanRequestDto request)
        {
            var exercises = new List<WorkoutExerciseDto>();

            // 1) 1–2 összetett láb (guggolás / felhúzás / lábtoló / stb.)
            var compounds = PickExercises("legs", "Legs", request, isCompound: true, count: 5)
                .ToList();

            if (!compounds.Any())
            {
                // Ha nincs összetett lábgyakorlat, essünk vissza a legjobb elérhető izolációkra
                compounds = PickExercises("legs", "Legs", request, isCompound: false, count: 5)
                    .ToList();
            }

            var pickedCompounds = TrimList(compounds, minCount: compounds.Any() ? 1 : 0, maxCount: 2);
            exercises.AddRange(pickedCompounds);



            // 2) Izolációk az egyes izomcsoportokra
            var isolations = new List<WorkoutExerciseDto>();

            WorkoutExerciseDto? PickIso(string subgroup)
            {
                var exactMatch = PickExercises("legs", "Legs", request, isCompound: false, count: 5, subgroup: subgroup)
                    .FirstOrDefault();

                if (exactMatch != null)
                    return exactMatch;

                return PickExercises("legs", "Legs", request, isCompound: false, count: 5)
                    .FirstOrDefault();
            }

            void AddIfUnique(WorkoutExerciseDto? exercise)
            {
                if (exercise != null &&
                    exercises.All(e => e.ExerciseId != exercise.ExerciseId) &&
                    isolations.All(e => e.ExerciseId != exercise.ExerciseId))
                {
                    isolations.Add(exercise);
                }
            }

            AddIfUnique(PickIso("legs_quads"));
            AddIfUnique(PickIso("legs_hamstrings"));
            AddIfUnique(PickIso("calves"));

            // 3) Ha az izolációkból hiányzik valamelyik izomcsoport, próbáljunk meg alternatívát keresni
            WorkoutExerciseDto? FallbackIso()
            {
                return PickExercises("legs", "Legs", request, isCompound: false, count: 10)
                    .FirstOrDefault(e => exercises.All(x => x.ExerciseId != e.ExerciseId) &&
                                         isolations.All(x => x.ExerciseId != e.ExerciseId));
            }

            while (isolations.Count < 3)
            {
                var extra = FallbackIso();
                if (extra == null)
                {
                    break;
                }

                AddIfUnique(extra);
            }

            //day.Exercises.AddRange(exercises);
            // Végső izolációs lista 3–4 elem között, ha elérhető
            var finalizedIsolations = TrimList(
                isolations,
                minCount: isolations.Count >= 3 ? 3 : isolations.Count,
                maxCount: Math.Min(4, isolations.Count));

            day.Exercises.AddRange(pickedCompounds);
            day.Exercises.AddRange(finalizedIsolations);
        }

        private List<Exercise> ApplyEquipmentFilter(List<Exercise> list, WorkoutPlanRequestDto request)
        {
            if (request.EquipmentLevel == 0)
            {
                // otthon: csak home-friendly
                return list.Where(e => e.IsHomeFriendly).ToList();
            }

            // kondiban: mindent engedünk (gépek, rúd, kézi súly, bodyweight)
            return list;
        }

        private WorkoutExerciseDto ToWorkoutExerciseDto(Exercise e)
        {
            return new WorkoutExerciseDto
            {
                ExerciseId = e.ExerciseId,
                NameHu = e.NameHu,
                Sets = e.DefaultSets,
                RepsLow = e.DefaultRepsLow,
                RepsHigh = e.DefaultRepsHigh
            };
        }
        private List<WorkoutExerciseDto> TrimList(
            List<WorkoutExerciseDto> items,
            int minCount,
            int maxCount)
        {
            if (items.Count == 0)
                return items;

            int upper = Math.Min(maxCount, items.Count);
            if (upper <= 0)
                return new List<WorkoutExerciseDto>();

            int lower = Math.Min(minCount, upper);
            int desired = _random.Next(lower, upper + 1);

            return items
                .OrderBy(_ => _random.Next())
                .Take(desired)
                .ToList();
        }

        //VÁLASZTÁS AZ Exercises TÁBLÁBÓL

        private IEnumerable<WorkoutExerciseDto> PickExercises(
            string primaryMuscleGroup,
            string pushPullCategory,
            WorkoutPlanRequestDto request,
            bool isCompound,
            int count,
            string? subgroup = null)
        {
            IQueryable<Exercise> query = _dbContext.Exercises.AsNoTracking()
                .Where(e => e.PrimaryMuscleGroup == primaryMuscleGroup)
                //.Where(e => e.PushPullCategory == pushPullCategory)
                .Where(e => e.IsCompound == isCompound)
                .Where(e => e.DifficultyLevel <= request.Experience + 1);


            // A lábgyakorlatoknál ne bukjunk el a Push/Pull kategórián –
            // sok lábmozdulatnál ez üres vagy másképp jelölt. A többi
            // izomcsoportnál marad a szigorú egyezés.
            if (!string.Equals(primaryMuscleGroup, "legs", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(e => e.PushPullCategory == pushPullCategory);
            }


            // HOME ONLY (equipment_level == 0): csak otthon végezhető gyakorlatok
            if (request.EquipmentLevel == 0)
            {
                query = query.Where(e => e.IsHomeFriendly);
            }
            else
            {
                // GYM: bodyweight gyakorlatok közül csak a kivételek mehetnek
                string[] allowedBodyweightEn =
                {
                    "Parallel Bar Dip",
                    "Pull-Up",
                    "Chin-Up",
                    "Plank"
                };

                string[] allowedBodyweightHu =
                {
                    "Tolódzkodás párhuzamos rúdon",
                    "Húzódzkodás",
                    "Plank"
                };

                query = query.Where(e =>
                        e.Equipment != "bodyweight" ||
                        allowedBodyweightEn.Contains(e.NameEn) ||
                        allowedBodyweightHu.Contains(e.NameHu));
                }

            if (!string.IsNullOrEmpty(subgroup))
            {
                query = query.Where(e => e.PrimaryMuscleSubgroup == subgroup);
            }

            var list = query.ToList();

            if (!list.Any())
                return Enumerable.Empty<WorkoutExerciseDto>();

            var picked = list
                .OrderBy(_ => _random.Next())
                .Take(count)
                .Select(e => new WorkoutExerciseDto
                {
                    ExerciseId = e.ExerciseId,
                    NameHu = e.NameHu,
                    Sets = e.DefaultSets,
                    RepsLow = e.DefaultRepsLow,
                    RepsHigh = e.DefaultRepsHigh
                })
                .ToList();

            return picked;
        }
    }
}
