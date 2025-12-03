# syntax=docker/dockerfile:1

ARG FLUTTER_VERSION=3.16.9
ARG API_BASE_URL=http://localhost:8080/api

FROM ghcr.io/cirruslabs/flutter:latest as build
WORKDIR /app

COPY pubspec.yaml pubspec.lock* ./
RUN flutter pub get

COPY . .
RUN flutter build web \
  --release \
  --dart-define=API_BASE_URL=${API_BASE_URL}

FROM nginx:1.25-alpine
COPY --from=build /app/build/web /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]