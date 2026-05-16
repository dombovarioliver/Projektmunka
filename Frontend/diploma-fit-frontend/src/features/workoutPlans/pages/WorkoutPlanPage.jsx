import { useMemo, useState } from "react";
import { generateWorkoutPlan } from "../services/workoutPlanService";

import "../styles/workoutPlan.css";

const WORKOUT_PLAN_STORAGE_KEY = "neurafit_workout_plan";
const WORKOUT_CHECKED_STORAGE_KEY = "neurafit_completed_exercises";

function safeParseStorage(key, fallback) {
  try {
    const value = localStorage.getItem(key);
    return value ? JSON.parse(value) : fallback;
  } catch {
    return fallback;
  }
}

function getExerciseName(exercise) {
  return exercise?.nameHu || exercise?.name || exercise?.exerciseName || "Gyakorlat";
}

function getDayExercises(day) {
  return Array.isArray(day?.exercises) ? day.exercises : [];
}

export default function WorkoutPlanPage() {
  const [workoutPlan, setWorkoutPlan] = useState(() =>
    safeParseStorage(WORKOUT_PLAN_STORAGE_KEY, null)
  );

  const [selectedDayIndex, setSelectedDayIndex] = useState(0);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isGenerating, setIsGenerating] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const [completedExercises, setCompletedExercises] = useState(() =>
    safeParseStorage(WORKOUT_CHECKED_STORAGE_KEY, {})
  );

  const [formData, setFormData] = useState({
    gender: 0,
    age: 22,
    goal_type: 2,
    activity_level: 3,
    experience: 2,
    days_per_week: 5,
    equipment_level: 1,
  });

  const selectedDay = workoutPlan?.days?.[selectedDayIndex];
  const selectedDayExercises = getDayExercises(selectedDay);

  const planStats = useMemo(() => {
    const days = workoutPlan?.days || [];
    const totalExercises = days.reduce((sum, day) => sum + getDayExercises(day).length, 0);
    const completedCount = days.reduce((sum, day) => {
      return (
        sum +
        getDayExercises(day).filter((exercise) => {
          const key = `${day.dayIndex}-${exercise.exerciseId}`;
          return completedExercises[key];
        }).length
      );
    }, 0);

    const progress = totalExercises > 0 ? Math.round((completedCount / totalExercises) * 100) : 0;

    return {
      totalExercises,
      completedCount,
      progress,
      trainingDays: days.filter((day) => getDayExercises(day).length > 0).length,
    };
  }, [workoutPlan, completedExercises]);

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

      localStorage.setItem(WORKOUT_CHECKED_STORAGE_KEY, JSON.stringify(updated));

      return updated;
    });
  }

  async function handleGenerateWorkoutPlan(event) {
    event.preventDefault();

    try {
      setIsGenerating(true);
      setErrorMessage("");

      const data = await generateWorkoutPlan(formData);

      localStorage.setItem(WORKOUT_PLAN_STORAGE_KEY, JSON.stringify(data));
      localStorage.removeItem(WORKOUT_CHECKED_STORAGE_KEY);

      setCompletedExercises({});
      setWorkoutPlan(data);
      setSelectedDayIndex(0);
      setIsModalOpen(false);
    } catch (err) {
      console.error("Edzésterv generálási hiba:", err);
      setErrorMessage("Nem sikerült legenerálni az edzéstervet. Próbáld újra később.");
    } finally {
      setIsGenerating(false);
    }
  }

  function clearWorkoutPlan() {
    localStorage.removeItem(WORKOUT_PLAN_STORAGE_KEY);
    localStorage.removeItem(WORKOUT_CHECKED_STORAGE_KEY);
    setWorkoutPlan(null);
    setCompletedExercises({});
    setSelectedDayIndex(0);
  }

  return (
    <main className="workout-plan-page">
      <section className="workout-hero">
        <div className="workout-hero-content">
          <span className="workout-eyebrow">AI edzésterv</span>

          <h1>Generálj edzéstervet, kövesd a napokat és pipáld ki a teljesített gyakorlatokat.</h1>

          <p>
            A NeuraFit edzésterv napokra bontva jelenik meg, így könnyen átlátod,
            hogy mikor mit kell edzened.
          </p>

          <div className="workout-hero-actions">
            <button type="button" className="generate-workout-btn" onClick={() => setIsModalOpen(true)}>
              Új edzésterv generálása
            </button>

            {workoutPlan && (
              <button type="button" className="clear-workout-btn" onClick={clearWorkoutPlan}>
                Terv törlése
              </button>
            )}
          </div>
        </div>

        <div className="workout-progress-panel">
          <div className="workout-progress-ring">
            <div>
              <strong>{planStats.progress}%</strong>
              <span>kész</span>
            </div>
          </div>

          <div className="workout-progress-stats">
            <div>
              <strong>{planStats.completedCount}</strong>
              <span>kész gyakorlat</span>
            </div>

            <div>
              <strong>{planStats.totalExercises}</strong>
              <span>összes gyakorlat</span>
            </div>

            <div>
              <strong>{planStats.trainingDays}</strong>
              <span>edzésnap</span>
            </div>
          </div>
        </div>
      </section>

      {errorMessage && <div className="workout-alert">{errorMessage}</div>}

      {!workoutPlan && (
        <section className="empty-workout-plan">
          <div>
            <span>🏋️</span>
            <h2>Még nincs generált edzésterved</h2>
            <p>Kattints az új edzésterv generálására, add meg az adataidat, és elkészül a heti terv.</p>
          </div>
        </section>
      )}

      {workoutPlan && selectedDay && (
        <section className="workout-result">
          <div className="workout-summary-card">
            <div>
              <span>Aktuális terv</span>
              <h2>{workoutPlan.splitName || "Személyre szabott edzésterv"}</h2>
              <p>{workoutPlan.daysPerWeek || planStats.trainingDays} edzésnap / hét</p>
            </div>

            <div className="workout-summary-pill">
              {planStats.progress}% teljesítve
            </div>
          </div>

          <div className="workout-day-tabs">
            {workoutPlan.days.map((day, index) => (
              <button
                type="button"
                key={day.dayIndex || index}
                className={selectedDayIndex === index ? "workout-day-tab active" : "workout-day-tab"}
                onClick={() => setSelectedDayIndex(index)}
              >
                {day.dayIndex || index + 1}. nap
              </button>
            ))}
          </div>

          <div className="selected-workout-day">
            <div className="selected-workout-day-header">
              <div>
                <span>{selectedDay.dayIndex}. nap</span>
                <h3>{selectedDay.dayType || "Edzésnap"}</h3>
              </div>

              <div className="selected-workout-day-count">
                {selectedDayExercises.length} gyakorlat
              </div>
            </div>

            {selectedDayExercises.length === 0 ? (
              <div className="rest-day-box">
                <span>☕</span>
                <strong>Pihenőnap</strong>
                <p>Ma nincs edzés. Regenerálódj, aludj eleget és figyelj a folyadékra.</p>
              </div>
            ) : (
              <div className="workout-exercise-list">
                {selectedDayExercises.map((exercise, index) => {
                  const exerciseKey = `${selectedDay.dayIndex}-${exercise.exerciseId}`;
                  const isCompleted = completedExercises[exerciseKey] || false;

                  return (
                    <div
                      className={isCompleted ? "workout-exercise-row completed" : "workout-exercise-row"}
                      key={`${exercise.exerciseId || getExerciseName(exercise)}-${index}`}
                    >
                      <div className="exercise-order">{index + 1}</div>

                      <label className="workout-checkbox-wrap">
                        <input
                          type="checkbox"
                          checked={isCompleted}
                          onChange={() => toggleExerciseCompleted(exerciseKey)}
                        />
                        <span />
                      </label>

                      <div className="exercise-info">
                        <h4>{getExerciseName(exercise)}</h4>
                        <p>
                          {exercise.sets || 0} sorozat · {exercise.repsLow || "?"}-{exercise.repsHigh || "?"} ismétlés
                        </p>
                      </div>

                      <div className="exercise-row-tag">
                        {isCompleted ? "Kész" : "Hátra van"}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </section>
      )}

      {isModalOpen && (
        <div className="workout-modal-backdrop" onClick={() => setIsModalOpen(false)}>
          <div className="workout-modal" onClick={(event) => event.stopPropagation()}>
            <div className="workout-modal-header">
              <div>
                <span>Beállítások</span>
                <h2>Edzésterv generálása</h2>
              </div>

              <button type="button" onClick={() => setIsModalOpen(false)} aria-label="Bezárás">
                ✕
              </button>
            </div>

            <form className="workout-form" onSubmit={handleGenerateWorkoutPlan}>
              <div className="workout-form-grid">
                <div className="form-group">
                  <label>Nem</label>
                  <select value={formData.gender} onChange={(event) => updateField("gender", event.target.value)}>
                    <option value={0}>Férfi</option>
                    <option value={1}>Nő</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Kor</label>
                  <input
                    type="number"
                    min={12}
                    max={100}
                    value={formData.age}
                    onChange={(event) => updateField("age", event.target.value)}
                  />
                </div>

                <div className="form-group">
                  <label>Cél</label>
                  <select value={formData.goal_type} onChange={(event) => updateField("goal_type", event.target.value)}>
                    <option value={0}>Szinten tartás</option>
                    <option value={1}>Szálkásítás</option>
                    <option value={2}>Tömegelés</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Aktivitási szint</label>
                  <select value={formData.activity_level} onChange={(event) => updateField("activity_level", event.target.value)}>
                    <option value={1}>1 - Alacsony</option>
                    <option value={2}>2 - Enyhe</option>
                    <option value={3}>3 - Közepes</option>
                    <option value={4}>4 - Magas</option>
                    <option value={5}>5 - Nagyon magas</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Tapasztalat</label>
                  <select value={formData.experience} onChange={(event) => updateField("experience", event.target.value)}>
                    <option value={0}>Kezdő</option>
                    <option value={1}>Középhaladó</option>
                    <option value={2}>Haladó</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Edzésnapok hetente</label>
                  <input
                    type="number"
                    min={1}
                    max={7}
                    value={formData.days_per_week}
                    onChange={(event) => updateField("days_per_week", event.target.value)}
                  />
                </div>

                <div className="form-group wide">
                  <label>Felszerelés</label>
                  <select value={formData.equipment_level} onChange={(event) => updateField("equipment_level", event.target.value)}>
                    <option value={0}>Otthon</option>
                    <option value={1}>Konditerem</option>
                  </select>
                </div>
              </div>

              <button type="submit" className="submit-workout-btn" disabled={isGenerating}>
                {isGenerating ? "Generálás..." : "Edzésterv generálása"}
              </button>
            </form>
          </div>
        </div>
      )}
    </main>
  );
}
