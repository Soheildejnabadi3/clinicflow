import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  {
    path: 'dashboard',
    title: 'Dashboard | ClinicFlow',
    loadComponent: () =>
      import('./components/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'patients',
    title: 'Patients | ClinicFlow',
    loadComponent: () =>
      import('./components/patient-list/patient-list.component').then(
        m => m.PatientListComponent
      )
  },
  {
    path: 'patients/new',
    title: 'New patient | ClinicFlow',
    loadComponent: () =>
      import('./components/patient-form/patient-form.component').then(
        m => m.PatientFormComponent
      )
  },
  {
    path: 'patients/:id',
    title: 'Edit patient | ClinicFlow',
    loadComponent: () =>
      import('./components/patient-form/patient-form.component').then(
        m => m.PatientFormComponent
      )
  },
  {
    path: 'schedule',
    title: 'Provider schedule | ClinicFlow',
    loadComponent: () =>
      import('./components/provider-schedule/provider-schedule.component').then(
        m => m.ProviderScheduleComponent
      )
  },
  {
    path: 'calendar',
    title: 'Appointments | ClinicFlow',
    loadComponent: () =>
      import('./components/appointment-calendar/appointment-calendar.component').then(
        m => m.AppointmentCalendarComponent
      )
  },
  { path: '**', redirectTo: 'dashboard' }
];
