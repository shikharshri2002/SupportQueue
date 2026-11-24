import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { DashboardStats, Ticket, TicketFilters, TicketPriority, TicketStatus, UserSummary } from './models';

const apiUrl = 'http://localhost:5172/api';

@Injectable({ providedIn: 'root' })
export class TicketService {
  private readonly http = inject(HttpClient);

  getTickets(filters: TicketFilters): Observable<Ticket[]> {
    let params = new HttpParams();
    if (filters.search.trim())
      params = params.set('search', filters.search.trim());
    if (filters.status)
      params = params.set('status', filters.status);
    if (filters.priority)
      params = params.set('priority', filters.priority);
    return this.http.get<Ticket[]>(`${apiUrl}/tickets`, { params });
  }

  getStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${apiUrl}/tickets/stats`);
  }

  getAgents(): Observable<UserSummary[]> {
    return this.http.get<UserSummary[]>(`${apiUrl}/team/agents`);
  }

  createTicket(input: {
    subject: string;
    description: string;
    category: string;
    priority: TicketPriority;
  }): Observable<Ticket> {
    return this.http.post<Ticket>(`${apiUrl}/tickets`, input);
  }

  updateStatus(id: number, status: TicketStatus): Observable<void> {
    return this.http.patch<void>(`${apiUrl}/tickets/${id}/status`, { status });
  }

  assign(id: number, assigneeId: string): Observable<void> {
    return this.http.patch<void>(`${apiUrl}/tickets/${id}/assignment`, { assigneeId });
  }
}
