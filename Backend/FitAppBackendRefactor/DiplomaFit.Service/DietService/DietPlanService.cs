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
                MealsPerDay = (int)Math.Round(prediction.MealsPerDay),
                SnacksPerDay = (int)Math.Round(prediction.SnacksPerDay)
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
                    var selectedFoods = PickFoodsForMeal(foods, meal.MealCategory, 3);

                    DistributeMealTargets(meal, selectedFoods);

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
                CreateMeal(MealType.Main, MealType.Main, 0.25, dailyCalories, dailyProtein, dailyCarbs, dailyFat),
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

        private List<Food> PickFoodsForMeal(
            List<Food> allFoods,
            MealType mealCategory,
            int count)
        {
            var candidates = allFoods
                .Where(food =>
                    food.MealType == mealCategory ||
                    food.MealType == MealType.Any)
                .ToList();

            if (!candidates.Any())
                candidates = allFoods.ToList();

            return candidates
                .OrderBy(_ => _random.Next())
                .Take(Math.Min(count, candidates.Count))
                .ToList();
        }

        private void DistributeMealTargets(MealDto meal, List<Food> foods)
        {
            meal.Items.Clear();

            if (!foods.Any() || meal.TargetCalories <= 0)
                return;

            var caloriesPerFood = meal.TargetCalories / foods.Count;

            foreach (var food in foods)
            {
                var kcalPerGram = food.KcalPer100 / 100.0;

                if (kcalPerGram <= 0)
                    continue;

                var grams = caloriesPerFood / kcalPerGram;
                var factor = grams / 100.0;

                meal.Items.Add(new MealItemDto
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
                });
            }
        }
    }
}