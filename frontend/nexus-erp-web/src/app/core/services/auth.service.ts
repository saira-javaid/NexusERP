import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, BehaviorSubject, filter, take, switchMap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse, RefreshTokenRequest, SignupRequest, User } from '../models/auth.model';

const TOKEN_KEY = 'nexus_access_token';
const REFRESH_KEY = 'nexus_refresh_token';
const USER_KEY = 'nexus_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _user = signal<User | null>(this.loadUser());
  private readonly _isAuthenticated = computed(() => !!this._user() && !!this.getAccessToken());
  private refreshInProgress = false;
  private refreshSubject = new BehaviorSubject<string | null>(null);

  readonly user = this._user.asReadonly();
  readonly isAuthenticated = this._isAuthenticated;

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap(response => this.setSession(response))
    );
  }

  signup(request: SignupRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/signup`, request).pipe(
      tap(response => this.setSession(response))
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    localStorage.removeItem(USER_KEY);
    this._user.set(null);
    this.router.navigate(['/auth/login']);
  }

  refreshToken(): Observable<LoginResponse> {
    const request: RefreshTokenRequest = {
      accessToken: this.getAccessToken() ?? '',
      refreshToken: this.getRefreshToken() ?? '',
    };

    if (this.refreshInProgress) {
      return this.refreshSubject.pipe(
        filter(token => token !== null),
        take(1),
        switchMap(() => throwError(() => new Error('Refresh handled')))
      );
    }

    this.refreshInProgress = true;

    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/refresh`, request).pipe(
      tap(response => {
        this.setSession(response);
        this.refreshInProgress = false;
        this.refreshSubject.next(response.accessToken);
      }),
      catchError(err => {
        this.refreshInProgress = false;
        this.logout();
        return throwError(() => err);
      })
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_KEY);
  }

  hasPermission(permission: string): boolean {
    const user = this._user();
    if (!user) return false;
    return user.permissions.includes(permission) || user.roles.includes('Admin');
  }

  hasRole(role: string): boolean {
    return this._user()?.roles.includes(role) ?? false;
  }

  private setSession(response: LoginResponse): void {
    localStorage.setItem(TOKEN_KEY, response.accessToken);
    localStorage.setItem(REFRESH_KEY, response.refreshToken);
    localStorage.setItem(USER_KEY, JSON.stringify(response.user));
    this._user.set(response.user);
  }

  private loadUser(): User | null {
    const stored = localStorage.getItem(USER_KEY);
    return stored ? JSON.parse(stored) : null;
  }
}
