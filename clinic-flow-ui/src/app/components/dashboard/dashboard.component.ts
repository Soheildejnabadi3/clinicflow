import { Component, OnInit, PLATFORM_ID, inject, signal } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AppointmentService } from '../../services/appointment.service';
import { DashboardStats } from '../../models/appointment';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent implements OnInit {
  private readonly appointments = inject(AppointmentService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  readonly stats = signal<DashboardStats | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  ngOnInit(): void {
    // Skip during server render/prerender: the API is not necessarily reachable
    // at build time, and the client refetches on hydration anyway.
    if (!this.isBrowser) {
      this.loading.set(false);
      return;
    }

    this.appointments.getDashboard().subscribe({
      next: stats => {
        this.stats.set(stats);
        this.loading.set(false);
      },
      error: () => {
        this.error.set('Could not reach the ClinicFlow API. Is it running on port 5057?');
        this.loading.set(false);
      }
    });
  }

  statusClass(status: string): string {
    return status.toLowerCase();
  }
}
