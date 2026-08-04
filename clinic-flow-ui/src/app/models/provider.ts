/** Mirrors ClinicFlow.Api AvailabilitySlotDto. */
export interface AvailabilitySlot {
  id: number;
  providerId: number;
  dayOfWeek: string;
  /** "HH:mm" */
  startTime: string;
  /** "HH:mm" */
  endTime: string;
  isActive: boolean;
}

/** Mirrors ClinicFlow.Api ProviderDto. */
export interface Provider {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  specialty: string;
  email: string;
  totalAppointments: number;
  availabilitySlots: AvailabilitySlot[];
}

export interface CreateProviderDto {
  firstName: string;
  lastName: string;
  specialty: string;
  email: string;
}

export interface CreateAvailabilitySlotDto {
  providerId: number;
  dayOfWeek: string;
  /** "HH:mm:ss" */
  startTime: string;
  endTime: string;
}
