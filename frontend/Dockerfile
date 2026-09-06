# ---- build ----
FROM node:22-alpine AS build
WORKDIR /src

# Instala dependências primeiro (camada cacheável enquanto o package.json não muda).
COPY package.json package-lock.json ./
RUN npm ci

# URL da API vista pelo NAVEGADOR (não pelo container). O Vite embute o valor no bundle
# em build time, por isso entra como build-arg e não como variável de runtime.
ARG VITE_API_URL=http://localhost:8080
ENV VITE_API_URL=$VITE_API_URL

COPY . .
RUN npm run build

# ---- runtime ----
FROM nginx:alpine AS final
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /src/dist /usr/share/nginx/html
EXPOSE 80
