ARG FLUTTER_VERSION=3.7.12
ARG API_BASE_URL=http://localhost:8080/api

FROM ghcr.io/cirruslabs/flutter:latest AS build
ARG API_BASE_URL

WORKDIR /app

COPY pubspec.yaml pubspec.lock* ./
RUN flutter pub get

COPY . .

RUN flutter build web \
  --release \
  --dart-define=API_BASE_URL=${API_BASE_URL}

FROM nginx:1.25-alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/build/web .
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]