import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api.config';
import {
  Appointment,
  CreateAppointmentDto,
  DashboardStats,
  UpdateAppointmentDto
} from '../models/appointment';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/appointments`;

  getAll(fromDate?: string, toDate?: string): Observable<Appointment[]> {
    let params = new HttpParams();
    if (fromDate) params = params.set('fromDate', fromDate);
    if (toDate) params = params.set('toDate', toDate);
    return this.http.get<Appointment[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Appointment> {
    return this.http.get<Appointment>(`${this.apiUrl}/${id}`);
  }

  getByPatient(patientId: number): Observable<Appointment[]> {
    return this.http.get<Appointment[]>(`${this.apiUrl}/patient/${patientId}`);
  }

  getByProvider(providerId: number, date?: string): Observable<Appointment[]> {
    const params = date ? new HttpParams().set('date', date) : undefined;
    return this.http.get<Appointment[]>(`${this.apiUrl}/provider/${providerId}`, { params });
  }

  getDashboard(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }

  create(appointment: CreateAppointmentDto): Observable<Appointment> {
    return this.http.post<Appointment>(this.apiUrl, appointment);
  }

  update(id: number, appointment: UpdateAppointmentDto): Observable<Appointment> {
    return this.http.put<Appointment>(`${this.apiUrl}/${id}`, appointment);
  }

  cancel(id: number, reason: string): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.apiUrl}/${id}/cancel`, { reason });
  }

  complete(id: number): Observable<Appointment> {
    return this.http.post<Appointment>(`${this.apiUrl}/${id}/complete`, {});
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
