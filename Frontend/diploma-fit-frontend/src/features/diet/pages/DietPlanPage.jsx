import { useEffect, useMemo, useState } from "react";

import {
  generateDietPlan,
  getFoods,
} from "../services/dietPlanService";

import "../styles/dietPlan.css";

const DIET_PLAN_STORAGE_KEY = "neurafit_diet_plan";
const EATEN_FOODS_STORAGE_KEY = "neurafit_eaten_foods";
const CUSTOM_FOODS_STORAGE_KEY = "neurafit_custom_foods";

const MEAL_LABELS = {
  1: "Reggeli",
  2: "Ebéd",
  3: "Vacsora",
  4: "Snack",
};

const MEAL_SELECT_OPTIONS = [
  { value: 1, label: "Reggeli" },
  { value: 2, label: "Ebéd" },
  { value: 3, label: "Vacsora" },
  { value: 4, label: "Snack" },
];

const DEFAULT_CUSTOM_FORM = {
  foodNameHu: "",
  brand: "",
  servingGrams: 100,
  kcalPer100: 0,
  proteinGPer100: 0,
  carbsGPer100: 0,
  fatGPer100: 0,
};

function readStorage(key, fallback) {
  try {
    const value = localStorage.getItem(key);
    return value ? JSON.parse(value) : fallback;
  } catch {
    return fallback;
  }
}

function numberOrZero(value) {
  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : 0;
}

function round(value, digits = 0) {
  const multiplier = 10 ** digits;
  return Math.round(numberOrZero(value) * multiplier) / multiplier;
}

function getFoodName(food) {
  return food.foodNameHu || food.foodNameEn || food.foodName || "Névtelen étel";
}

function getFoodBrand(food) {
  return food.brand || food.foodNameEn || "Saját / adatbázis";
}

function getMealType(meal) {
  return Number(meal.mealType ?? meal.mealCategory ?? 0);
}

function calcByGrams(food, grams) {
  const ratio = numberOrZero(grams) / 100;

  return {
    calories: round(numberOrZero(food.kcalPer100) * ratio),
    protein: round(numberOrZero(food.proteinGPer100) * ratio, 1),
    carbs: round(numberOrZero(food.carbsGPer100) * ratio, 1),
    fat: round(numberOrZero(food.fatGPer100) * ratio, 1),
  };
}

function planItemToFood(item) {
  const grams = numberOrZero(item.quantityGrams || 100);
  const multiplier = grams > 0 ? 100 / grams : 1;

  return {
    foodId: item.foodId,
    foodNameHu: item.foodName,
    foodNameEn: item.foodName,
    kcalPer100: numberOrZero(item.calories) * multiplier,
    proteinGPer100: numberOrZero(item.protein) * multiplier,
    carbsGPer100: numberOrZero(item.carbs) * multiplier,
    fatGPer100: numberOrZero(item.fat) * multiplier,
  };
}

function makeFoodItem(food, mealType, grams = 100) {
  const macros = calcByGrams(food, grams);

  return {
    localId: `${food.foodId || "custom"}-${Date.now()}-${Math.random()
      .toString(16)
      .slice(2)}`,
    foodId: food.foodId || food.localId || "",
    foodName: getFoodName(food),
    mealType,
    quantityGrams: numberOrZero(grams),
    calories: macros.calories,
    protein: macros.protein,
    carbs: macros.carbs,
    fat: macros.fat,
    kcalPer100: numberOrZero(food.kcalPer100),
    proteinGPer100: numberOrZero(food.proteinGPer100),
    carbsGPer100: numberOrZero(food.carbsGPer100),
    fatGPer100: numberOrZero(food.fatGPer100),
  };
}

function getItemKey(dayIndex, mealIndex, item) {
  return `${dayIndex}-${mealIndex}-${item.localId || item.foodId || item.foodName}`;
}

