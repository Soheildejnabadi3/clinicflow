import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { PatientFormComponent } from './patient-form.component';

describe('PatientFormComponent', () => {
  let component: PatientFormComponent;
  let fixture: ComponentFixture<PatientFormComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [PatientFormComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(PatientFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => {
    // The component fetches on init; drain those requests so verify() is meaningful.
    httpMock.match(() => true).forEach(req => req.flush([]));
    httpMock.verify();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
