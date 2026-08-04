/** Mirrors ClinicFlow.Api PatientDto. */
export interface Patient {
  id: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  phone: string;
  dateOfBirth: string;
  age: number;
  createdAt: string;
  totalAppointments: number;
}

export interface CreatePatientDto {
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  /** ISO date, e.g. 1988-06-04. */
  dateOfBirth: string;
}

export type UpdatePatientDto = Partial<CreatePatientDto>;
