import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { FormBuilder, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule]
})
export class SignupComponent {
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
    if (this.signupForm.invalid) return;

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
        alert('Erro ao criar conta. Tente novamente.');
      },
    });
  }
}