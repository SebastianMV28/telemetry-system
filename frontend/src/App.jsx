import { useEffect, useState } from "react";
import { MapContainer, TileLayer, Marker, Popup } from "react-leaflet";
import "./App.css";

function App() {
    const [vehicles, setVehicles] = useState([]);
    const [lastUpdate, setLastUpdate] = useState(null);
    const [error, setError] = useState(null);

    const fetchVehicles = async () => {
        try {
            const response = await fetch("http://localhost:5149/vehicles");

            if (!response.ok) {
                throw new Error("Error fetching vehicles");
            }

            const data = await response.json();

            setVehicles(data);
            setLastUpdate(new Date());
            setError(null);
        } catch (err) {
            console.error(err);
            setError("No se pudo conectar con el backend");
        }
    };

    useEffect(() => {
        fetchVehicles();

        const intervalId = setInterval(() => {
            fetchVehicles();
        }, 5000);

        return () => clearInterval(intervalId);
    }, []);

    const getStatusClass = (status) => {
        if (status === "En movimiento") return "status moving";
        if (status === "Detenido") return "status stopped";
        if (status === "Sin señal") return "status offline";

        return "status";
    };

    return (
        <main className="container">
            <header className="header">
                <h1>Sistema de telemetría GPS</h1>
                <p>
                    Panel de control para el seguimiento en tiempo real de los
                    vehículos de la flota
                </p>

                {lastUpdate && (
                    <span className="last-update">
                        Última actualización: {lastUpdate.toLocaleTimeString()}
                    </span>
                )}
            </header>

            {error && <p className="error">{error}</p>}

            <section className="card">
                <h2>Vehículos monitoreados</h2>

                {vehicles.length === 0 ? (
                    <p>No hay vehículos registrados todavía.</p>
                ) : (
                    <table>
                        <thead>
                            <tr>
                                <th>Vehicle ID</th>
                                <th>Status</th>
                                <th>Last seen</th>
                                <th>Latitude</th>
                                <th>Longitude</th>
                            </tr>
                        </thead>

                        <tbody>
                            {vehicles.map((vehicle) => {
                                const vehicleId =
                                    vehicle.vehicleId || vehicle.vehicle_id;
                                const lastLat =
                                    vehicle.lastLat || vehicle.last_lat;
                                const lastLng =
                                    vehicle.lastLng || vehicle.last_lng;
                                const lastSeen =
                                    vehicle.lastSeen || vehicle.last_seen;

                                return (
                                    <tr key={vehicleId}>
                                        <td>{vehicleId}</td>
                                        <td>
                                            <span
                                                className={getStatusClass(
                                                    vehicle.status,
                                                )}
                                            >
                                                {vehicle.status}
                                            </span>
                                        </td>
                                        <td>
                                            {new Date(
                                                lastSeen,
                                            ).toLocaleString()}
                                        </td>
                                        <td>{lastLat?.toFixed(5)}</td>
                                        <td>{lastLng?.toFixed(5)}</td>
                                    </tr>
                                );
                            })}
                        </tbody>
                    </table>
                )}
            </section>

            <section className="card map-card">
                <h2>Mapa de vehículos</h2>

                <MapContainer center={[4.65, -74.1]} zoom={11} className="map">
                    <TileLayer
                        attribution="&copy; OpenStreetMap contributors"
                        url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
                    />

                    {vehicles.map((vehicle) => {
                        const vehicleId =
                            vehicle.vehicleId || vehicle.vehicle_id;
                        const lastLat = vehicle.lastLat || vehicle.last_lat;
                        const lastLng = vehicle.lastLng || vehicle.last_lng;

                        if (!lastLat || !lastLng) return null;

                        return (
                            <Marker
                                key={vehicleId}
                                position={[lastLat, lastLng]}
                            >
                                <Popup>
                                    <strong>{vehicleId}</strong>
                                    <br />
                                    Estado: {vehicle.status}
                                </Popup>
                            </Marker>
                        );
                    })}
                </MapContainer>
            </section>
        </main>
    );
}

export default App;
