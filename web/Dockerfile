FROM node:22-alpine AS build
WORKDIR /web

COPY package.json package-lock.json ./
RUN npm ci

ARG VITE_API_URL=http://localhost:5168
ENV VITE_API_URL=$VITE_API_URL

COPY . .
RUN npm run build

FROM nginx:alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /web/dist /usr/share/nginx/html
EXPOSE 80
