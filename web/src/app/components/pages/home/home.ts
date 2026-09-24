import { Component, computed, inject } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { TeamService } from '../../../services/team.service';
import { HomePageDto } from '../../../shared/Dtos/Home/HomePageDto';
import { srcEmblema } from '../../../shared/imagem/imagem';

type EstadoResumo =
  | { tipo: 'sem-equipa' }
  | { tipo: 'a-carregar' }
  | { tipo: 'erro' }
  | { tipo: 'ok'; dados: HomePageDto };

/**
 * Página inicial. Um visitante vê a apresentação da plataforma; um jogador sem equipa vê os
 * passos para entrar numa; um membro de uma equipa vê os próximos jogos e a forma recente.
 */
@Component({
  selector: 'app-home',
  imports: [RouterLink, DatePipe],
  templateUrl: './home.html',
  styleUrl: './home.css',
})
export class Home {
  private readonly auth = inject(AuthService);
  private readonly teams = inject(TeamService);

  protected readonly sessao = this.auth.sessao;

  protected readonly resumo = toSignal(
    toObservable(computed(() => this.sessao().equipaId)).pipe(
      switchMap((equipaId) =>
        !equipaId
          ? of<EstadoResumo>({ tipo: 'sem-equipa' })
          : this.teams.getHomePage(equipaId).pipe(
              map((dados): EstadoResumo => ({ tipo: 'ok', dados })),
              catchError(() => of<EstadoResumo>({ tipo: 'erro' })),
              startWith<EstadoResumo>({ tipo: 'a-carregar' })
            )
      )
    ),
    { initialValue: { tipo: 'a-carregar' } as EstadoResumo }
  );

  protected readonly dados = computed(() => {
    const r = this.resumo();
    return r.tipo === 'ok' ? r.dados : null;
  });

  protected readonly emblema = srcEmblema;

  /** Letra e classe de cada resultado (valores do enum `MatchResult` da API). */
  protected readonly forma: Record<number, { letra: string; classe: string; texto: string }> = {
    0: { letra: 'V', classe: 'forma--v', texto: 'Vitória' },
    1: { letra: 'D', classe: 'forma--d', texto: 'Derrota' },
    2: { letra: 'E', classe: 'forma--e', texto: 'Empate' },
  };
}
