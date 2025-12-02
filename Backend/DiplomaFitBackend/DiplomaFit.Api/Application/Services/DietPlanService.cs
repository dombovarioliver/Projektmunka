using DiplomaFit.Api.Application.Dtos;
using DiplomaFit.Api.Application.Dtos.Diet;
using DiplomaFit.Api.Application.Interfaces;
using DiplomaFit.Api.Domain.Entities;
using DiplomaFit.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DiplomaFit.Api.Application.Services
{
    public class DietPlanService : IDietPlanService
    {
        private readonly AppDbContext _db;
        private readonly Random _random = new();

        public DietPlanService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<WeeklyMealPlanDto> GenerateWeeklyPlanAsync(Guid caseId)
        {
            var caseEntity = await _db.Cases
                .Include(c => c.Plan)
                .FirstOrDefaultAsync(c => c.CaseId == caseId);

            if (caseEntity == null)
                throw new KeyNotFoundException($"Case not found: {caseId}");

            var plan = caseEntity.Plan;

            var foods = await _db.Foods.ToListAsync();
            if (!foods.Any())
                throw new InvalidOperationException("No foods available in database.");

            var dailyCalories = plan.Calories;
            var dailyProtein = plan.Protein;
            var dailyCarbs = plan.Carbs;
            var dailyFat = plan.Fat;

            var result = new WeeklyMealPlanDto
            {
                CaseId = caseId,
                DailyCalories = dailyCalories,
                DailyProtein = dailyProtein,
                DailyCarbs = dailyCarbs,
                DailyFat = dailyFat
            };

            // 7 napra generálunk
            for (int day = 1; day <= 7; day++)
            {
                var dayPlan = new DayPlanDto
                {
                    DayIndex = day,
                    Name = $"Nap {day}"
                };

                // étkezésenkénti arányok
                var meals = GenerateDailyMealsTemplate(dailyCalories, dailyProtein, dailyCarbs, dailyFat);

                foreach (var meal in meals)
                {
                    var mealFoods = PickFoodsForMeal(foods, meal.MealCategory, 3);
                    DistributeMealTargets(meal, mealFoods);
                    dayPlan.Meals.Add(meal);
                }

                result.Days.Add(dayPlan);
            }

            return result;
        }

        // 4 étkezés + 2 snack sablon
        private List<MealDto> GenerateDailyMealsTemplate(
            double dailyCalories,
            double dailyProtein,
            double dailyCarbs,
            double dailyFat)
        {
            var meals = new List<MealDto>();

            // reggeli 25%, ebéd 35%, vacsora 25%, 2 snack 7.5-7.5%
            meals.Add(CreateMeal("Breakfast", "breakfast", 0.25, dailyCalories, dailyProtein, dailyCarbs, dailyFat));
            meals.Add(CreateMeal("Lunch", "main", 0.35, dailyCalories, dailyProtein, dailyCarbs, dailyFat));
            meals.Add(CreateMeal("Dinner", "main", 0.25, dailyCalories, dailyProtein, dailyCarbs, dailyFat));
            meals.Add(CreateMeal("Snack 1", "snack", 0.075, dailyCalories, dailyProtein, dailyCarbs, dailyFat));
            meals.Add(CreateMeal("Snack 2", "snack", 0.075, dailyCalories, dailyProtein, dailyCarbs, dailyFat));

            return meals;
        }

        private MealDto CreateMeal(
            string mealType,
            string mealCategory,
            double ratio,
            double dailyCalories,
            double dailyProtein,
            double dailyCarbs,
            double dailyFat)
        {
            return new MealDto
            {
                MealType = mealType,
                MealCategory = mealCategory,
                TargetCalories = dailyCalories * ratio,
                TargetProtein = dailyProtein * ratio,
                TargetCarbs = dailyCarbs * ratio,
                TargetFat = dailyFat * ratio
            };
        }

        private List<Food> PickFoodsForMeal(List<Food> allFoods, string mealCategory, int count)
        {
            // először próbáljunk a kategórián belül
            var candidates = allFoods
                .Where(f =>
                    f.MealType == mealCategory ||
                    f.MealType == "any")
                .ToList();

            if (!candidates.Any())
            {
                // ha nagyon nincs, válasszunk bármiből (fallback)
                candidates = allFoods.ToList();
            }

            count = Math.Min(count, candidates.Count);

            var selected = new List<Food>();
            var usedIndexes = new HashSet<int>();

            while (selected.Count < count)
            {
                var idx = _random.Next(candidates.Count);
                if (usedIndexes.Add(idx))
                {
                    selected.Add(candidates[idx]);
                }
            }

            return selected;
        }

        private void DistributeMealTargets(MealDto meal, List<Food> foods)
        {
            meal.Items.Clear();

            if (!foods.Any() || meal.TargetCalories <= 0)
                return;

            // kalóriacél elosztása egyenlően az ételek között
            var caloriesPerFood = meal.TargetCalories / foods.Count;

            foreach (var food in foods)
            {
                // kcal/100g -> kcal/1g
                var kcalPerGram = food.KcalPer100 / 100.0;
                if (kcalPerGram <= 0)
                    continue;

                var grams = caloriesPerFood / kcalPerGram;

                var factor = grams / 100.0;

                var item = new MealItemDto
                {
                    FoodId = food.FoodId,
                    FoodName = string.IsNullOrWhiteSpace(food.FoodNameHu)
                        ? food.FoodNameEn
                        : food.FoodNameHu,
                    QuantityGrams = Math.Round(grams, 1),
                    Calories = Math.Round(food.KcalPer100 * factor, 1),
                    Protein = Math.Round(food.ProteinGPer100 * factor, 1),
                    Carbs = Math.Round(food.CarbsGPer100 * factor, 1),
                    Fat = Math.Round(food.FatGPer100 * factor, 1)
                };

                meal.Items.Add(item);
            }
        }
    }
}
