import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

/** Operações sobre a conta do utilizador autenticado. */
@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly http = inject(HttpClient);

  /** As palavras-passe vão no corpo do pedido (nunca no URL, onde ficariam em logs e no histórico). */
  alterarPalavraPasse(palavraPasseAtual: string, novaPalavraPasse: string): Observable<void> {
    return this.http.put<void>(`${environment.apiBaseUrl}/User/password`, {
      currentPassword: palavraPasseAtual,
      newPassword: novaPalavraPasse,
    });
  }
}
