import { useState } from "react";
import { generateWorkoutPlan } from "../services/workoutPlanService";

import "../styles/workoutPlan.css";

const WORKOUT_PLAN_STORAGE_KEY = "neurafit_workout_plan";

const WORKOUT_CHECKED_STORAGE_KEY =
  "neurafit_completed_exercises";

export default function WorkoutPlanPage() {
  const [workoutPlan, setWorkoutPlan] = useState(() => {
  const savedPlan = localStorage.getItem(WORKOUT_PLAN_STORAGE_KEY);

  return savedPlan ? JSON.parse(savedPlan) : null;
});

  const [selectedDayIndex, setSelectedDayIndex] =
    useState(0);

  const [isModalOpen, setIsModalOpen] =
    useState(false);

  const [completedExercises, setCompletedExercises] = useState(() => {
  const savedCompleted = localStorage.getItem(WORKOUT_CHECKED_STORAGE_KEY);

  return savedCompleted ? JSON.parse(savedCompleted) : {};
});

  const [formData, setFormData] = useState({
    gender: 0,
    age: 22,
    goal_type: 2,
    activity_level: 3,
    experience: 2,
    days_per_week: 5,
    equipment_level: 1,
  });

  const selectedDay =
    workoutPlan?.days?.[selectedDayIndex];

  function updateField(name, value) {
    setFormData((prev) => ({
      ...prev,
      [name]: Number(value),
    }));
  }

  function toggleExerciseCompleted(exerciseKey) {
    setCompletedExercises((prev) => {
      const updated = {
        ...prev,
        [exerciseKey]: !prev[exerciseKey],
      };

      localStorage.setItem(
        WORKOUT_CHECKED_STORAGE_KEY,
        JSON.stringify(updated)
      );

      return updated;
    });
  }

  async function handleGenerateWorkoutPlan(e) {
    e.preventDefault();

    try {
      const data = await generateWorkoutPlan(
        formData
      );

      localStorage.setItem(
        WORKOUT_PLAN_STORAGE_KEY,
        JSON.stringify(data)
      );

      localStorage.removeItem(
        WORKOUT_CHECKED_STORAGE_KEY
      );

      setCompletedExercises({});

      setWorkoutPlan(data);

      setSelectedDayIndex(0);

      setIsModalOpen(false);
    } catch (err) {
      console.error(
        "Edzésterv generálási hiba:",
        err
      );
    }
  }

  return (
    <div className="workout-plan-page">
      <div className="workout-plan-header">
        <button
          className="generate-workout-btn"
          onClick={() =>
            setIsModalOpen(true)
          }
        >
          + Új edzéstervet generálok
        </button>
      </div>

      {!workoutPlan && (
        <div className="empty-workout-plan">
          Még nincs generált edzésterved.
        </div>
      )}

      {workoutPlan && selectedDay && (
        <div className="workout-result">
          <div className="workout-summary">
            <h2>
              {workoutPlan.splitName}
            </h2>

            <p>
              {
                workoutPlan.daysPerWeek
              }{" "}
              edzésnap / hét
            </p>
          </div>

          <div className="workout-day-tabs">
            {workoutPlan.days.map(
              (day, index) => (
                <button
                  key={day.dayIndex}
                  className={
                    selectedDayIndex ===
                    index
                      ? "workout-day-tab active"
                      : "workout-day-tab"
                  }
                  onClick={() =>
                    setSelectedDayIndex(
                      index
                    )
                  }
                >
                  {day.dayIndex}. nap
                </button>
              )
            )}
          </div>

          <div className="selected-workout-day">
            <h3>
              {selectedDay.dayIndex}. nap:{" "}
              {selectedDay.dayType}
            </h3>

            {selectedDay.exercises.length ===
            0 ? (
              <div className="rest-day-box">
                Pihenőnap
              </div>
            ) : (
              <div className="workout-exercise-list">
                {selectedDay.exercises.map(
                  (exercise, index) => {
                    const exerciseKey = `${selectedDay.dayIndex}-${exercise.exerciseId}`;

                    const isCompleted =
                      completedExercises[
                        exerciseKey
                      ] || false;

                    return (
                      <div
                        className={
                          isCompleted
                            ? "workout-exercise-row completed"
                            : "workout-exercise-row"
                        }
                        key={`${exercise.exerciseId}-${index}`}
                      >
                        <div className="exercise-order">
                          {index + 1}
                        </div>

                        <input
                          type="checkbox"
                          className="exercise-checkbox"
                          checked={
                            isCompleted
                          }
                          onChange={() =>
                            toggleExerciseCompleted(
                              exerciseKey
                            )
                          }
                        />

                        <div className="exercise-info">
                          <h4>
                            {
                              exercise.nameHu
                            }
                          </h4>

                          <p>
                            {
                              exercise.sets
                            }{" "}
                            sorozat ·{" "}
                            {
                              exercise.repsLow
                            }
                            -
                            {
                              exercise.repsHigh
                            }{" "}
                            ismétlés
                          </p>
                        </div>
                      </div>
                    );
                  }
                )}
              </div>
            )}
          </div>
        </div>
      )}

      {isModalOpen && (
        <div
          className="workout-modal-backdrop"
          onClick={() =>
            setIsModalOpen(false)
          }
        >
          <div
            className="workout-modal"
            onClick={(e) =>
              e.stopPropagation()
            }
          >
            <h2>
              Edzésterv generálása
            </h2>

            <form
              className="workout-form"
              onSubmit={
                handleGenerateWorkoutPlan
              }
            >
              <div className="form-group">
                <label>Nem</label>

                <select
                  value={formData.gender}
                  onChange={(e) =>
                    updateField(
                      "gender",
                      e.target.value
                    )
                  }
                >
                  <option value={0}>
                    Férfi
                  </option>

                  <option value={1}>
                    Nő
                  </option>
                </select>
              </div>

              <div className="form-group">
                <label>Kor</label>

                <input
                  type="number"
                  min={12}
                  max={100}
                  value={formData.age}
                  onChange={(e) =>
                    updateField(
                      "age",
                      e.target.value
                    )
                  }
                />
              </div>

              <div className="form-group">
                <label>Cél</label>

                <select
                  value={
                    formData.goal_type
                  }
                  onChange={(e) =>
                    updateField(
                      "goal_type",
                      e.target.value
                    )
                  }
                >
                  <option value={0}>
                    Szinten tartás
                  </option>

                  <option value={1}>
                    Szálkásítás
                  </option>

                  <option value={2}>
                    Tömegelés
                  </option>
                </select>
              </div>

              <div className="form-group">
                <label>
                  Aktivitási szint
                </label>

                <select
                  value={
                    formData.activity_level
                  }
                  onChange={(e) =>
                    updateField(
                      "activity_level",
                      e.target.value
                    )
                  }
                >
                  <option value={1}>
                    1 - Alacsony
                  </option>

                  <option value={2}>
                    2 - Enyhe
                  </option>

                  <option value={3}>
                    3 - Közepes
                  </option>

                  <option value={4}>
                    4 - Magas
                  </option>

                  <option value={5}>
                    5 - Nagyon magas
                  </option>
                </select>
              </div>

              <div className="form-group">
                <label>
                  Tapasztalat
                </label>

                <select
                  value={
                    formData.experience
                  }
                  onChange={(e) =>
                    updateField(
                      "experience",
                      e.target.value
                    )
                  }
                >
                  <option value={0}>
                    Kezdő
                  </option>

                  <option value={1}>
                    Középhaladó
                  </option>

                  <option value={2}>
                    Haladó
                  </option>
                </select>
              </div>

              <div className="form-group">
                <label>
                  Edzésnapok száma
                  hetente
                </label>

                <input
                  type="number"
                  min={1}
                  max={7}
                  value={
                    formData.days_per_week
                  }
                  onChange={(e) =>
                    updateField(
                      "days_per_week",
                      e.target.value
                    )
                  }
                />
              </div>

              <div className="form-group">
                <label>
                  Felszerelés
                </label>

                <select
                  value={
                    formData.equipment_level
                  }
                  onChange={(e) =>
                    updateField(
                      "equipment_level",
                      e.target.value
                    )
                  }
                >
                  <option value={0}>
                    Otthon
                  </option>

                  <option value={1}>
                    Konditerem
                  </option>
                </select>
              </div>

              <button
                type="submit"
                className="submit-workout-btn"
              >
                Edzésterv generálása
              </button>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}