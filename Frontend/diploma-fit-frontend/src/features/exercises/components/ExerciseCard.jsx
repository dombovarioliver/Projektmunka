import { getVideoUrl } from "../../../utils/videoUrl";

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

const MOVEMENT_TYPE_LABELS = {
  compound: "Összetett",
  isolation: "Izolációs",
};

function prettifyValue(value) {
  if (value === null || value === undefined || value === "") {
    return "Nincs megadva";
  }

  if (Array.isArray(value)) {
    return value.map(prettifyValue).join(", ");
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

function getPrimaryMuscleGroup(exercise) {
  const value =
    exercise?.primaryMuscleGroup ||
    exercise?.muscleGroup ||
    exercise?.muscleGroupName ||
    exercise?.category ||
    exercise?.targetMuscle;

  return MUSCLE_GROUP_LABELS[value] || prettifyValue(value || "Egyéb");
}

function getPrimaryMuscleSubgroup(exercise) {
  const value = exercise?.primaryMuscleSubgroup || exercise?.primarySubgroup;
  return MUSCLE_SUBGROUP_LABELS[value] || prettifyValue(value || "Nincs megadva");
}

function getMovementType(exercise) {
  const value = exercise?.movementType;
  return MOVEMENT_TYPE_LABELS[value] || prettifyValue(value || "Nincs megadva");
}

function isHomeFriendlyExercise(exercise) {
  return Boolean(
    exercise?.isHomeFriendly ||
      exercise?.homeFriendly ||
      exercise?.canBeDoneAtHome ||
      exercise?.equipmentLevel === 0
  );
}

export default function ExerciseCard({ exercise, onClick }) {
  const videoSrc = getVideoUrl(exercise.videoUrl);
  const exerciseName = getExerciseName(exercise);
  const primaryMuscleGroup = getPrimaryMuscleGroup(exercise);
  const primaryMuscleSubgroup = getPrimaryMuscleSubgroup(exercise);
  const movementType = getMovementType(exercise);
  const homeFriendly = isHomeFriendlyExercise(exercise);

  return (
    <button className="exercise-card" type="button" onClick={() => onClick(exercise)}>
      <div className="exercise-card-video">
        {videoSrc ? (
          <video src={videoSrc} muted preload="metadata" playsInline />
        ) : (
          <div className="exercise-card-video-fallback">NF</div>
        )}

        <div className="exercise-card-overlay">
          <span>▶</span>
        </div>

        <div className="exercise-card-badge">{primaryMuscleGroup}</div>
      </div>

      <div className="exercise-card-body">
        <h3>{exerciseName}</h3>

        <div className="exercise-card-meta">
          <span>{primaryMuscleSubgroup}</span>
          <span>{movementType}</span>
          <span>{homeFriendly ? "Otthon is" : "Konditerem"}</span>
        </div>
      </div>
    </button>
  );
}
