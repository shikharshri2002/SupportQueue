import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { DatePipe, DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { DashboardStats, Ticket, TicketFilters, TicketPriority, TicketStatus, UserSummary } from './models';
import { TicketService } from './ticket.service';

type DashboardView = 'overview' | 'tickets' | 'team';

@Component({
  selector: 'app-dashboard',
  imports: [DatePipe, DecimalPipe, FormsModule, ReactiveFormsModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly ticketsApi = inject(TicketService);
  private readonly formBuilder = inject(FormBuilder);
  private readonly router = inject(Router);

  readonly statuses: TicketStatus[] = ['Open', 'InProgress', 'Resolved'];
  readonly priorities: TicketPriority[] = ['Low', 'Medium', 'High', 'Urgent'];
  readonly categories = ['Access', 'Hardware', 'Software', 'Operations'];
  readonly filters: TicketFilters = { search: '', status: '', priority: '' };
  readonly session = this.auth.session;
  readonly requestForm = this.formBuilder.nonNullable.group({
    subject: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(120)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(3000)]],
    category: ['Access', Validators.required],
    priority: ['Medium' as TicketPriority, Validators.required]
  });

  view: DashboardView = 'overview';
  tickets: Ticket[] = [];
  stats: DashboardStats | null = null;
  agents: UserSummary[] = [];
  loadingTickets = true;
  creating = false;
  showRequestForm = false;
  expandedTicketId: number | null = null;
  errorMessage = '';
  notice = '';

  get user(): UserSummary | null {
    return this.session()?.user ?? null;
  }

  get isEmployee(): boolean {
    return this.user?.role === 'Employee';
  }

  get canManageTickets(): boolean {
    return this.user?.role === 'Admin' || this.user?.role === 'SupportAgent';
  }

  get title(): string {
    if (this.view === 'team')
      return 'Team workload';
    if (this.view === 'tickets')
      return this.isEmployee ? 'My requests' : 'All tickets';
    return 'Overview';
  }

  get subtitle(): string {
    if (this.view === 'team')
      return 'Review, filter, and take action on incoming requests.';
    if (this.isEmployee)
      return 'Track the support requests you have submitted.';
    return "Here's what's happening across your support queue today.";
  }

  get resolutionPercent(): number {
    return Math.round(this.stats?.resolutionPercent ?? 0);
  }

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loadTickets();
    this.ticketsApi.getStats().subscribe({
      next: stats => this.stats = stats,
      error: error => this.showApiError(error)
    });
    if (this.canManageTickets) {
      this.ticketsApi.getAgents().subscribe({
        next: agents => this.agents = agents,
        error: error => this.showApiError(error)
      });
    }
  }

  loadTickets(): void {
    this.loadingTickets = true;
    this.ticketsApi.getTickets(this.filters).subscribe({
      next: results => {
        this.tickets = results;
        this.loadingTickets = false;
        this.errorMessage = '';
      },
      error: error => {
        this.loadingTickets = false;
        this.showApiError(error);
      }
    });
  }

  chooseView(view: DashboardView): void {
    this.view = view;
    this.notice = '';
  }

  openRequestForm(): void {
    this.notice = '';
    this.showRequestForm = true;
  }

  closeRequestForm(): void {
    this.showRequestForm = false;
    this.requestForm.reset({ subject: '', description: '', category: 'Access', priority: 'Medium' });
  }

  submitRequest(): void {
    if (this.requestForm.invalid) {
      this.requestForm.markAllAsTouched();
      return;
    }
    this.creating = true;
    this.ticketsApi.createTicket(this.requestForm.getRawValue()).subscribe({
      next: ticket => {
        this.creating = false;
        this.closeRequestForm();
        this.notice = `Request ${ticket.ticketNumber} was submitted.`;
        this.loadDashboard();
      },
      error: error => {
        this.creating = false;
        this.showApiError(error);
      }
    });
  }

  changeStatus(ticket: Ticket, value: string): void {
    if (!this.statuses.includes(value as TicketStatus))
      return;
    this.ticketsApi.updateStatus(ticket.id, value as TicketStatus).subscribe({
      next: () => {
        this.notice = `${ticket.ticketNumber} is now ${this.statusLabel(value)}.`;
        this.loadDashboard();
      },
      error: error => this.showApiError(error)
    });
  }

  changeAssignment(ticket: Ticket, assigneeId: string): void {
    if (!assigneeId || assigneeId === ticket.assigneeId)
      return;
    this.ticketsApi.assign(ticket.id, assigneeId).subscribe({
      next: () => {
        this.notice = `${ticket.ticketNumber} was assigned to the selected agent.`;
        this.loadDashboard();
      },
      error: error => this.showApiError(error)
    });
  }

  toggleDetails(id: number): void {
    this.expandedTicketId = this.expandedTicketId === id ? null : id;
  }

  activeTicketsFor(agentId: string): number {
    return this.tickets.filter(ticket =>
      ticket.assigneeId === agentId && ticket.status !== 'Resolved').length;
  }

  statusLabel(status: string): string {
    return status === 'InProgress' ? 'In progress' : status;
  }

  initials(name: string | undefined): string {
    return name?.split(' ').map(part => part[0]).slice(0, 2).join('').toUpperCase() ?? 'SQ';
  }

  logout(): void {
    this.auth.logout();
    void this.router.navigate(['/login']);
  }

  statusChanged(ticket: Ticket, event: Event): void {
    this.changeStatus(ticket, (event.target as HTMLSelectElement).value);
  }

  assignmentChanged(ticket: Ticket, event: Event): void {
    this.changeAssignment(ticket, (event.target as HTMLSelectElement).value);
  }

  private showApiError(error: unknown): void {
    const response = error instanceof HttpErrorResponse ? error : null;
    this.errorMessage = response?.status === 0
      ? 'The API is unavailable. Start the ASP.NET Core API and SQL Server, then refresh.'
      : response?.error?.message ?? 'The request could not be completed. Please try again.';
  }
}
