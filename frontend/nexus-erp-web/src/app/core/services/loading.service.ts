import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private readonly _loading = signal(false);
  private requestCount = 0;

  readonly isLoading = this._loading.asReadonly();

  show(): void {
    this.requestCount++;
    this._loading.set(true);
  }

  hide(): void {
    this.requestCount = Math.max(0, this.requestCount - 1);
    if (this.requestCount === 0) {
      this._loading.set(false);
    }
  }
}
