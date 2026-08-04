import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ProviderService } from './provider.service';
import { API_BASE_URL } from '../core/api.config';

describe('ProviderService', () => {
  let service: ProviderService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(ProviderService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('requests a provider availability list', () => {
    service.getAvailability(7).subscribe();
    const req = httpMock.expectOne(`${API_BASE_URL}/providers/7/availability`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
