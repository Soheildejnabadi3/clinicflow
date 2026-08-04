import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { ProviderScheduleComponent } from './provider-schedule.component';

describe('ProviderScheduleComponent', () => {
  let component: ProviderScheduleComponent;
  let fixture: ComponentFixture<ProviderScheduleComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProviderScheduleComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()]
    }).compileComponents();

    httpMock = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(ProviderScheduleComponent);
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
