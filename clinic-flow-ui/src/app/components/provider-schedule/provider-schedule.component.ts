import { Component, OnInit, PLATFORM_ID, inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ProviderService } from '../../services/provider.service';
import { Provider } from '../../models/provider';

const DAY_ORDER = [
  'Monday',
  'Tuesday',
  'Wednesday',
  'Thursday',
  'Friday',
  'Saturday',
  'Sunday'
];

@Component({
  selector: 'app-provider-schedule',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './provider-schedule.component.html',
  styleUrl: './provider-schedule.component.scss'
})
export class ProviderScheduleComponent implements OnInit {
  private readonly providers = inject(ProviderService);
  private readonly isBrowser = isPlatformBrowser(inject(PLATFORM_ID));

  readonly days = DAY_ORDER;
  list: Provider[] = [];
  loading = true;
  error: string | null = null;

  ngOnInit(): void {
    if (!this.isBrowser) {
      this.loading = false;
      return;
    }

    this.providers.getAll().subscribe({
      next: providers => {
        this.list = providers;
        this.loading = false;
      },
      error: () => {
        this.error = 'Could not load providers. Is the API running on port 5057?';
        this.loading = false;
      }
    });
  }

  /** Slot label for a provider on a given weekday, or null when not working. */
  slotFor(provider: Provider, day: string): string | null {
    const slot = provider.availabilitySlots.find(s => s.dayOfWeek === day && s.isActive);
    return slot ? `${slot.startTime}–${slot.endTime}` : null;
  }
}
