import GymsMap from "../components/GymsMap";

export default function GymsMapPage() {
  return (
    <section>
      <div className="mb-4">
        <h1>Budapesti konditermek</h1>
        <p className="lead">
          Térképes nézet a Budapesten található edzőtermekkel.
        </p>
      </div>

      <GymsMap />
    </section>
  );
}