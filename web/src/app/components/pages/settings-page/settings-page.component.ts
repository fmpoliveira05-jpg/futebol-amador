import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './settings-page.component.html',
  styleUrl: './settings-page.component.css',
})
export class SettingsPageComponent {
  protected readonly darkMode = signal<boolean>(
    window.matchMedia &&
      window.matchMedia('(prefers-color-scheme: dark)').matches,
  );
  protected readonly notificationsEnabled = signal<boolean>(true);
  protected readonly language = signal<'pt' | 'en'>('pt');

  protected toggleDarkMode(): void {
    this.darkMode.set(!this.darkMode());
  }

  protected toggleNotifications(): void {
    this.notificationsEnabled.set(!this.notificationsEnabled());
  }

  protected setLanguage(lang: 'pt' | 'en'): void {
    this.language.set(lang);
  }
}