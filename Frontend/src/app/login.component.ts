import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { UserRole } from './models';

@Component({
  selector: 'app-login',
  imports: [ReactiveFormsModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private readonly formBuilder = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly roles: { label: string; role: UserRole; email: string; initial: string }[] = [
    { label: 'Admin', role: 'Admin', email: 'admin@supportqueue.com', initial: 'A' },
    { label: 'Support Agent', role: 'SupportAgent', email: 'agent@supportqueue.com', initial: 'S' },
    { label: 'Employee', role: 'Employee', email: 'employee@supportqueue.com', initial: 'E' }
  ];
  selectedRole: UserRole = 'Admin';
  loading = false;
  errorMessage = '';

  readonly form = this.formBuilder.nonNullable.group({
    email: ['admin@supportqueue.com', [Validators.required, Validators.email]],
    password: ['QueueDemo!2026', [Validators.required]]
  });

  selectRole(role: typeof this.roles[number]): void {
    this.selectedRole = role.role;
    this.form.controls.email.setValue(role.email);
    this.errorMessage = '';
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading = true;
    this.errorMessage = '';
    this.auth.login(this.form.controls.email.value, this.form.controls.password.value).subscribe({
      next: () => void this.router.navigate(['/dashboard']),
      error: (error: HttpErrorResponse) => {
        this.loading = false;
        this.errorMessage = error.status === 401
          ? 'That email and password combination was not recognized.'
          : 'Unable to reach the support workspace. Check that the API and SQL Server are running.';
      }
    });
  }
}
