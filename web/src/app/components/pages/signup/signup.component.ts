import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { FormBuilder, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
})
export class SignupComponent {
  errorMessage: string | null = null;
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  signupForm = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    dateOfBirth: ['', [Validators.required]],
    address: ['', [Validators.required]],
    phone: ['', [Validators.required]],
    position: [0, [Validators.required]],
    height: [0, [Validators.required]],
  });

  onSignup() {
    if (this.signupForm.invalid) {
      this.signupForm.markAllAsTouched();
      return;
    }

    const name = this.signupForm.value.name ?? '';
    const email = this.signupForm.value.email ?? '';
    const password = this.signupForm.value.password ?? '';
    const dateOfBirth = this.signupForm.value.dateOfBirth ?? '';
    const address = this.signupForm.value.address ?? '';
    const phone = this.signupForm.value.phone ?? '';
    const position = Number(this.signupForm.value.position ?? 0);
    const height = Number(this.signupForm.value.height ?? 0);

    this.authService.signup(name, email, password, dateOfBirth, address, phone, position, height).subscribe({
      next: () => {
        this.router.navigate(['/players']);
      },
      error: (err) => {
        console.error(err);
        if (err.status === 400 && err.error.errors) {
          const errorMessages = err.error.errors;
          this.errorMessage = this.formatValidationErrors(errorMessages);
        } else if (err.error && err.error.detail) {
          this.errorMessage = err.error.detail;
        } else {
          this.errorMessage = 'Erro ao criar conta. Tenta novamente.';
        }
      },
    });
  }

  private formatValidationErrors(errors: any): string {
    let message = '';
    for (const field in errors) {
      if (errors.hasOwnProperty(field)) {
        message += `${field}: ${errors[field].join(', ')}\n`;
      }
    }
    return message;
  }
}