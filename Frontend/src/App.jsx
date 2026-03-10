import React, { useState } from 'react';
import { MapContainer, TileLayer, Polyline, Marker, Popup, useMapEvents } from 'react-leaflet';
import axios from 'axios';
import 'leaflet/dist/leaflet.css';
import './index.css';

// Leaflet'in varsayılan ikonlarının React'te görünmeme sorununu çözen standart ayar
import L from 'leaflet';
import icon from 'leaflet/dist/images/marker-icon.png';
import iconShadow from 'leaflet/dist/images/marker-shadow.png';
let DefaultIcon = L.icon({
    iconUrl: icon,
    shadowUrl: iconShadow,
    iconAnchor: [12, 41],
    popupAnchor: [1, -34],
});
L.Marker.prototype.options.icon = DefaultIcon;

// Haritaya tıklama olaylarını (A ve B noktası seçimi) dinleyen alt bileşen
function LocationSelector({ startCoord, setStartCoord, endCoord, setEndCoord }) {
  useMapEvents({
    click(e) {
      if (!startCoord || (startCoord && endCoord)) {
        // Eğer hiç nokta yoksa veya her ikisi de seçiliyse (sıfırlayıp) A noktasını koy
        setStartCoord([e.latlng.lat, e.latlng.lng]);
        setEndCoord(null);
      } else if (!endCoord) {
        // A noktası var ama B yoksa, B noktasını koy
        setEndCoord([e.latlng.lat, e.latlng.lng]);
      }
    },
  });
  return null;
}

