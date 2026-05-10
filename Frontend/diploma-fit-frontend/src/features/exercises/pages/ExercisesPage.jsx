import { useEffect, useState } from "react";
import ExerciseCard from "../components/ExerciseCard";
import ExerciseModal from "../components/ExerciseModal";
import { getExercises } from "../services/exerciseService";

import "../styles/exercises.css";

export default function ExercisesPage() {
  const [exercises, setExercises] = useState([]);
  const [selectedExercise, setSelectedExercise] = useState(null);

  useEffect(() => {
    let isMounted = true;

    async function loadExercises() {
      try {
        const data = await getExercises();

        if (isMounted) {
          setExercises(data);
        }
      } catch (err) {
        console.error(err);
      }
    }

    loadExercises();

    return () => {
      isMounted = false;
    };
  }, []);

  return (
    <div className="exercises-page">
      <div className="exercise-grid">
        {exercises.map((exercise) => (
          <ExerciseCard
            key={exercise.exerciseId}
            exercise={exercise}
            onClick={setSelectedExercise}
          />
        ))}
      </div>

      <ExerciseModal
        exercise={selectedExercise}
        onClose={() => setSelectedExercise(null)}
      />
    </div>
  );
}