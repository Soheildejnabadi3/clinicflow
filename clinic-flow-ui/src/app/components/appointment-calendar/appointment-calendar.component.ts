import { Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { AppointmentService } from '../../services/appointment.service';
import { PatientService } from '../../services/patient.service';
import { ProviderService } from '../../services/provider.service';
import { Appointment } from '../../models/appointment';
import { Patient } from '../../models/patient';
import { Provider } from '../../models/provider';

@Component({
  selector: 'app-appointment-calendar',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './appointment-calendar.component.html',
  styleUrl: './appointment-calendar.component.scss'
})
export class AppointmentCalendarComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly appointments = inject(AppointmentService);
  private readonly patients = inject(PatientService);
  private readonly providers = inject(ProviderService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  list: Appointment[] = [];
  patientOptions: Patient[] = [];
  providerOptions: Provider[] = [];
  loading = true;
  saving = false;
  error: string | null = null;
  showForm = false;

  readonly form = this.fb.nonNullable.group({
    patientId: [0, [Validators.required, Validators.min(1)]],
    providerId: [0, [Validators.required, Validators.min(1)]],
    date: ['', Validators.required],
    startTime: ['09:00', Validators.required],
    endTime: ['09:30', Validators.required],
    notes: ['']
  });

  ngOnInit(): void {
    if (!this.isBrowser) {
      this.loading = false;
      return;
    }
    this.load();
  }

  load(): void {
    this.loading = true;
    forkJoin({
      appointments: this.appointments.getAll(),
      patients: this.patients.getAll(),
      providers: this.providers.getAll()
    }).subscribe({
      next: ({ appointments, patients, providers }) => {
        this.list = appointments;
        this.patientOptions = patients;
        this.providerOptions = providers;
        this.loading = false;
      },
      error: () => {
        this.error = 'Could not load appointments. Is the API running on port 5057?';
        this.loading = false;
      }
    });
  }

  book(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const v = this.form.getRawValue();
    this.saving = true;
    this.error = null;

    // The API stores instants in UTC; build one from the local date + time inputs.
    this.appointments
      .create({
        patientId: Number(v.patientId),
        providerId: Number(v.providerId),
        startTime: new Date(`${v.date}T${v.startTime}`).toISOString(),
        endTime: new Date(`${v.date}T${v.endTime}`).toISOString(),
        notes: v.notes || null
      })
      .subscribe({
        next: () => {
          this.saving = false;
          this.showForm = false;
          this.form.patchValue({ notes: '' });
          this.load();
        },
        error: err => {
          this.error = err?.error?.error ?? 'Could not book this appointment.';
          this.saving = false;
        }
      });
  }

  cancel(appointment: Appointment): void {
    const reason = prompt(`Why is this appointment being cancelled?`);
    if (reason === null) return;

    this.appointments.cancel(appointment.id, reason || 'No reason given').subscribe({
      next: updated => this.replace(updated),
      error: err => (this.error = err?.error?.error ?? 'Could not cancel.')
    });
  }

  complete(appointment: Appointment): void {
    this.appointments.complete(appointment.id).subscribe({
      next: updated => this.replace(updated),
      error: err => (this.error = err?.error?.error ?? 'Could not complete.')
    });
  }

  isOpen(appointment: Appointment): boolean {
    return appointment.status !== 'Cancelled' && appointment.status !== 'Completed';
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }

  private replace(updated: Appointment): void {
    this.list = this.list.map(a => (a.id === updated.id ? updated : a));
  }
}
