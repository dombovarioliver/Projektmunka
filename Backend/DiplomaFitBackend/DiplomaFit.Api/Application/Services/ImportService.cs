using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DiplomaFit.Api.Application.Interfaces;
using DiplomaFit.Api.Domain.Entities;
using DiplomaFit.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;


namespace DiplomaFit.Api.Application.Services
{
    public class ImportService : IImportService
    {
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public ImportService(AppDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private string MapPath(string path) 
        {
            if (Path.IsPathRooted(path))
                return path;

            return Path.Combine(_env.ContentRootPath,
                path.Replace('/', Path.DirectorySeparatorChar));
        }

        public async Task ImportPlansAndCasesAsync(string planCsvPath, string caseCsvPath)
        {
            planCsvPath = MapPath(planCsvPath);
            caseCsvPath = MapPath(caseCsvPath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            // Plan.csv beolvasása 
            List<PlanRow> planRows;
            using (var reader = new StreamReader(planCsvPath))
            using (var csv = new CsvReader(reader, config))
            {
                planRows = csv.GetRecords<PlanRow>().ToList();
            }

            //Case.csv beolvasása
            List<CaseRow> caseRows;
            using (var reader = new StreamReader(caseCsvPath))
            using (var csv = new CsvReader(reader, config))
            {
                caseRows = csv.GetRecords<CaseRow>().ToList();
            }

            if (planRows.Count != caseRows.Count)
                throw new Exception($"Plan sorok száma ({planRows.Count}) != Case sorok száma ({caseRows.Count})");

            // GUID összerendelés
            for (int i = 0; i < planRows.Count; i++)
            {
                var p = planRows[i];
                var c = caseRows[i];

                Guid planId = string.IsNullOrWhiteSpace(p.plan_id) || p.plan_id.ToUpper() == "NULL"
                    ? Guid.NewGuid()
                    : Guid.Parse(p.plan_id);

                Guid caseId = string.IsNullOrWhiteSpace(c.case_id) || c.case_id.ToUpper() == "NULL"
                    ? Guid.NewGuid()
                    : Guid.Parse(c.case_id);

                // Case plan_id → ha nincs, ugyanaz mint Plan
                if (string.IsNullOrWhiteSpace(c.plan_id) || c.plan_id.ToUpper() == "NULL")
                    c.plan_id = planId.ToString();

                // PLAN ENTITY
                var planEntity = new DietPlan
                {
                    PlanId = planId,
                    Calories = p.calories_kcal,
                    Protein = p.protein_g,
                    Carbs = p.carbs_g,
                    Fat = p.fat_g,
                    MealsPerDay = p.meals_per_day,
                    SnacksPerDay = p.snacks_per_day
                };
                _db.DietPlans.Add(planEntity);

                // CASE ENTITY
                var caseEntity = new Case
                {
                    CaseId = caseId,
                    Gender = MapGender(c.gender),
                    Age = c.age,
                    HeightCm = c.height_cm,
                    WeightKg = c.weight_kg,
                    BodyfatPercent = c.bodyfat_percent,
                    ActivityLevel = c.activity_level,
                    GoalType = MapGoalType(c.goal_type),
                    GoalDeltaKg = c.goal_delta_kg,
                    GoalTimeWeeks = c.goal_time_weeks,
                    PlanId = planId
                };
                _db.Cases.Add(caseEntity);
            }

            await _db.SaveChangesAsync();
        }

        // Foods import
        public async Task ImportFoodsAsync(string csvPath)
        {
            csvPath = MapPath(csvPath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(csvPath);
            using var csv = new CsvReader(reader, config);

            var rows = csv.GetRecords<FoodRow>().ToList();

            foreach (var r in rows)
            {
                Guid foodId =
                    string.IsNullOrWhiteSpace(r.food_id) || r.food_id.ToUpper() == "NULL"
                        ? Guid.NewGuid()
                        : Guid.Parse(r.food_id);

                var entity = await _db.Foods.FindAsync(foodId);
                if (entity == null)
                {
                    entity = new Food { FoodId = foodId };
                    _db.Foods.Add(entity);
                }

                entity.FoodNameHu = r.food_name_hu;
                entity.FoodNameEn = r.food_name_en;
                entity.KcalPer100 = r.kcal_per_100;
                entity.ProteinGPer100 = r.prot;
                entity.CarbsGPer100 = r.carbs;
                entity.FatGPer100 = r.fat;
                entity.MealType = string.IsNullOrWhiteSpace(r.meal_type)
                    ? "any"
                    : r.meal_type.Trim().ToLower();
            }

            await _db.SaveChangesAsync();
        }


        private sealed class ExerciseCsvModel
        {
            public string exercise_id { get; set; } = string.Empty;
            public string name_hu { get; set; } = string.Empty;
            public string name_en { get; set; } = string.Empty;
            public string primary_muscle_group { get; set; } = string.Empty;
            public string primary_muscle_subgroup { get; set; } = string.Empty;
            public string movement_type { get; set; } = string.Empty;
            public string pattern { get; set; } = string.Empty;
            public string equipment { get; set; } = string.Empty;
            public int is_compound { get; set; }
            public int difficulty_level { get; set; }
            public string push_pull_category { get; set; } = string.Empty;
            public int min_experience_level { get; set; }
            public int default_sets { get; set; }
            public int default_reps_low { get; set; }
            public int default_reps_high { get; set; }
            public int is_home_friendly { get; set; }
        }

        public async Task ImportExercisesAsync(string exercisesCsvPath)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                Encoding = System.Text.Encoding.UTF8
            };

            using var reader = new StreamReader(exercisesCsvPath);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<ExerciseCsvModel>().ToList();

            var entities = new List<Exercise>();

            foreach (var r in records)
            {
                // ha üres lenne az ID, generálunk
                Guid id;
                if (string.IsNullOrWhiteSpace(r.exercise_id))
                    id = Guid.NewGuid();
                else
                    id = Guid.Parse(r.exercise_id);

                var entity = new Exercise
                {
                    ExerciseId = id,
                    NameHu = r.name_hu,
                    NameEn = r.name_en,
                    PrimaryMuscleGroup = r.primary_muscle_group,
                    PrimaryMuscleSubgroup = r.primary_muscle_subgroup,
                    MovementType = r.movement_type,
                    Pattern = r.pattern,
                    Equipment = r.equipment,
                    IsCompound = r.is_compound == 1,
                    DifficultyLevel = r.difficulty_level,
                    PushPullCategory = r.push_pull_category,
                    MinExperienceLevel = r.min_experience_level,
                    DefaultSets = r.default_sets,
                    DefaultRepsLow = r.default_reps_low,
                    DefaultRepsHigh = r.default_reps_high,
                    IsHomeFriendly = r.is_home_friendly == 1
                };

                entities.Add(entity);
            }


            await _db.Exercises.AddRangeAsync(entities);
            await _db.SaveChangesAsync();
        }


        private int MapGender(string gender)
        {
            gender = gender.Trim().ToLower();
            return gender switch
            {
                "male" => 0,
                "female" => 1,
                _ => 0
            };
        }

        private int MapGoalType(string g)
        {
            g = g.Trim().ToLower();
            return g switch
            {
                "maintain" => 0,
                "lose" => 1,
                "gain" => 2,
                _ => 0
            };
        }

        private sealed class PlanRow
        {
            public string plan_id { get; set; }
            public double calories_kcal { get; set; }
            public double protein_g { get; set; }
            public double carbs_g { get; set; }
            public double fat_g { get; set; }
            public int meals_per_day { get; set; }
            public int snacks_per_day { get; set; }
        }

        private sealed class CaseRow
        {
            public string case_id { get; set; }
            public string gender { get; set; }
            public int age { get; set; }
            public int height_cm { get; set; }
            public double weight_kg { get; set; }
            public double bodyfat_percent { get; set; }
            public int activity_level { get; set; }
            public string goal_type { get; set; }
            public int goal_delta_kg { get; set; }
            public int goal_time_weeks { get; set; }
            public string plan_id { get; set; }
        }

        private sealed class FoodRow
        {
            public string food_id { get; set; } = "";
            public string food_name_hu { get; set; } = "";
            public string food_name_en { get; set; } = "";
            public double kcal_per_100 { get; set; }
            public double prot { get; set; }
            public double carbs { get; set; }
            public double fat { get; set; }
            public string meal_type { get; set; } = "";
        }
    }
}
