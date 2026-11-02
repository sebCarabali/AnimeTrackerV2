import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { environment } from '../environments/environment';
import { provideApiConfiguration } from './core/api/generated/api-configuration';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // withInterceptorsFromDi() deja la puerta abierta a interceptores basados en DI,
    // como el de correlación que añadirá HU-00.11.
    provideHttpClient(withInterceptorsFromDi()),
    provideApiConfiguration(environment.apiBaseUrl),
  ],
};
