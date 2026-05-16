using DiplomaFit.Data;
using DiplomaFit.Model.Dto.Diet;
using DiplomaFit.Model.Dto.ML;
using DiplomaFit.Model.Dto.User;
using DiplomaFit.Model.Entities;
using DiplomaFit.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiplomaFit.Service.DietService
{
    public class DietPlanService
    {
        private readonly FoodRepository _foodRepository;
        private readonly DietMlClientService _dietMlClientService;
        private readonly Random _random = new();

        public DietPlanService(
            FoodRepository foodRepository,
            DietMlClientService dietMlClientService)
        {
            _foodRepository = foodRepository;
            _dietMlClientService = dietMlClientService;
        }

        public async Task<WeeklyMealPlanDto> GenerateWeeklyPlanAsync(
            UserResponseDto user,
            CancellationToken cancellationToken = default)
        {
            var mlRequest = new DietPredictionRequestDto
            {
                Gender = (int)user.Gender,
                Age = user.Age,
                HeightCm = user.HeightCm,
                WeightKg = user.WeightKg,
                BodyfatPercent = user.BodyfatPercent ?? 0,
                ActivityLevel = (int)user.ActivityLevel,
                GoalType = (int)user.GoalType,
                GoalDeltaKg = user.GoalDeltaKg,
                GoalTimeWeeks = user.GoalTimeWeeks
            };

            var prediction = await _dietMlClientService.PredictAsync(mlRequest, cancellationToken);

            var foods = _foodRepository.GetAll().ToList();

            if (!foods.Any())
                throw new InvalidOperationException("Nincs étel az adatbázisban.");

            var result = new WeeklyMealPlanDto
            {
                UserId = user.Id,
                DailyCalories = Math.Round(prediction.CaloriesKcal, 1),
                DailyProtein = Math.Round(prediction.ProteinG, 1),
                DailyCarbs = Math.Round(prediction.CarbsG, 1),
                DailyFat = Math.Round(prediction.FatG, 1),
                MealsPerDay = 3,
                SnacksPerDay = 2
            };

            for (int day = 1; day <= 7; day++)
            {
                var dayPlan = new DayPlanDto
                {
                    DayIndex = day,
                    Name = $"Nap {day}"
                };

                var meals = GenerateDailyMealsTemplate(
                    result.DailyCalories,
                    result.DailyProtein,
                    result.DailyCarbs,
                    result.DailyFat);

                foreach (var meal in meals)
                {
                    BuildRealisticMeal(meal, foods);
                    dayPlan.Meals.Add(meal);
                }

                result.Days.Add(dayPlan);
            }

            return result;
        }

        private List<MealDto> GenerateDailyMealsTemplate(
            double dailyCalories,
            double dailyProtein,
            double dailyCarbs,
            double dailyFat)
        {
            return new List<MealDto>
            {
                CreateMeal(MealType.Breakfast, MealType.Breakfast, 0.25, dailyCalories, dailyProtein, dailyCarbs, dailyFat),
                CreateMeal(MealType.Main, MealType.Main, 0.35, dailyCalories, dailyProtein, dailyCarbs, dailyFat),
                CreateMeal(MealType.Dinner, MealType.Dinner, 0.25, dailyCalories, dailyProtein, dailyCarbs, dailyFat),
                CreateMeal(MealType.Snack, MealType.Snack, 0.075, dailyCalories, dailyProtein, dailyCarbs, dailyFat),
                CreateMeal(MealType.Snack, MealType.Snack, 0.075, dailyCalories, dailyProtein, dailyCarbs, dailyFat)
            };
        }

        private MealDto CreateMeal(
            MealType mealType,
            MealType mealCategory,
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
                TargetCalories = Math.Round(dailyCalories * ratio, 1),
                TargetProtein = Math.Round(dailyProtein * ratio, 1),
                TargetCarbs = Math.Round(dailyCarbs * ratio, 1),
                TargetFat = Math.Round(dailyFat * ratio, 1)
            };
        }

        private void BuildRealisticMeal(MealDto meal, List<Food> allFoods)
        {
            meal.Items.Clear();

            if (meal.TargetCalories <= 0)
                return;

            if (meal.MealType == MealType.Breakfast)
            {
                AddFoodByCategory(meal, allFoods, FoodCategory.BreakfastBase, 0.45);
                AddFoodByAnyCategory(meal, allFoods, new[] { FoodCategory.ProteinSource, FoodCategory.Dairy }, 0.35);
                AddFoodByCategory(meal, allFoods, FoodCategory.Fruit, 0.20);
            }
            else if (meal.MealType == MealType.Snack)
            {
                AddFoodByAnyCategory(meal, allFoods, new[]
                {
                    FoodCategory.ProteinSupplement,
                    FoodCategory.Dairy,
                    FoodCategory.Fruit,
                    FoodCategory.NutsAndSeeds,
                    FoodCategory.Snack
                }, 1.00);
            }
            else
            {
                var useCompleteMeal = _random.NextDouble() < 0.35 && HasCandidate(allFoods, meal, FoodCategory.CompleteMeal);

                if (useCompleteMeal)
                {
                    AddFoodByCategory(meal, allFoods, FoodCategory.CompleteMeal, 0.82);
                    AddFoodByCategory(meal, allFoods, FoodCategory.VegetableSide, 0.18);
                }
                else
                {
                    AddFoodByCategory(meal, allFoods, FoodCategory.ProteinSource, 0.45);
                    AddFoodByCategory(meal, allFoods, FoodCategory.CarbSide, 0.35);
                    AddFoodByCategory(meal, allFoods, FoodCategory.VegetableSide, 0.20);
                }
            }

            AddSmallFillerIfNeeded(meal, allFoods);
        }

        private bool HasCandidate(List<Food> foods, MealDto meal, FoodCategory category)
        {
            return foods.Any(food => IsMealCompatible(food, meal.MealType, meal.MealCategory) && food.FoodCategory == category);
        }

        private void AddFoodByCategory(
            MealDto meal,
            List<Food> allFoods,
            FoodCategory category,
            double calorieRatio)
        {
            var food = PickFood(allFoods, meal, new[] { category });

            if (food == null)
                return;

            AddFoodItem(meal, food, meal.TargetCalories * calorieRatio);
        }

        private void AddFoodByAnyCategory(
            MealDto meal,
            List<Food> allFoods,
            IReadOnlyCollection<FoodCategory> categories,
            double calorieRatio)
        {
            var food = PickFood(allFoods, meal, categories);

            if (food == null)
                return;

            AddFoodItem(meal, food, meal.TargetCalories * calorieRatio);
        }

        private Food? PickFood(
            List<Food> allFoods,
            MealDto meal,
            IReadOnlyCollection<FoodCategory> categories)
        {
            var candidates = allFoods
                .Where(food =>
                    IsMealCompatible(food, meal.MealType, meal.MealCategory) &&
                    categories.Contains(food.FoodCategory))
                .OrderBy(_ => _random.Next())
                .ToList();

            if (candidates.Any())
                return candidates.First();

            return allFoods
                .Where(food => categories.Contains(food.FoodCategory))
                .OrderBy(_ => _random.Next())
                .FirstOrDefault();
        }

        private bool IsMealCompatible(Food food, MealType mealType, MealType mealCategory)
        {
            if (food.MealType == MealType.Any)
                return true;

            if (food.MealType == mealCategory || food.MealType == mealType)
                return true;

            // A főétel típusú ételeket vacsorára is engedjük, mert az adatbázisban a legtöbb hús/köret MealType.Main.
            if (mealType == MealType.Dinner && food.MealType == MealType.Main)
                return true;

            return false;
        }

        private void AddFoodItem(MealDto meal, Food food, double targetCaloriesForFood)
        {
            if (food.KcalPer100 <= 0 || targetCaloriesForFood <= 0)
                return;

            var rawGrams = targetCaloriesForFood / (food.KcalPer100 / 100.0);
            var grams = ClampAndRoundPortion(rawGrams, food.MinPortionGrams, food.MaxPortionGrams);
            var factor = grams / 100.0;

            meal.Items.Add(new MealItemDto
            {
                FoodId = food.FoodId,
                FoodName = string.IsNullOrWhiteSpace(food.FoodNameHu)
                    ? food.FoodNameEn
                    : food.FoodNameHu,

                QuantityGrams = grams,
                Calories = Math.Round(food.KcalPer100 * factor, 1),
                Protein = Math.Round(food.ProteinGPer100 * factor, 1),
                Carbs = Math.Round(food.CarbsGPer100 * factor, 1),
                Fat = Math.Round(food.FatGPer100 * factor, 1)
            });
        }

        private double ClampAndRoundPortion(double grams, double minGrams, double maxGrams)
        {
            var safeMin = minGrams <= 0 ? 50 : minGrams;
            var safeMax = maxGrams <= 0 ? 250 : maxGrams;

            if (safeMax < safeMin)
                safeMax = safeMin;

            var clamped = Math.Clamp(grams, safeMin, safeMax);

            // 5 grammos lépcső, hogy emberibb adagok legyenek: 95g, 120g, 175g stb.
            return Math.Round(clamped / 5.0) * 5.0;
        }

        private void AddSmallFillerIfNeeded(MealDto meal, List<Food> allFoods)
        {
            var currentCalories = meal.Items.Sum(item => item.Calories);
            var missingCalories = meal.TargetCalories - currentCalories;

            if (missingCalories < 120)
                return;

            if (meal.MealType == MealType.Snack)
            {
                AddFoodByAnyCategory(meal, allFoods, new[] { FoodCategory.NutsAndSeeds, FoodCategory.Fruit }, 0.45);
                return;
            }

            if (meal.MealType == MealType.Breakfast)
            {
                AddFoodByAnyCategory(meal, allFoods, new[] { FoodCategory.Dairy, FoodCategory.Fruit }, 0.25);
                return;
            }

            AddFoodByAnyCategory(meal, allFoods, new[] { FoodCategory.CarbSide, FoodCategory.ProteinSource }, 0.20);
        }
    }
}