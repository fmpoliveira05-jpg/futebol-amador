import { Component, computed, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { LeaderboardService } from '../../../services/leaderboard.service';
import { TeamLeaderboardDto } from '../../../shared/Dtos/Leaderboard/TeamLeaderboardDto';

type Estado =
  | { tipo: 'a-carregar' }
  | { tipo: 'erro' }
  | { tipo: 'ok'; equipas: TeamLeaderboardDto[] };

/** Classificação geral: as 100 equipas com mais pontos, com filtro por nome ou divisão. */
@Component({
  selector: 'app-leaderboard',
  imports: [RouterLink],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.css',
})
export class Leaderboard {
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly pesquisa = signal('');

  protected readonly estado = toSignal(
    inject(LeaderboardService)
      .getClassificacao()
      .pipe(
        map((equipas): Estado => ({ tipo: 'ok', equipas })),
        catchError(() => of<Estado>({ tipo: 'erro' })),
        startWith<Estado>({ tipo: 'a-carregar' })
      ),
    { requireSync: true }
  );

  protected readonly equipas = computed(() => {
    const e = this.estado();
    if (e.tipo !== 'ok') {
      return [];
    }
    const termo = this.pesquisa().trim().toLowerCase();
    return termo
      ? e.equipas.filter(
          (t) => t.teamName.toLowerCase().includes(termo) || t.rankName.toLowerCase().includes(termo)
        )
      : e.equipas;
  });
}
