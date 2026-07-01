import { Injectable, signal, effect } from '@angular/core';

export type ThemeMode = 'light' | 'dark';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly STORAGE_KEY = 'nexus_theme';
  readonly mode = signal<ThemeMode>(this.loadTheme());

  constructor() {
    effect(() => this.applyTheme(this.mode()));
  }

  init(): void {
    this.applyTheme(this.mode());
  }

  toggle(): void {
    this.mode.update(m => m === 'light' ? 'dark' : 'light');
    localStorage.setItem(this.STORAGE_KEY, this.mode());
  }

  setTheme(mode: ThemeMode): void {
    this.mode.set(mode);
    localStorage.setItem(this.STORAGE_KEY, mode);
  }

  private loadTheme(): ThemeMode {
    return (localStorage.getItem(this.STORAGE_KEY) as ThemeMode) ?? 'light';
  }

  private applyTheme(mode: ThemeMode): void {
    const body = document.body;
    body.classList.remove('light-theme', 'dark-theme');
    body.classList.add(mode === 'dark' ? 'dark-theme' : 'light-theme');
  }
}
