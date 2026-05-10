import { getVideoUrl } from "../../../utils/videoUrl";



export default function ExerciseModal({ exercise, onClose }) {
  if (!exercise) return null;
    const videoSrc = getVideoUrl(exercise.videoUrl);
    console.log("MODAL VIDEO URL:", videoSrc);
  return (
    
    <div className="exercise-modal-backdrop" onClick={onClose}>
      <div
        className="exercise-modal-content"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="exercise-modal-header">
          <h3>{exercise.nameHu}</h3>

          <button onClick={onClose}>
            ✕
          </button>
        </div>

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
      </div>
    </div>
  );
}