export default function ExerciseCard({ exercise, onClick }) {
  return (
    <button
      className="exercise-card"
      onClick={() => onClick(exercise)}
    >
      <div className="exercise-card-video">
        <video
          src={exercise.videoUrl}
          muted
          preload="metadata"
        />

        <div className="exercise-card-overlay">
          <span>▶</span>
        </div>
      </div>

      <div className="exercise-card-name">
        {exercise.nameHu}
      </div>
    </button>
  );
}