function App() {
  const centerPosition = [41.0390, 29.1771]; // Çekmeköy

  const [startCoord, setStartCoord] = useState(null);
  const [endCoord, setEndCoord] = useState(null);
  const [routePath, setRoutePath] = useState([]);
  const [metrics, setMetrics] = useState(null);
  const [isLoading, setIsLoading] = useState(false);
  const [mood, setMood] = useState('nature');

  const handleCalculateRoute = async () => {
    if (!startCoord || !endCoord) {
      alert("Lütfen önce haritaya tıklayarak Başlangıç (A) ve Bitiş (B) noktalarını seçin!");
      return;
    }

    setIsLoading(true);
    setMetrics(null);
    setRoutePath([]);

    try {
      const apiUrl = 'http://localhost:5149/api/Routes/calculate';
      
      // Dinamik olarak kullanıcının tıkladığı koordinatları gönderiyoruz
      const requestData = {
        startLat: startCoord[0],
        startLng: startCoord[1],
        endLat: endCoord[0],
        endLng: endCoord[1],
        mood: mood
      };

      const response = await axios.post(apiUrl, requestData);
      
      setRoutePath(response.data.routeCoordinates);
      setMetrics({
        distance: response.data.totalDistanceKm,
        time: response.data.estimatedTimeMinutes
      });
      
    } catch (error) {
      console.error("Hata:", error);
      alert("Hata: Overpass sunucusu yoğun veya iki nokta arasında yürünebilir bir yol bulunamadı. Lütfen noktaları birbirine biraz daha yakın seçip tekrar deneyin.");
      
      // İki nokta arasına API çökse bile düz bir çizgi çekerek test imkanı sunar
      setRoutePath([startCoord, endCoord]); 
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div style={{ position: 'relative', width: '100%', height: '100vh' }}>
      
      {/* SOL KONTROL PANELİ */}
      <div style={{ position: 'absolute', top: 20, left: 20, zIndex: 1000, background: 'white', padding: '20px', borderRadius: '12px', boxShadow: '0 4px 15px rgba(0,0,0,0.1)', width: '320px' }}>
        <h1 style={{ margin: '0 0 5px 0', color: '#2E7D32', fontSize: '24px' }}>WalkMood</h1>
        <p style={{ margin: '0 0 15px 0', fontSize: '13px', color: '#666' }}>Doğa ve Güvenli Rota Planlayıcısı</p>
        
        {/* Bilgilendirme Kutusu */}
        <div style={{ background: '#fff3cd', padding: '10px', borderRadius: '8px', marginBottom: '15px', fontSize: '12px', color: '#856404', border: '1px solid #ffeeba' }}>
          <strong>Nasıl Kullanılır?</strong><br/>
          Haritaya tıklayarak önce Başlangıç, sonra Bitiş noktasını seçin.
        </div>

        {/* Mod Seçimi Butonları */}
        <div style={{ marginBottom: '15px' }}>
            <p style={{ fontSize: '14px', fontWeight: 'bold', marginBottom: '8px', color: '#333' }}>Yürüyüş Modu:</p>
            <div style={{ display: 'flex', gap: '10px' }}>
                <button 
                    onClick={() => setMood('nature')}
                    style={{ flex: 1, padding: '10px', borderRadius: '8px', border: mood === 'nature' ? '2px solid #2E7D32' : '1px solid #ddd', background: mood === 'nature' ? '#e8f5e9' : 'white', cursor: 'pointer', fontWeight: 'bold', color: '#333' }}>
                    🌳 Doğa
                </button>
                <button 
                    onClick={() => setMood('safe')}
                    style={{ flex: 1, padding: '10px', borderRadius: '8px', border: mood === 'safe' ? '2px solid #1976D2' : '1px solid #ddd', background: mood === 'safe' ? '#e3f2fd' : 'white', cursor: 'pointer', fontWeight: 'bold', color: '#333' }}>
                    🛡️ Güvenli
                </button>
            </div>
        </div>

        {/* Hesapla Butonu */}
        <button 
            onClick={handleCalculateRoute} 
            disabled={isLoading || !startCoord || !endCoord}
            style={{ width: '100%', padding: '12px', background: (!startCoord || !endCoord) ? '#ccc' : '#333', color: 'white', border: 'none', borderRadius: '8px', cursor: (!startCoord || !endCoord || isLoading) ? 'not-allowed' : 'pointer', fontWeight: 'bold' }}>
            {isLoading ? 'Analiz Ediliyor...' : 'Rotayı Çiz'}
        </button>

        {/* Metrikler */}
        {metrics && (
            <div style={{ marginTop: '20px', padding: '15px', background: '#f8f9fa', borderRadius: '8px', border: '1px solid #eee' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '10px' }}>
                    <span style={{ fontSize: '14px', color: '#666' }}>Mesafe:</span>
                    <span style={{ fontWeight: 'bold', color: '#333' }}>{metrics.distance} km</span>
                </div>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                    <span style={{ fontSize: '14px', color: '#666' }}>Tahmini Süre:</span>
                    <span style={{ fontWeight: 'bold', color: '#333' }}>{metrics.time} Dakika</span>
                </div>
            </div>
        )}
      </div>

      {/* HARİTA ALANI */}
      <MapContainer center={centerPosition} zoom={14} scrollWheelZoom={true}>
        <TileLayer attribution='&copy; OpenStreetMap' url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png" />
        
        {/* Tıklama Dinleyicisi */}
        <LocationSelector startCoord={startCoord} setStartCoord={setStartCoord} endCoord={endCoord} setEndCoord={setEndCoord} />

        {/* Başlangıç Noktası (A) */}
        {startCoord && (
          <Marker position={startCoord}>
            <Popup>Başlangıç (A)</Popup>
          </Marker>
        )}

        {/* Bitiş Noktası (B) */}
        {endCoord && (
          <Marker position={endCoord}>
            <Popup>Bitiş (B)</Popup>
          </Marker>
        )}

        {/* Rota Çizgisi */}
        {routePath.length > 0 && (
            <Polyline positions={routePath} color={mood === 'nature' ? '#2E7D32' : '#1976D2'} weight={6} opacity={0.8} />
        )}
      </MapContainer>

    </div>
  );
}

export default App;