import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api.config';
import { CreatePatientDto, Patient, UpdatePatientDto } from '../models/patient';

@Injectable({
  providedIn: 'root'  // Singleton service (like React context)
})
export class PatientService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/patients`;

  getAll(search?: string): Observable<Patient[]> {
    const params = search ? new HttpParams().set('search', search) : undefined;
    return this.http.get<Patient[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Patient> {
    return this.http.get<Patient>(`${this.apiUrl}/${id}`);
  }

  create(patient: CreatePatientDto): Observable<Patient> {
    return this.http.post<Patient>(this.apiUrl, patient);
  }

  update(id: number, patient: UpdatePatientDto): Observable<Patient> {
    return this.http.put<Patient>(`${this.apiUrl}/${id}`, patient);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
