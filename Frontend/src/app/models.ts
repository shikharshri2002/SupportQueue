export type UserRole = 'Admin' | 'SupportAgent' | 'Employee';
export type TicketStatus = 'Open' | 'InProgress' | 'Resolved';
export type TicketPriority = 'Low' | 'Medium' | 'High' | 'Urgent';

export interface UserSummary {
  id: string;
  fullName: string;
  email: string;
  role: UserRole;
  department: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserSummary;
}

export interface Ticket {
  id: number;
  ticketNumber: string;
  subject: string;
  description: string;
  category: string;
  priority: TicketPriority;
  status: TicketStatus;
  requesterName: string;
  requesterEmail: string;
  requesterId: string;
  assigneeName: string | null;
  assigneeId: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  resolvedAtUtc: string | null;
}

export interface DashboardStats {
  totalTickets: number;
  openTickets: number;
  inProgressTickets: number;
  resolvedTickets: number;
  urgentTickets: number;
  resolutionPercent: number;
}

export interface TicketFilters {
  search: string;
  status: TicketStatus | '';
  priority: TicketPriority | '';
}
