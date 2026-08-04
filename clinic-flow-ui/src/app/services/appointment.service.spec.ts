import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AppointmentService } from './appointment.service';
import { API_BASE_URL } from '../core/api.config';

describe('AppointmentService', () => {
  let service: AppointmentService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    service = TestBed.inject(AppointmentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('requests the dashboard stats', () => {
    service.getDashboard().subscribe();
    const req = httpMock.expectOne(`${API_BASE_URL}/appointments/dashboard`);
    expect(req.request.method).toBe('GET');
    req.flush({});
  });

  it('posts a cancellation reason', () => {
    service.cancel(3, 'Patient rescheduled').subscribe();
    const req = httpMock.expectOne(`${API_BASE_URL}/appointments/3/cancel`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ reason: 'Patient rescheduled' });
    req.flush({});
  });
});
