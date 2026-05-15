import { APIProvider, Map, Marker, InfoWindow } from "@vis.gl/react-google-maps";
import { useEffect, useMemo, useState } from "react";
import { getBudapestGyms } from "../gymService";

const BUDAPEST_CENTER = {
  lat: 47.4979,
  lng: 19.0402,
};

const googleMapsApiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;

function getGymKey(gym, index) {
  return gym.placeId || gym.id || `${gym.name}-${index}`;
}

function formatRating(rating) {
  if (rating === null || rating === undefined || rating === "") {
    return "Nincs értékelés";
  }

  return `${rating} / 5`;
}

export default function GymsMap() {
  const [gyms, setGyms] = useState([]);
  const [selectedGym, setSelectedGym] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function loadGyms() {
      try {
        const data = await getBudapestGyms();
        setGyms(Array.isArray(data) ? data : []);
      } catch (error) {
        console.error(error);
        setErrorMessage("Nem sikerült betölteni a konditermek adatait.");
      } finally {
        setIsLoading(false);
      }
    }

    loadGyms();
  }, []);

  const averageRating = useMemo(() => {
    const ratings = gyms
      .map((gym) => Number(gym.rating))
      .filter((rating) => !Number.isNaN(rating) && rating > 0);

    if (ratings.length === 0) {
      return "-";
    }

    const sum = ratings.reduce((acc, rating) => acc + rating, 0);
    return (sum / ratings.length).toFixed(1);
  }, [gyms]);

  if (isLoading) {
    return (
      <section className="gyms-state-card">
        <div className="gyms-loader" />
        <h2>Konditermek betöltése...</h2>
        <p>Betöltjük a Budapesten található edzőtermeket.</p>
      </section>
    );
  }

  if (errorMessage) {
    return (
      <section className="gyms-state-card error">
        <h2>Hiba történt</h2>
        <p>{errorMessage}</p>
      </section>
    );
  }

  if (!googleMapsApiKey) {
    return (
      <section className="gyms-state-card warning">
        <h2>Hiányzó Google Maps API kulcs</h2>
        <p>
          A térkép nem jeleníthető meg, mert hiányzik a
          <strong> VITE_GOOGLE_MAPS_API_KEY</strong> környezeti változó.
        </p>
      </section>
    );
  }

  return (
    <APIProvider apiKey={googleMapsApiKey}>
      <section className="gyms-dashboard-card">
        <div className="gyms-toolbar">
          <div>
            <span className="gyms-card-label">Térképes kereső</span>
            <h2>Edzőtermek Budapesten</h2>
          </div>

          <div className="gyms-summary-pills">
            <div>
              <strong>{gyms.length}</strong>
              <span>hely</span>
            </div>

            <div>
              <strong>{averageRating}</strong>
              <span>átlag értékelés</span>
            </div>
          </div>
        </div>

        <div className="gyms-map-layout">
          <aside className="gyms-list-panel">
            <div className="gyms-list-head">
              <h3>Találatok</h3>
              <span>{gyms.length} db</span>
            </div>

            <div className="gyms-list">
              {gyms.map((gym, index) => {
                const isSelected = selectedGym && getGymKey(selectedGym, index) === getGymKey(gym, index);

                return (
                  <button
                    type="button"
                    className={isSelected ? "gyms-list-item active" : "gyms-list-item"}
                    key={getGymKey(gym, index)}
                    onClick={() => setSelectedGym(gym)}
                  >
                    <span className="gyms-pin-dot" />

                    <span className="gyms-list-text">
                      <strong>{gym.name}</strong>
                      <small>{gym.address || "Nincs cím megadva"}</small>
                    </span>

                    <span className="gyms-rating-chip">
                      ⭐ {gym.rating ?? "-"}
                    </span>
                  </button>
                );
              })}
            </div>
          </aside>

          <div className="gyms-map-panel">
            <Map
              defaultCenter={BUDAPEST_CENTER}
              defaultZoom={12}
              gestureHandling="greedy"
              disableDefaultUI={false}
            >
              {gyms.map((gym, index) => (
                <Marker
                  key={getGymKey(gym, index)}
                  position={{
                    lat: gym.latitude,
                    lng: gym.longitude,
                  }}
                  title={gym.name}
                  onClick={() => setSelectedGym(gym)}
                />
              ))}

              {selectedGym && (
                <InfoWindow
                  position={{
                    lat: selectedGym.latitude,
                    lng: selectedGym.longitude,
                  }}
                  onCloseClick={() => setSelectedGym(null)}
                >
                  <div className="gyms-info-window">
                    <h3>{selectedGym.name}</h3>
                    <p>{selectedGym.address || "Nincs cím megadva"}</p>
                    <strong>⭐ {formatRating(selectedGym.rating)}</strong>
                  </div>
                </InfoWindow>
              )}
            </Map>
          </div>
        </div>
      </section>
    </APIProvider>
  );
}
