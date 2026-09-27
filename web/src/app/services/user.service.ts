import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
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

  /** Cópia de todos os dados pessoais (RGPD: acesso e portabilidade), como ficheiro JSON. */
  exportarDados(): Observable<Blob> {
    return this.http
      .get<unknown>(`${environment.apiBaseUrl}/User/me/export`)
      .pipe(map((dados) => new Blob([JSON.stringify(dados, null, 2)], { type: 'application/json' })));
  }

  /** Elimina a conta (RGPD: apagamento). Exige a palavra-passe atual; é irreversível. */
  eliminarConta(palavraPasse: string): Observable<void> {
    return this.http.delete<void>(`${environment.apiBaseUrl}/User/me`, { body: { password: palavraPasse } });
  }
}
