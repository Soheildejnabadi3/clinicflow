import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { PatientService } from './patient.service';
import { API_BASE_URL } from '../core/api.config';

describe('PatientService', () => {
  let service: PatientService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(PatientService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('requests the patients collection', () => {
    service.getAll().subscribe();
    const req = httpMock.expectOne(`${API_BASE_URL}/patients`);
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('passes the search term as a query parameter', () => {
    service.getAll('chen').subscribe();
    const req = httpMock.expectOne(`${API_BASE_URL}/patients?search=chen`);
    expect(req.request.params.get('search')).toBe('chen');
    req.flush([]);
  });
});
