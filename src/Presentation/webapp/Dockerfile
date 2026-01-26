FROM node:22-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build the Angular app
RUN npm run build

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built Angular app
COPY --from=build /app/dist/webapp/browser .

# Copy nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy custom entrypoint script and fix line endings (Windows CRLF -> Unix LF)
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN sed -i 's/\r$//' /docker-entrypoint.sh && chmod +x /docker-entrypoint.sh

EXPOSE 80

# Default API URL (will be replaced at runtime)
ENV API_URL=http://localhost:5000

ENTRYPOINT ["/docker-entrypoint.sh"]
