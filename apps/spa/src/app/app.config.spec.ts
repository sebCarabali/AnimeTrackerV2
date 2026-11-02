import { TestBed } from '@angular/core/testing';
import { ApiConfiguration } from './core/api/generated/api-configuration';
import { environment } from '../environments/environment';
import { appConfig } from './app.config';

describe('appConfig', () => {
  it('CA4: ApiConfiguration.rootUrl se provee desde environment.apiBaseUrl, no está fija en el código', () => {
    TestBed.configureTestingModule({
      providers: [...appConfig.providers],
    });

    const config = TestBed.inject(ApiConfiguration);

    expect(config.rootUrl).toBe(environment.apiBaseUrl);
  });
});
