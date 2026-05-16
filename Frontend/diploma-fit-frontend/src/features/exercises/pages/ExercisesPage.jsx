import { useEffect, useMemo, useState } from "react";
import ExerciseCard from "../components/ExerciseCard";
import ExerciseModal from "../components/ExerciseModal";
import { getExercises } from "../services/exerciseService";

import "../styles/exercises.css";

const MUSCLE_GROUP_LABELS = {
  chest: "Mell",
  back: "Hát",
  shoulders: "Váll",
  shoulder: "Váll",
  biceps: "Bicepsz",
  triceps: "Tricepsz",
  legs: "Láb",
  quadriceps: "Combfeszítő",
  hamstrings: "Combhajlító",
  glutes: "Farizom",
  calves: "Vádli",
  abs: "Has",
  core: "Törzs",
  traps: "Csuklya",
  forearms: "Alkar",
};

const MUSCLE_SUBGROUP_LABELS = {
  chest_mid: "Mell középső része",
  chest_upper: "Felső mell",
  chest_lower: "Alsó mell",
  lats: "Széles hátizom",
  upper_back: "Felső hát",
  lower_back: "Alsó hát",
  front_delts: "Első váll",
  side_delts: "Oldalsó váll",
  rear_delts: "Hátsó váll",
  quads: "Combfeszítő",
  hamstrings: "Combhajlító",
  glutes: "Farizom",
  calves: "Vádli",
  abs: "Hasizom",
  obliques: "Ferde hasizom",
};

function prettifyValue(value) {
  if (value === null || value === undefined || value === "") {
    return "Nincs megadva";
  }

  return String(value)
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/[_-]/g, " ")
    .replace(/\s+/g, " ")
    .trim()
    .replace(/^./, (char) => char.toUpperCase());
}

function getExerciseName(exercise) {
  return exercise?.nameHu || exercise?.name || exercise?.exerciseName || "Névtelen gyakorlat";
}

function getPrimaryMuscleGroupRaw(exercise) {
  return (
    exercise?.primaryMuscleGroup ||
    exercise?.muscleGroup ||
    exercise?.muscleGroupName ||
    exercise?.category ||
    exercise?.targetMuscle ||
    "other"
  );
}

function getPrimaryMuscleGroup(exercise) {
  const value = getPrimaryMuscleGroupRaw(exercise);
  return MUSCLE_GROUP_LABELS[value] || prettifyValue(value || "Egyéb");
}

function getPrimaryMuscleSubgroup(exercise) {
  const value = exercise?.primaryMuscleSubgroup || exercise?.primarySubgroup;
  return MUSCLE_SUBGROUP_LABELS[value] || prettifyValue(value || "");
}

function isHomeFriendlyExercise(exercise) {
  return Boolean(
    exercise?.isHomeFriendly ||
      exercise?.homeFriendly ||
      exercise?.canBeDoneAtHome ||
      exercise?.equipmentLevel === 0
  );
}

function exerciseMatchesSearch(exercise, normalizedSearch) {
  if (!normalizedSearch) {
    return true;
  }

  const searchableText = [
    getExerciseName(exercise),
    exercise?.nameEn,
    getPrimaryMuscleGroup(exercise),
    getPrimaryMuscleSubgroup(exercise),
    exercise?.primaryMuscleGroup,
    exercise?.primaryMuscleSubgroup,
    exercise?.movementType,
    exercise?.pattern,
    exercise?.equipment,
  ]
    .filter(Boolean)
    .join(" ")
    .toLowerCase();

  return searchableText.includes(normalizedSearch);
}

