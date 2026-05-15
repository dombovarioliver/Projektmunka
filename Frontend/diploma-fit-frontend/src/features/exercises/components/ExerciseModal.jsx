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

const EQUIPMENT_LABELS = {
  none: "Saját testsúly",
  bodyweight: "Saját testsúly",
  dumbbell: "Kézisúlyzó",
  dumbbells: "Kézisúlyzó",
  barbell: "Rúd",
  machine: "Gép",
  cable: "Kábel / csiga",
  kettlebell: "Kettlebell",
  band: "Gumiszalag",
};

const PATTERN_LABELS = {
  push: "Nyomás",
  pull: "Húzás",
  squat: "Guggolás",
  hinge: "Csípőhajlítás",
  lunge: "Kitörés",
  carry: "Cipelés",
  fly: "Tárogatás",
  curl: "Hajlítás",
  extension: "Nyújtás",
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

function getSecondaryMuscleGroup(exercise) {
  const value =
    exercise?.secondaryMuscleGroup ||
    exercise?.secondaryMuscleGroups ||
    exercise?.secondaryMuscles;

  if (!value || (Array.isArray(value) && value.length === 0)) {
    return "Nincs megadva";
  }

  if (Array.isArray(value)) {
    return value.map((item) => MUSCLE_GROUP_LABELS[item] || prettifyValue(item)).join(", ");
  }

  return MUSCLE_GROUP_LABELS[value] || prettifyValue(value);
}

function getSecondaryMuscleSubgroup(exercise) {
  const value = exercise?.secondaryMuscleSubgroup || exercise?.secondaryMuscleSubgroups;

  if (!value || (Array.isArray(value) && value.length === 0)) {
    return "Nincs megadva";
  }

  if (Array.isArray(value)) {
    return value.map((item) => MUSCLE_SUBGROUP_LABELS[item] || prettifyValue(item)).join(", ");
  }

  return MUSCLE_SUBGROUP_LABELS[value] || prettifyValue(value);
}

function getMovementType(exercise) {
  const value = exercise?.movementType;
  return MOVEMENT_TYPE_LABELS[value] || prettifyValue(value || "Nincs megadva");
}

function getPattern(exercise) {
  const value = exercise?.pattern || exercise?.pushPullCategory;
  return PATTERN_LABELS[value] || prettifyValue(value || "Nincs megadva");
}

function getEquipment(exercise) {
  const value = exercise?.equipment;
  return EQUIPMENT_LABELS[value] || prettifyValue(value || "Nincs megadva");
}

function isHomeFriendlyExercise(exercise) {
  return Boolean(
    exercise?.isHomeFriendly ||
      exercise?.homeFriendly ||
      exercise?.canBeDoneAtHome ||
      exercise?.equipmentLevel === 0
  );
}

export default function ExerciseModal({ exercise, onClose }) {
  if (!exercise) return null;

  const videoSrc = getVideoUrl(exercise.videoUrl);
  const exerciseName = getExerciseName(exercise);
  const primaryMuscleGroup = getPrimaryMuscleGroup(exercise);
  const primaryMuscleSubgroup = getPrimaryMuscleSubgroup(exercise);
  const secondaryMuscleGroup = getSecondaryMuscleGroup(exercise);
  const secondaryMuscleSubgroup = getSecondaryMuscleSubgroup(exercise);
  const movementType = getMovementType(exercise);
  const pattern = getPattern(exercise);
  const equipment = getEquipment(exercise);
  const homeFriendly = isHomeFriendlyExercise(exercise);

  return (
    <div className="exercise-modal-backdrop" onClick={onClose}>
      <div className="exercise-modal-content" onClick={(event) => event.stopPropagation()}>
        <div className="exercise-modal-header">
          <div>
            <span>{primaryMuscleGroup} · {primaryMuscleSubgroup}</span>
            <h2>{exerciseName}</h2>
          </div>

          <button type="button" onClick={onClose} aria-label="Bezárás">
            ✕
          </button>
        </div>

        <div className="exercise-modal-video-wrap">
          {videoSrc ? (
            <video
              key={videoSrc}
              src={videoSrc}
              controls
              autoPlay
              playsInline
              muted
              loop
              className="exercise-modal-video"
            />
          ) : (
            <div className="exercise-modal-video-empty">Ehhez a gyakorlathoz még nincs videó.</div>
          )}
        </div>

        <div className="exercise-modal-info-grid">
          <div>
            <strong>{primaryMuscleGroup}</strong>
            <span>Elsődleges izomcsoport</span>
          </div>

          <div>
            <strong>{primaryMuscleSubgroup}</strong>
            <span>Elsődleges izomrész</span>
          </div>

          <div>
            <strong>{secondaryMuscleGroup}</strong>
            <span>Másodlagos izomcsoport</span>
          </div>

          <div>
            <strong>{secondaryMuscleSubgroup}</strong>
            <span>Másodlagos izomrész</span>
          </div>

          <div>
            <strong>{movementType}</strong>
            <span>Mozgástípus</span>
          </div>

          <div>
            <strong>{pattern}</strong>
            <span>Mozgásminta</span>
          </div>

          <div>
            <strong>{equipment}</strong>
            <span>Eszköz</span>
          </div>

          <div>
            <strong>{homeFriendly ? "Otthon is végezhető" : "Konditermi gyakorlat"}</strong>
            <span>Helyszín</span>
          </div>

          <div>
            <strong>{exercise.videoUrl ? "Elérhető" : "Hiányzik"}</strong>
            <span>Videó</span>
          </div>
        </div>
      </div>
    </div>
  );
}
