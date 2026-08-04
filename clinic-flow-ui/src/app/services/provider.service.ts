import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../core/api.config';
import {
  AvailabilitySlot,
  CreateAvailabilitySlotDto,
  CreateProviderDto,
  Provider
} from '../models/provider';

@Injectable({ providedIn: 'root' })
export class ProviderService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/providers`;

  getAll(specialty?: string): Observable<Provider[]> {
    const params = specialty ? new HttpParams().set('specialty', specialty) : undefined;
    return this.http.get<Provider[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<Provider> {
    return this.http.get<Provider>(`${this.apiUrl}/${id}`);
  }

  create(provider: CreateProviderDto): Observable<Provider> {
    return this.http.post<Provider>(this.apiUrl, provider);
  }

  update(id: number, provider: Partial<CreateProviderDto>): Observable<Provider> {
    return this.http.put<Provider>(`${this.apiUrl}/${id}`, provider);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getAvailability(providerId: number): Observable<AvailabilitySlot[]> {
    return this.http.get<AvailabilitySlot[]>(`${this.apiUrl}/${providerId}/availability`);
  }

  addAvailability(
    providerId: number,
    slot: CreateAvailabilitySlotDto
  ): Observable<AvailabilitySlot> {
    return this.http.post<AvailabilitySlot>(
      `${this.apiUrl}/${providerId}/availability`,
      slot
    );
  }

  removeAvailability(slotId: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/availability/${slotId}`);
  }

  isAvailable(
    providerId: number,
    startTime: string,
    endTime: string
  ): Observable<{ available: boolean }> {
    const params = new HttpParams().set('startTime', startTime).set('endTime', endTime);
    return this.http.get<{ available: boolean }>(
      `${this.apiUrl}/${providerId}/available`,
      { params }
    );
  }
}
