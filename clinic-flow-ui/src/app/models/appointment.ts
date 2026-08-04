export type AppointmentStatus =
  | 'Scheduled'
  | 'Confirmed'
  | 'InProgress'
  | 'Completed'
  | 'Cancelled'
  | 'NoShow';

/** Mirrors ClinicFlow.Api AppointmentDto. */
export interface Appointment {
  id: number;
  patientId: number;
  patientName: string;
  providerId: number;
  providerName: string;
  providerSpecialty: string;
  startTime: string;
  endTime: string;
  status: AppointmentStatus;
  notes: string | null;
  createdAt: string;
}

export interface CreateAppointmentDto {
  patientId: number;
  providerId: number;
  /** ISO instant, e.g. 2026-08-05T14:00:00Z. */
  startTime: string;
  endTime: string;
  notes?: string | null;
}

export interface UpdateAppointmentDto {
  startTime?: string;
  endTime?: string;
  status?: AppointmentStatus;
  notes?: string | null;
}

/** Mirrors ClinicFlow.Api DashboardStatsDto. */
export interface DashboardStats {
  totalPatients: number;
  totalProviders: number;
  todayAppointments: number;
  upcomingAppointments: number;
  cancelledThisWeek: number;
  completedThisWeek: number;
  nextFiveAppointments: Appointment[];
}
