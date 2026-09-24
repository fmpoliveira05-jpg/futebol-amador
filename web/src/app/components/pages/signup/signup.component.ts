import { Component, inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { FormBuilder, Validators } from '@angular/forms';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

/**
 * Componente responsável pelo processo de registo de um novo jogador.
 * Permite ao utilizador criar uma nova conta ao fornecer dados como nome, email, senha, entre outros.
 */
@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
  standalone: true,
  imports: [ReactiveFormsModule, CommonModule],
})
export class SignupComponent {
  
  /**
   * Mensagem de erro que será exibida se ocorrer algum problema durante o processo de registo.
   */
  errorMessage: string | null = null;

  /**
   * Serviço de autenticação, utilizado para realizar o registo do jogador.
   */
  private authService = inject(AuthService);

  /**
   * Serviço de routing, utilizado para navegar entre as páginas.
   */
  private router = inject(Router);

  /**
   * Serviço para construir o formulário de registo com suas respectivas validações.
   */
  private fb = inject(FormBuilder);

  positions = [
    { label: 'Avançado', value: 0 },
    { label: 'Médio', value: 1 },
    { label: 'Defesa', value: 2 },
    { label: 'Guarda-redes', value: 3 }
  ];

  /**
   * Formulário reativo para registo do jogador, que contém campos como nome, email, senha, etc.
   * Cada campo possui validações específicas.
   */
  signupForm = this.fb.group({
    name: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email, Validators.minLength(4), Validators.maxLength(256)]],
    password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(32)]],
    dateOfBirth: ['', [Validators.required]],
    address: ['', [Validators.required]],
    phone: ['', [Validators.required, Validators.pattern(/^\+\d{3,3}\d{9,9}$/)]],
    position: [0, [Validators.required, Validators.pattern(/^[0-3]{1}$/)]],
    height: [0, [Validators.required, Validators.min(100), Validators.max(250)]],
  });

  /**
   * Método responsável por processar o envio do formulário de registo.
   * Realiza validações e, se o formulário for válido, chama o serviço de autenticação para criar a conta.
   */
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

  /**
   * Formata os erros de validação recebidos da API em uma string legível.
   * @param errors Erros de validação retornados pela API.
   * @returns Mensagem formatada com os erros.
   */
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