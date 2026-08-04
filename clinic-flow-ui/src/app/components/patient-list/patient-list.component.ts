import { Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PatientService } from '../../services/patient.service';
import { Patient } from '../../models/patient';

@Component({
  selector: 'app-patient-list',
  standalone: true,
  // ngModel needs FormsModule, and routerLink needs RouterLink. Without these a
  // standalone component silently renders the attributes as plain HTML.
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './patient-list.component.html',
  styleUrls: ['./patient-list.component.scss']
})
export class PatientListComponent implements OnInit {
  private readonly patientService = inject(PatientService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  patients: Patient[] = [];
  loading = true;
  error: string | null = null;
  searchTerm = '';

  // Like useEffect(() => { ... }, []) in React
  ngOnInit(): void {
    if (!this.isBrowser) {
      this.loading = false;
      return;
    }
    this.loadPatients();
  }

  loadPatients(): void {
    this.loading = true;
    this.error = null;
    this.patientService.getAll().subscribe({
      next: (data) => {
        this.patients = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Failed to load patients', err);
        this.error = 'Could not load patients. Is the API running on port 5057?';
        this.loading = false;
      }
    });
  }

  get filteredPatients(): Patient[] {
    if (!this.searchTerm) return this.patients;
    const term = this.searchTerm.toLowerCase();
    return this.patients.filter(p =>
      p.firstName.toLowerCase().includes(term) ||
      p.lastName.toLowerCase().includes(term) ||
      p.email.toLowerCase().includes(term)
    );
  }

  deletePatient(patient: Patient): void {
    if (!confirm(`Delete ${patient.fullName}?`)) return;

    this.patientService.delete(patient.id).subscribe({
      next: () => {
        this.patients = this.patients.filter(p => p.id !== patient.id);
      },
      // The API returns 409 when the patient still has upcoming appointments;
      // surface that instead of failing silently.
      error: (err) => {
        this.error = err?.error?.error ?? 'Could not delete this patient.';
      }
    });
  }
}
