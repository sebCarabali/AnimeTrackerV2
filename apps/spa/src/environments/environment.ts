export const environment = {
  production: true,
  // En producción el BFF se sirve en el mismo origen vía reverse proxy (Caddy/Traefik, ver ADR Q5).
  apiBaseUrl: '',
};