function addLocalIdsToPlan(plan) {
  if (!plan?.days) {
    return plan;
  }

  return {
    ...plan,
    days: plan.days.map((day) => ({
      ...day,
      meals: (day.meals || []).map((meal) => ({
        ...meal,
        items: (meal.items || []).map((item) => ({
          ...item,
          localId:
            item.localId ||
            `${item.foodId || item.foodName}-${Date.now()}-${Math.random()
              .toString(16)
              .slice(2)}`,
          kcalPer100:
            item.kcalPer100 ?? planItemToFood(item).kcalPer100,
          proteinGPer100:
            item.proteinGPer100 ?? planItemToFood(item).proteinGPer100,
          carbsGPer100:
            item.carbsGPer100 ?? planItemToFood(item).carbsGPer100,
          fatGPer100:
            item.fatGPer100 ?? planItemToFood(item).fatGPer100,
        })),
      })),
    })),
  };
}

function getEmptyPlan(userId) {
  return {
    userId,
    dailyCalories: 0,
    dailyProtein: 0,
    dailyCarbs: 0,
    dailyFat: 0,
    days: [
      {
        dayIndex: 1,
        name: "1. nap",
        meals: MEAL_SELECT_OPTIONS.map((meal) => ({
          mealType: meal.value,
          mealCategory: meal.value,
          targetCalories: 0,
          targetProtein: 0,
          targetCarbs: 0,
          targetFat: 0,
          items: [],
        })),
      },
    ],
  };
}

