import GymsMap from "../components/GymsMap";
import "../styles/gymsMap.css";

export default function GymsMapPage() {
  return (
    <main className="gyms-page">
      <section className="gyms-hero-card">
        <div className="gyms-hero-content">
          <span className="gyms-eyebrow">NeuraFit térkép</span>

          <h1>Budapesti konditermek</h1>

          <p>
            Keress edzőtermet Budapesten modern térképes nézetben, értékeléssel
            és gyors áttekintéssel.
          </p>
        </div>

        <div className="gyms-hero-stats">
          <div>
            <strong>20+</strong>
            <span>Konditerem</span>
          </div>

          <div>
            <strong>Google</strong>
            <span>térképes nézet</span>
          </div>

          <div>
            <strong>⭐</strong>
            <span>értékelések</span>
          </div>
        </div>
      </section>

      <GymsMap />
    </main>
  );
}