export default function ExercisesPage() {
  const [exercises, setExercises] = useState([]);
  const [selectedExercise, setSelectedExercise] = useState(null);
  const [selectedMuscleGroup, setSelectedMuscleGroup] = useState("Összes");
  const [searchTerm, setSearchTerm] = useState("");
  const [homeOnly, setHomeOnly] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    let isMounted = true;

    async function loadExercises() {
      try {
        setIsLoading(true);
        setErrorMessage("");

        const data = await getExercises();

        if (isMounted) {
          setExercises(Array.isArray(data) ? data : []);
        }
      } catch (err) {
        console.error(err);

        if (isMounted) {
          setErrorMessage("Nem sikerült betölteni a gyakorlatokat.");
        }
      } finally {
        if (isMounted) {
          setIsLoading(false);
        }
      }
    }

    loadExercises();

    return () => {
      isMounted = false;
    };
  }, []);

  const muscleGroups = useMemo(() => {
    const groups = new Set(exercises.map(getPrimaryMuscleGroup).filter(Boolean));
    return ["Összes", ...Array.from(groups).sort((a, b) => a.localeCompare(b, "hu"))];
  }, [exercises]);

  const filteredExercises = useMemo(() => {
    const normalizedSearch = searchTerm.trim().toLowerCase();

    return exercises.filter((exercise) => {
      const muscleGroup = getPrimaryMuscleGroup(exercise);

      const matchesSearch = exerciseMatchesSearch(exercise, normalizedSearch);
      const matchesGroup = selectedMuscleGroup === "Összes" || muscleGroup === selectedMuscleGroup;
      const matchesHome = !homeOnly || isHomeFriendlyExercise(exercise);

      return matchesSearch && matchesGroup && matchesHome;
    });
  }, [exercises, searchTerm, selectedMuscleGroup, homeOnly]);

  const homeFriendlyCount = useMemo(
    () => exercises.filter(isHomeFriendlyExercise).length,
    [exercises]
  );

  return (
    <main className="exercises-page">
      <section className="exercises-hero">
        <div className="exercises-hero-content">
          <span className="exercises-eyebrow">Gyakorlat adatbázis</span>

          <h1>Válassz gyakorlatot izomcsoportra, célra és edzéshelyszínre szűrve.</h1>

          <p>
            Böngéssz a NeuraFit gyakorlatok között, nézd meg a videós bemutatót,
            és használd őket az edzésterveidhez.
          </p>
        </div>

        <div className="exercises-hero-stats">
          <div>
            <strong>{exercises.length}</strong>
            <span>gyakorlat</span>
          </div>

          <div>
            <strong>{muscleGroups.length - 1}</strong>
            <span>elsődleges izomcsoport</span>
          </div>

          <div>
            <strong>{homeFriendlyCount}</strong>
            <span>otthoni</span>
          </div>
        </div>
      </section>

      <section className="exercises-toolbar">
        <div className="exercises-search-box">
          <span>⌕</span>
          <input
            type="text"
            value={searchTerm}
            onChange={(event) => setSearchTerm(event.target.value)}
            placeholder="Keresés név, izomcsoport, eszköz vagy mozgástípus alapján..."
          />
        </div>

        <button
          type="button"
          className={homeOnly ? "exercises-home-toggle active" : "exercises-home-toggle"}
          onClick={() => setHomeOnly((prev) => !prev)}
        >
          Csak otthoni
        </button>
      </section>

      <section className="exercises-category-tabs">
        {muscleGroups.map((group) => (
          <button
            type="button"
            key={group}
            className={selectedMuscleGroup === group ? "exercise-category-tab active" : "exercise-category-tab"}
            onClick={() => setSelectedMuscleGroup(group)}
          >
            {group}
          </button>
        ))}
      </section>

      {errorMessage && <div className="exercises-message error">{errorMessage}</div>}

      {isLoading && <div className="exercises-message">Gyakorlatok betöltése...</div>}

      {!isLoading && !errorMessage && filteredExercises.length === 0 && (
        <div className="exercises-empty-card">
          <h2>Nincs találat</h2>
          <p>Próbálj másik keresést vagy kapcsold ki az otthoni szűrőt.</p>
        </div>
      )}

      {!isLoading && filteredExercises.length > 0 && (
        <section className="exercise-grid">
          {filteredExercises.map((exercise) => (
            <ExerciseCard
              key={exercise.exerciseId || exercise.id || getExerciseName(exercise)}
              exercise={exercise}
              onClick={setSelectedExercise}
            />
          ))}
        </section>
      )}

      <ExerciseModal exercise={selectedExercise} onClose={() => setSelectedExercise(null)} />
    </main>
  );
}
