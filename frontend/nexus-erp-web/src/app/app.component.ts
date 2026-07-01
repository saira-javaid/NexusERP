import { Component, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { ThemeService } from './core/services/theme.service';
import { UserPreferencesService } from './core/services/user-preferences.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: `<router-outlet />`,
})
export class AppComponent implements OnInit {
  private readonly themeService = inject(ThemeService);
  private readonly preferences = inject(UserPreferencesService);

  ngOnInit(): void {
    this.themeService.init();
    const prefs = this.preferences.preferences();
    this.themeService.setTheme(prefs.darkMode ? 'dark' : 'light');
  }
}
