import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { LoginResponse, UserSummary } from './models';

const apiUrl = 'http://localhost:5172/api';
const sessionKey = 'support-queue-session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  readonly session = signal<LoginResponse | null>(this.readSession());

  get accessToken(): string | null {
    return this.session()?.accessToken ?? null;
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${apiUrl}/auth/login`, { email, password }).pipe(
      tap(session => {
        sessionStorage.setItem(sessionKey, JSON.stringify(session));
        this.session.set(session);
      })
    );
  }

  logout(): void {
    sessionStorage.removeItem(sessionKey);
    this.session.set(null);
  }

  private readSession(): LoginResponse | null {
    const stored = sessionStorage.getItem(sessionKey);
    if (!stored)
      return null;

    try {
      const session = JSON.parse(stored) as LoginResponse;
      if (!session.accessToken || !session.user?.id || Date.parse(session.expiresAtUtc) <= Date.now()) {
        sessionStorage.removeItem(sessionKey);
        return null;
      }
      return session;
    } catch {
      sessionStorage.removeItem(sessionKey);
      return null;
    }
  }
}
