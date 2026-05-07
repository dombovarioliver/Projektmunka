import { APIProvider, Map, Marker, InfoWindow } from "@vis.gl/react-google-maps";
import { useEffect, useState } from "react";
import { getBudapestGyms } from "../gymService";

const BUDAPEST_CENTER = {
  lat: 47.4979,
  lng: 19.0402,
};

const googleMapsApiKey = import.meta.env.VITE_GOOGLE_MAPS_API_KEY;

export default function GymsMap() {
  const [gyms, setGyms] = useState([]);
  const [selectedGym, setSelectedGym] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function loadGyms() {
      try {
        const data = await getBudapestGyms();
        setGyms(data);
      } catch (error) {
        console.error(error);
        setErrorMessage("Nem sikerült betölteni a konditermek adatait.");
      } finally {
        setIsLoading(false);
      }
    }

    loadGyms();
  }, []);

  if (isLoading) {
    return <div className="alert alert-info">Konditermek betöltése...</div>;
  }

  if (errorMessage) {
    return <div className="alert alert-danger">{errorMessage}</div>;
  }

  if (!googleMapsApiKey) {
    return (
      <div className="alert alert-warning">
        A térkép nem jeleníthető meg, mert hiányzik a Google Maps API kulcs
        (`VITE_GOOGLE_MAPS_API_KEY`).
      </div>
    );
  }

  return (
    <APIProvider apiKey={googleMapsApiKey}>
      <div className="rounded shadow-sm overflow-hidden" style={{ height: "650px" }}>
        <Map
          defaultCenter={BUDAPEST_CENTER}
          defaultZoom={12}
          gestureHandling="greedy"
          disableDefaultUI={false}
        >
          {gyms.map((gym) => (
            <Marker
              key={gym.placeId}
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
              <div style={{ minWidth: "220px" }}>
                <h6 className="mb-1">{selectedGym.name}</h6>

                <p className="mb-2 text-muted">{selectedGym.address}</p>

                <div className="fw-semibold">
                  ⭐ {selectedGym.rating ?? "Nincs értékelés"} / 5
                </div>
              </div>
            </InfoWindow>
          )}
        </Map>
      </div>

      <p className="text-muted mt-3">
        Betöltött konditermek száma: {gyms.length}
      </p>
    </APIProvider>
  );
}