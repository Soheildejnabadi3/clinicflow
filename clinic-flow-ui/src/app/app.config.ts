import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    // Required for the feature services to resolve HttpClient. withFetch() keeps
    // the server-side render from falling back to the XHR shim.
    provideHttpClient(withFetch()),
    provideClientHydration(withEventReplay())
  ]
};
