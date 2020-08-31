FROM node:8.11.2-slim as builder

WORKDIR /opt/map_component

# Install hard deps
RUN set -ex \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        libcairo2-dev \
        libjpeg-dev \
        libpango1.0-dev \
        libgif-dev \
        build-essential \
        g++ \
        git \
    && rm -rf /var/lib/apt/lists/*

# Install node deps
COPY package*.json ./
RUN npm install

# Copy files
COPY . .

RUN npm run build



FROM node:8.11.2-slim as prod

# Metadata
LABEL \
    version="1.0" \
    author="SmartGeoSystem" \
    maintainer="Nikulin Evgeniy" \
    description="OSM Stack" \
    source="https://gitlab.com/gis4sphera/osm/gis4sphera_osm_view_creator.git"


WORKDIR /opt/map_component

RUN set -ex \
    && apt-get update \
    && apt-get install -y --no-install-recommends \
        git \
    && rm -rf /var/lib/apt/lists/*

# Install node deps
COPY package*.json ./
RUN npm install --only=production

# Copy files
COPY . .
COPY --from=builder /opt/map_component/build /opt/map_component/build

# Startup params
ENV BACKEND_ADDR http://localhost:8081
EXPOSE 3000
CMD  "/opt/map_component/start.sh"

