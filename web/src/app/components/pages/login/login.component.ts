import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { FormBuilder, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { CookieService } from 'ngx-cookie-service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private cookieService = inject(CookieService);
  
  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email, Validators.minLength(4), Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(6)]],
  });

  onLogin() {
    if (this.loginForm.invalid) return;

    const { email, password } = this.loginForm.value as { email: string; password: string };

    this.authService.login(email, password).subscribe({
      next: (response) => {
        console.log(response);
        this.cookieService.set('access_token', response.firebaseLoginResponseDto.idToken, 7, '/');
        this.cookieService.set('user_id', response.firebaseLoginResponseDto.localId, 7, '/');

        this.router.navigate(['/players/me']);
      },
      error: (err) => {
        console.error(err);
        alert('Falha no login! Tenta novamente.');
      },
    });
  }
}