export default function DietPlanPage() {
  const [dietPlan, setDietPlan] = useState(() =>
    addLocalIdsToPlan(readStorage(DIET_PLAN_STORAGE_KEY, null))
  );
  const [selectedDayIndex, setSelectedDayIndex] = useState(0);
  const [checkedFoods, setCheckedFoods] = useState(() =>
    readStorage(EATEN_FOODS_STORAGE_KEY, {})
  );
  const [expandedItems, setExpandedItems] = useState({});
  const [foodsFromSql, setFoodsFromSql] = useState([]);
  const [customFoods, setCustomFoods] = useState(() =>
    readStorage(CUSTOM_FOODS_STORAGE_KEY, [])
  );
  const [isGenerating, setIsGenerating] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const [foodModal, setFoodModal] = useState({
    isOpen: false,
    dayIndex: 0,
    mealType: 1,
    activeTab: "all",
  });
  const [searchText, setSearchText] = useState("");
  const [selectedFood, setSelectedFood] = useState(null);
  const [servingGrams, setServingGrams] = useState(100);
  const [servingCount, setServingCount] = useState(1);
  const [customForm, setCustomForm] = useState(DEFAULT_CUSTOM_FORM);

  useEffect(() => {
    async function loadFoods() {
      try {
        const foods = await getFoods();
        setFoodsFromSql(Array.isArray(foods) ? foods : []);
      } catch (err) {
        console.error("Ételek betöltési hiba:", err);
      }
    }

    loadFoods();
  }, []);

  const currentPlan = dietPlan || getEmptyPlan(localStorage.getItem("userId"));
  const selectedDay = currentPlan.days?.[selectedDayIndex];

  const filteredSqlFoods = useMemo(() => {
    const normalized = searchText.trim().toLowerCase();

    if (!normalized) {
      return foodsFromSql.slice(0, 20);
    }

    return foodsFromSql
      .filter((food) => {
        const hu = (food.foodNameHu || "").toLowerCase();
        const en = (food.foodNameEn || "").toLowerCase();
        return hu.includes(normalized) || en.includes(normalized);
      })
      .slice(0, 30);
  }, [foodsFromSql, searchText]);

  const filteredCustomFoods = useMemo(() => {
    const normalized = searchText.trim().toLowerCase();

    if (!normalized) {
      return customFoods;
    }

    return customFoods.filter((food) =>
      getFoodName(food).toLowerCase().includes(normalized)
    );
  }, [customFoods, searchText]);

  const eatenSummary = useMemo(() => {
  const summary = {
    calories: 0,
    protein: 0,
    carbs: 0,
    fat: 0,
  };

  const activeDay = currentPlan.days?.[selectedDayIndex];

  activeDay?.meals?.forEach((meal, mealIndex) => {
    meal.items?.forEach((item) => {
      const key = getItemKey(selectedDayIndex, mealIndex, item);

      if (checkedFoods[key]) {
        summary.calories += numberOrZero(item.calories);
        summary.protein += numberOrZero(item.protein);
        summary.carbs += numberOrZero(item.carbs);
        summary.fat += numberOrZero(item.fat);
      }
    });
  });

  return {
    calories: round(summary.calories),
    protein: round(summary.protein, 1),
    carbs: round(summary.carbs, 1),
    fat: round(summary.fat, 1),
  };
}, [currentPlan, checkedFoods, selectedDayIndex]);

  const progressPercent = currentPlan.dailyCalories
    ? Math.min(100, round((eatenSummary.calories / currentPlan.dailyCalories) * 100))
    : 0;

  function savePlan(updatedPlan) {
    setDietPlan(updatedPlan);
    localStorage.setItem(DIET_PLAN_STORAGE_KEY, JSON.stringify(updatedPlan));
  }

  function saveChecked(updatedCheckedFoods) {
    setCheckedFoods(updatedCheckedFoods);
    localStorage.setItem(
      EATEN_FOODS_STORAGE_KEY,
      JSON.stringify(updatedCheckedFoods)
    );
  }

  function removeFoodFromMeal(dayArrayIndex, mealIndex, itemIndex, item) {
  const itemKey = getItemKey(dayArrayIndex, mealIndex, item);

  const updatedPlan = {
    ...currentPlan,
    days: currentPlan.days.map((day, dIndex) => {
      if (dIndex !== dayArrayIndex) {
        return day;
      }

      return {
        ...day,
        meals: day.meals.map((meal, mIndex) => {
          if (mIndex !== mealIndex) {
            return meal;
          }

          return {
            ...meal,
            items: (meal.items || []).filter((_, iIndex) => iIndex !== itemIndex),
          };
        }),
      };
    }),
  };

  const updatedCheckedFoods = { ...checkedFoods };
  delete updatedCheckedFoods[itemKey];

  const updatedExpandedItems = { ...expandedItems };
  delete updatedExpandedItems[itemKey];

  savePlan(updatedPlan);
  saveChecked(updatedCheckedFoods);
  setExpandedItems(updatedExpandedItems);
}

  function openFoodModal(dayIndex = selectedDayIndex, mealType = 1) {
    setFoodModal({
      isOpen: true,
      dayIndex,
      mealType,
      activeTab: "all",
    });
    setSearchText("");
    setSelectedFood(null);
    setServingGrams(100);
    setServingCount(1);
  }

  function closeFoodModal() {
    setFoodModal((prev) => ({
      ...prev,
      isOpen: false,
    }));
    setSelectedFood(null);
  }

  async function handleGenerateDietPlan() {
    const userId = localStorage.getItem("userId");

    if (!userId) {
      setErrorMessage("Nem található userId a localStorage-ben. Jelentkezz be újra.");
      return;
    }

    try {
      setIsGenerating(true);
      setErrorMessage("");

      const generatedPlan = addLocalIdsToPlan(await generateDietPlan(userId));

      localStorage.setItem(DIET_PLAN_STORAGE_KEY, JSON.stringify(generatedPlan));
      localStorage.removeItem(EATEN_FOODS_STORAGE_KEY);

      setDietPlan(generatedPlan);
      setCheckedFoods({});
      setExpandedItems({});
      setSelectedDayIndex(0);
    } catch (err) {
      console.error("Étrend generálási hiba:", err);
      setErrorMessage("Nem sikerült új étrendet generálni. Nézd meg, fut-e a backend és van-e userId.");
    } finally {
      setIsGenerating(false);
    }
  }

  function toggleChecked(dayArrayIndex, mealIndex, item) {
    const key = getItemKey(dayArrayIndex, mealIndex, item);

    saveChecked({
      ...checkedFoods,
      [key]: !checkedFoods[key],
    });
  }

  function toggleExpanded(dayArrayIndex, mealIndex, item) {
    const key = getItemKey(dayArrayIndex, mealIndex, item);

    setExpandedItems((prev) => ({
      ...prev,
      [key]: !prev[key],
    }));
  }

  function updateFoodWeight(dayArrayIndex, mealIndex, itemIndex, grams) {
    const updatedGrams = Math.max(0, numberOrZero(grams));

    const updatedPlan = {
      ...currentPlan,
      days: currentPlan.days.map((day, dIndex) => {
        if (dIndex !== dayArrayIndex) {
          return day;
        }

        return {
          ...day,
          meals: day.meals.map((meal, mIndex) => {
            if (mIndex !== mealIndex) {
              return meal;
            }

            return {
              ...meal,
              items: meal.items.map((item, iIndex) => {
                if (iIndex !== itemIndex) {
                  return item;
                }

                const foodLike = {
                  kcalPer100: item.kcalPer100,
                  proteinGPer100: item.proteinGPer100,
                  carbsGPer100: item.carbsGPer100,
                  fatGPer100: item.fatGPer100,
                };
                const macros = calcByGrams(foodLike, updatedGrams);

                return {
                  ...item,
                  quantityGrams: updatedGrams,
                  calories: macros.calories,
                  protein: macros.protein,
                  carbs: macros.carbs,
                  fat: macros.fat,
                };
              }),
            };
          }),
        };
      }),
    };

    savePlan(updatedPlan);
  }

  function addFoodToMeal(food, mealType, grams) {
    const updatedPlan = {
      ...currentPlan,
      days: currentPlan.days.map((day, dIndex) => {
        if (dIndex !== foodModal.dayIndex) {
          return day;
        }

        let hasMeal = false;

        const meals = day.meals.map((meal) => {
          if (getMealType(meal) !== Number(mealType)) {
            return meal;
          }

          hasMeal = true;

          return {
            ...meal,
            items: [
              ...(meal.items || []),
              makeFoodItem(food, Number(mealType), grams),
            ],
          };
        });

        if (!hasMeal) {
          meals.push({
            mealType: Number(mealType),
            mealCategory: Number(mealType),
            targetCalories: 0,
            targetProtein: 0,
            targetCarbs: 0,
            targetFat: 0,
            items: [makeFoodItem(food, Number(mealType), grams)],
          });
        }

        return {
          ...day,
          meals,
        };
      }),
    };

    savePlan(updatedPlan);
    closeFoodModal();
  }

  function handleSaveCustomFood(e) {
    e.preventDefault();

    if (!customForm.foodNameHu.trim()) {
      return;
    }

    const customFood = {
      ...customForm,
      localId: `local-${Date.now()}`,
      foodId: `local-${Date.now()}`,
      foodNameEn: customForm.foodNameHu,
      mealType: foodModal.mealType,
      servingGrams: numberOrZero(customForm.servingGrams) || 100,
      kcalPer100: numberOrZero(customForm.kcalPer100),
      proteinGPer100: numberOrZero(customForm.proteinGPer100),
      carbsGPer100: numberOrZero(customForm.carbsGPer100),
      fatGPer100: numberOrZero(customForm.fatGPer100),
    };

    const updatedCustomFoods = [customFood, ...customFoods];

    setCustomFoods(updatedCustomFoods);
    localStorage.setItem(
      CUSTOM_FOODS_STORAGE_KEY,
      JSON.stringify(updatedCustomFoods)
    );

    setCustomForm(DEFAULT_CUSTOM_FORM);
    setSelectedFood(customFood);
    setServingGrams(customFood.servingGrams || 100);
    setServingCount(1);
    setFoodModal((prev) => ({
      ...prev,
      activeTab: "myFoods",
    }));
  }

  function selectFood(food) {
    setSelectedFood(food);
    setServingGrams(numberOrZero(food.servingGrams) || 100);
    setServingCount(1);
  }

  function renderFoodList(foods) {
    if (!foods.length) {
      return (
        <div className="diet-food-empty">
          Nincs találat.
        </div>
      );
    }

    return foods.map((food) => (
      <button
        type="button"
        className="diet-food-search-row"
        key={food.foodId || food.localId || getFoodName(food)}
        onClick={() => selectFood(food)}
      >
        <div>
          <strong>{getFoodName(food)}</strong>
          <span>
            {round(food.kcalPer100)} kcal / 100 g · {getFoodBrand(food)}
          </span>
        </div>

        <span className="diet-food-plus">+</span>
      </button>
    ));
  }

  function renderSelectedFoodDetails() {
    if (!selectedFood) {
      return null;
    }

    const totalGrams = numberOrZero(servingGrams) * numberOrZero(servingCount);
    const macros = calcByGrams(selectedFood, totalGrams);
    const totalMacro = macros.protein + macros.carbs + macros.fat;
    const carbPercent = totalMacro ? round((macros.carbs / totalMacro) * 100) : 0;
    const fatPercent = totalMacro ? round((macros.fat / totalMacro) * 100) : 0;
    const proteinPercent = totalMacro ? round((macros.protein / totalMacro) * 100) : 0;

    return (
      <div className="diet-food-detail-card">
        <div className="diet-food-detail-head">
          <div>
            <h3>{getFoodName(selectedFood)}</h3>
            <p>{getFoodBrand(selectedFood)}</p>
          </div>

          <button
            type="button"
            className="diet-close-detail"
            onClick={() => setSelectedFood(null)}
          >
            ×
          </button>
        </div>

        <div className="diet-detail-grid">
          <label>Adag mérete</label>
          <input
            type="number"
            min="1"
            value={servingGrams}
            onChange={(e) => setServingGrams(e.target.value)}
          />

          <label>Adagok száma</label>
          <input
            type="number"
            min="0.1"
            step="0.1"
            value={servingCount}
            onChange={(e) => setServingCount(e.target.value)}
          />

          <label>Étkezés</label>
          <select
            value={foodModal.mealType}
            onChange={(e) =>
              setFoodModal((prev) => ({
                ...prev,
                mealType: Number(e.target.value),
              }))
            }
          >
            {MEAL_SELECT_OPTIONS.map((meal) => (
              <option value={meal.value} key={meal.value}>
                {meal.label}
              </option>
            ))}
          </select>
        </div>

        <div className="diet-nutrition-row">
          <div className="diet-kcal-ring small">
            <div>
              <strong>{macros.calories}</strong>
              <span>kcal</span>
            </div>
          </div>

          <div className="diet-macro-box">
            <span>{carbPercent}%</span>
            <strong>{macros.carbs} g</strong>
            <p>Szénhidrát</p>
          </div>

          <div className="diet-macro-box">
            <span>{fatPercent}%</span>
            <strong>{macros.fat} g</strong>
            <p>Zsír</p>
          </div>

          <div className="diet-macro-box">
            <span>{proteinPercent}%</span>
            <strong>{macros.protein} g</strong>
            <p>Fehérje</p>
          </div>
        </div>

        <button
          type="button"
          className="diet-primary-btn full"
          onClick={() =>
            addFoodToMeal(
              selectedFood,
              foodModal.mealType,
              totalGrams
            )
          }
        >
          Hozzáadás az étrendhez
        </button>
      </div>
    );
  }

  return (
    <div className="diet-page">
      <div className="diet-topbar">
        <button
          type="button"
          className="diet-primary-btn"
          onClick={handleGenerateDietPlan}
          disabled={isGenerating}
        >
          {isGenerating ? "Étrend készítése..." : "Új étrend készítése"}
        </button>

        <button
          type="button"
          className="diet-secondary-btn"
          onClick={() => openFoodModal(selectedDayIndex, 1)}
        >
          Új étel hozzáadása
        </button>
      </div>

      <section className="diet-progress-section">
        <div className="diet-kcal-ring">
          <div>
            <strong>{eatenSummary.calories}</strong>
            <span>kcal</span>
          </div>
        </div>

        <div className="diet-progress-info">
          <h2>Mai bevitel</h2>
          <p>
            Bepipált ételek alapján · {progressPercent}% a napi célból
          </p>

          <div className="diet-progress-bar">
            <span style={{ width: `${progressPercent}%` }} />
          </div>
        </div>

        <div className="diet-progress-macros">
          <div>
            <strong>{eatenSummary.protein} g</strong>
            <span>Fehérje</span>
          </div>
          <div>
            <strong>{eatenSummary.carbs} g</strong>
            <span>Szénhidrát</span>
          </div>
          <div>
            <strong>{eatenSummary.fat} g</strong>
            <span>Zsír</span>
          </div>
        </div>
      </section>

      {errorMessage && <div className="diet-error">{errorMessage}</div>}

      {!dietPlan && (
        <div className="diet-empty-state">
          Még nincs generált étrended. Kattints az új étrend készítésére.
        </div>
      )}

      {currentPlan.days?.length > 0 && (
        <>
          <div className="diet-day-tabs">
            {currentPlan.days.map((day, index) => (
              <button
                type="button"
                key={day.dayIndex || index}
                className={
                  selectedDayIndex === index
                    ? "diet-day-tab active"
                    : "diet-day-tab"
                }
                onClick={() => setSelectedDayIndex(index)}
              >
                {day.dayIndex || index + 1}. nap
              </button>
            ))}
          </div>

          <section className="diet-plan-card">
            <div className="diet-plan-title-row">
              <div>
                <span>Étrend</span>
                <h1>{selectedDay?.name || `${selectedDayIndex + 1}. nap`}</h1>
              </div>

              <p>
                Napi cél: {round(currentPlan.dailyCalories)} kcal · {round(currentPlan.dailyProtein, 1)} g fehérje
              </p>
            </div>

            <div className="diet-meal-list">
              {(selectedDay?.meals || []).map((meal, mealIndex) => {
                const mealType = getMealType(meal);
                const mealLabel = MEAL_LABELS[mealType] || "Étkezés";

                return (
                  <div className="diet-meal-block" key={`${mealType}-${mealIndex}`}>
                    <div className="diet-meal-title">
                      <h2>{mealLabel}</h2>

                      <button
                        type="button"
                        className="diet-add-meal-btn"
                        onClick={() => openFoodModal(selectedDayIndex, mealType)}
                      >
                        +
                      </button>
                    </div>

                    <div className="diet-table-head">
                      <span>Étel neve</span>
                      <span>Súly</span>
                      <span>kcal</span>
                      <span></span>
                    </div>

                    {meal.items?.length ? (
                      meal.items.map((item, itemIndex) => {
                        const itemKey = getItemKey(selectedDayIndex, mealIndex, item);
                        const isChecked = Boolean(checkedFoods[itemKey]);
                        const isExpanded = Boolean(expandedItems[itemKey]);

                        return (
                          <div
                            className={
                              isChecked
                                ? "diet-food-row completed"
                                : "diet-food-row"
                            }
                            key={itemKey}
                          >
                            <div className="diet-food-main-row">
                              <label className="diet-check-wrap">
                                <input
                                  type="checkbox"
                                  checked={isChecked}
                                  onChange={() =>
                                    toggleChecked(selectedDayIndex, mealIndex, item)
                                  }
                                />
                                <span></span>
                              </label>

                              <div className="diet-food-name">
                                <strong>{item.foodName}</strong>
                                <small>{round(item.protein, 1)} g fehérje</small>
                              </div>

                              <div className="diet-food-weight">
                                {round(item.quantityGrams)} g
                              </div>

                              <div className="diet-food-kcal">
                                {round(item.calories)} kcal
                              </div>

                              <button
                                type="button"
                                className={
                                  isExpanded
                                    ? "diet-expand-btn open"
                                    : "diet-expand-btn"
                                }
                                onClick={() =>
                                  toggleExpanded(selectedDayIndex, mealIndex, item)
                                }
                              >
                               ⌄
                              </button>
                            </div>

                            {isExpanded && (
                              <div className="diet-food-expanded">
                                <div className="diet-weight-edit">
                                  <label>Súly módosítása</label>
                                  <input
                                    type="number"
                                    min="0"
                                    value={round(item.quantityGrams)}
                                    onChange={(e) =>
                                      updateFoodWeight(
                                        selectedDayIndex,
                                        mealIndex,
                                        itemIndex,
                                        e.target.value
                                      )
                                    }
                                  />
                                  <span>gramm</span>
                                </div>

                                <div className="diet-food-expanded-right">
                                  <div className="diet-expanded-macros">
                                    <div>
                                      <strong>{round(item.protein, 1)} g</strong>
                                      <span>Fehérje</span>
                                    </div>
                                    <div>
                                      <strong>{round(item.fat, 1)} g</strong>
                                      <span>Zsír</span>
                                    </div>
                                    <div>
                                      <strong>{round(item.carbs, 1)} g</strong>
                                      <span>Szénhidrát</span>
                                    </div>
                                  </div>

                                  <button
                                    type="button"
                                    className="diet-delete-food-btn"
                                    onClick={() =>
                                      removeFoodFromMeal(
                                        selectedDayIndex,
                                        mealIndex,
                                        itemIndex,
                                        item
                                      )
                                    }
                                  >
                                    Törlés
                                  </button>
                                </div>
                              </div>
                            )}
                          </div>
                        );
                      })
                    ) : (
                      <div className="diet-no-foods">
                        Ehhez az étkezéshez még nincs étel.
                      </div>
                    )}
                  </div>
                );
              })}
            </div>
          </section>
        </>
      )}

      {foodModal.isOpen && (
        <div className="diet-modal-backdrop" onClick={closeFoodModal}>
          <div className="diet-food-modal" onClick={(e) => e.stopPropagation()}>
            <div className="diet-modal-header">
              <button
                type="button"
                className="diet-back-btn"
                onClick={() => {
                  if (selectedFood) {
                    setSelectedFood(null);
                    return;
                  }

                  closeFoodModal();
                }}
              >
                ←
              </button>

              <h2>Új étel hozzáadása</h2>

              <button type="button" onClick={closeFoodModal}>
                ×
              </button>
            </div>

            <div className="diet-searchbar">
              <span>⌕</span>
              <input
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                placeholder="Keresés ételek, márkák alapján..."
              />
              {searchText && (
                <button type="button" onClick={() => setSearchText("")}>×</button>
              )}
            </div>

            <div className="diet-modal-tabs">
              <button type="button" className={foodModal.activeTab === "all" ? "active" : ""} onClick={() => setFoodModal((prev) => ({ ...prev, activeTab: "all" }))} >Összes</button>
              <button type="button" className={foodModal.activeTab === "myFoods" ? "active" : ""} onClick={() => setFoodModal((prev) => ({ ...prev, activeTab: "myFoods" }))} >Ételeim</button>
              <button type="button" className={foodModal.activeTab === "new" ? "active" : ""} onClick={() => setFoodModal((prev) => ({ ...prev, activeTab: "new" }))} >Saját étel hozzáadása</button>
            </div>

            <div className="diet-modal-content">
              {foodModal.activeTab === "all" && (
                <div className="diet-food-browser">

                  <h3>Találatok</h3>
                  {renderFoodList(filteredSqlFoods)}
                  {renderSelectedFoodDetails()}
                </div>
              )}

              {foodModal.activeTab === "myFoods" && (
                <div className="diet-food-browser">
                  <h3>My Foods</h3>
                  {renderFoodList(filteredCustomFoods)}
                  {renderSelectedFoodDetails()}
                </div>
              )}

              {foodModal.activeTab === "new" && (
                <form className="diet-custom-food-form" onSubmit={handleSaveCustomFood}>
                  <div className="diet-form-row wide">
                    <label>Étel neve</label>
                    <input
                      value={customForm.foodNameHu}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, foodNameHu: e.target.value }))}
                      placeholder="pl. Zabkása banánnal"
                      required
                    />
                  </div>

                  <div className="diet-form-row wide">
                    <label>Márka / megjegyzés</label>
                    <input
                      value={customForm.brand}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, brand: e.target.value }))}
                      placeholder="opcionális"
                    />
                  </div>

                  <div className="diet-form-row">
                    <label>Adag gramm</label>
                    <input
                      type="number"
                      min="1"
                      value={customForm.servingGrams}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, servingGrams: e.target.value }))}
                    />
                  </div>

                  <div className="diet-form-row">
                    <label>kcal / 100 g</label>
                    <input
                      type="number"
                      min="0"
                      value={customForm.kcalPer100}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, kcalPer100: e.target.value }))}
                    />
                  </div>

                  <div className="diet-form-row">
                    <label>Fehérje / 100 g</label>
                    <input
                      type="number"
                      min="0"
                      step="0.1"
                      value={customForm.proteinGPer100}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, proteinGPer100: e.target.value }))}
                    />
                  </div>

                  <div className="diet-form-row">
                    <label>Szénhidrát / 100 g</label>
                    <input
                      type="number"
                      min="0"
                      step="0.1"
                      value={customForm.carbsGPer100}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, carbsGPer100: e.target.value }))}
                    />
                  </div>

                  <div className="diet-form-row">
                    <label>Zsír / 100 g</label>
                    <input
                      type="number"
                      min="0"
                      step="0.1"
                      value={customForm.fatGPer100}
                      onChange={(e) => setCustomForm((prev) => ({ ...prev, fatGPer100: e.target.value }))}
                    />
                  </div>

                  <button type="submit" className="diet-primary-btn full">
                    Mentés a My Foods listába
                  </button>
                </form>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
