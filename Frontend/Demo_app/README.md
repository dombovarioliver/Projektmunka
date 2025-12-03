# Demo App

Egy egyszerű Flutter alkalmazás, amelyen keresztül az elkészült funkciók API-n keresztüli bemutatása megoldható. A képernyő listázza a rendelkezésre álló végpontokat, lekérdezi az állapotukat, és gombbal lehet akciókat indítani.

## Követelmények
- Flutter 3.16+ SDK
- Android/iOS/Web futtatókörnyezet
- Futtatás előtt a backend API-nak elérhetőnek kell lennie (Docker Compose vagy saját futtatás)

## Backend indítása (Docker Compose)
1. Lépj a repository gyökerébe: `cd /workspace/Projektmunka`.
2. Indítsd el a compose stacket (SQL + ML + API):
   ```
   docker compose up -d api
   ```
   A parancs felhúzza a függő szolgáltatásokat is, és az API-t a `http://localhost:8080` címen teszi elérhetővé.

## Frontend Docker image (webes demo)
1. **Build** (alapértelmezett API cím: `http://localhost:8080/api`; módosítsd `API_BASE_URL`-t ha kell):
   ```
   docker compose build demo-web
   ```
2. **Futtatás**: ekkor a Flutter web build az nginxből szolgál ki a `http://localhost:8081` címen.
   ```
   docker compose up -d demo-web
   ```
3. **Leállítás**:
   ```
   docker compose stop demo-web
   ```

## Futatás (frontend)
1. Telepítsd a függőségeket:
   ```
   flutter pub get
   ```
2. Indítsd az alkalmazást (webes példa):
   ```
   flutter run -d chrome
   ```
   Mobilhoz használj csatlakoztatott eszközt vagy emulátort (pl. `flutter devices` → `flutter run -d <deviceId>`).

### Android (emulátor / eszköz)
- **Base URL**: a kód alapból Android emulátorra optimalizált (`http://10.0.2.2:8080/api`).
- **Futás**: indítsd az Android emulátort (Android Studio), majd:
  ```
  flutter run -d emulator-5554
  ```
- **Saját IP eszközön**: ha fizikai eszközről éred el a host gépet, add meg a host IP-t build időben:
  ```
  flutter run \
    --dart-define=API_BASE_URL=http://<HOST_IP>:8080/api \
    -d <deviceId>
  ```

## Konfiguráció
- A `lib/services/api_client.dart` fájlban a `baseUrl` értéket állítsd a backend szerver URL-jére (Docker Compose esetén alapértelmezetten `http://10.0.2.2:8080/api` mobilon, Weben `http://localhost:8080/api`).
- Build időben felülírhatod: `--dart-define=API_BASE_URL=http://<cim>/api`.

## Szükséges importok
- Alkalmazás- és widget-szint: `package:flutter/material.dart` (`lib/main.dart`, `lib/screens/home_screen.dart`, `lib/widgets/feature_card.dart`).
- HTTP hívások és JSON: `package:http/http.dart` és `dart:convert` (`lib/services/api_client.dart`).
- Belső modellek/komponensek: a saját fájlok relatív importjai (`../models/feature_action.dart`, `../services/api_client.dart`, `../widgets/feature_card.dart`).

## Felépítés
- `lib/main.dart`: belépési pont, téma és routing
- `lib/screens/home_screen.dart`: fő képernyő, kártyák a funkciókhoz
- `lib/services/api_client.dart`: egyszerű HTTP kliens réteg
- `lib/models/feature_action.dart`: modell a végpontok metaadataihoz
- `lib/widgets/feature_card.dart`: UI komponens az egyes funkciók megjelenítéséhez