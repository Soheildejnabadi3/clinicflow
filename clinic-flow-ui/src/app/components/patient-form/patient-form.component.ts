import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { PatientService } from '../../services/patient.service';

@Component({
  selector: 'app-patient-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  templateUrl: './patient-form.component.html',
  styleUrl: './patient-form.component.scss'
})
export class PatientFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly patients = inject(PatientService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  patientId: number | null = null;
  saving = false;
  error: string | null = null;

  readonly form = this.fb.nonNullable.group({
    firstName: ['', [Validators.required, Validators.maxLength(100)]],
    lastName: ['', [Validators.required, Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required]],
    dateOfBirth: ['', [Validators.required]]
  });

  get isEdit(): boolean {
    return this.patientId !== null;
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (!idParam) return;

    this.patientId = Number(idParam);
    this.patients.getById(this.patientId).subscribe({
      next: patient =>
        this.form.patchValue({
          firstName: patient.firstName,
          lastName: patient.lastName,
          email: patient.email,
          phone: patient.phone,
          // <input type="date"> needs a bare yyyy-MM-dd value.
          dateOfBirth: patient.dateOfBirth.substring(0, 10)
        }),
      error: () => (this.error = 'Could not load this patient.')
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = null;
    const value = this.form.getRawValue();

    const request$ = this.isEdit
      ? this.patients.update(this.patientId!, value)
      : this.patients.create(value);

    request$.subscribe({
      next: () => this.router.navigate(['/patients']),
      error: err => {
        // ExceptionHandlingMiddleware returns { error, statusCode }.
        this.error = err?.error?.error ?? 'Could not save this patient.';
        this.saving = false;
      }
    });
  }

  invalid(control: string): boolean {
    const c = this.form.get(control);
    return !!c && c.invalid && c.touched;
  }
}
