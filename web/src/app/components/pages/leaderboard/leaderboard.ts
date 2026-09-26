import { Component, computed, effect, inject, signal } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { catchError, map, of, startWith } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { LigaService } from '../../../services/liga.service';
import { ESTADO_EPOCA, LeagueDto, StandingsDto } from '../../../shared/Dtos/Competicao/competicao';
import { FORMA, comSinal, textoZona } from '../../../shared/competicao/competicao';

type EstadoLigas = { tipo: 'a-carregar' } | { tipo: 'erro' } | { tipo: 'ok'; ligas: LeagueDto[] };
type EstadoTabela = { tipo: 'a-carregar' } | { tipo: 'erro' } | { tipo: 'ok'; tabela: StandingsDto };

/**
 * Classificação de uma liga na época atual: PD, V, E, D, GM, GS, DG, P e a forma dos últimos
 * cinco jogos. Vitória 3 pontos, empate 1, derrota 0; os amigáveis não contam.
 */
@Component({
  selector: 'app-leaderboard',
  imports: [RouterLink],
  templateUrl: './leaderboard.html',
  styleUrl: './leaderboard.css',
})
export class Leaderboard {
  private readonly ligasService = inject(LigaService);
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly forma = FORMA;
  protected readonly comSinal = comSinal;
  protected readonly textoZona = textoZona;
  protected readonly estadoEpoca = ESTADO_EPOCA;

  protected readonly ligas = toSignal(
    this.ligasService.getLigas().pipe(
      map((ligas): EstadoLigas => ({ tipo: 'ok', ligas: ligas ?? [] })),
      catchError(() => of<EstadoLigas>({ tipo: 'erro' })),
      startWith<EstadoLigas>({ tipo: 'a-carregar' })
    ),
    { requireSync: true }
  );

  protected readonly ligaEscolhida = signal<string | null>(null);
  protected readonly tabela = signal<EstadoTabela>({ tipo: 'a-carregar' });

  protected readonly dadosTabela = computed(() => {
    const t = this.tabela();
    return t.tipo === 'ok' ? t.tabela : null;
  });

  protected readonly listaLigas = computed(() => {
    const l = this.ligas();
    return l.tipo === 'ok' ? l.ligas : [];
  });

  constructor() {
    // Por omissão, a liga do escalão mais alto.
    effect(() => {
      const ligas = this.listaLigas();
      if (ligas.length && !this.ligaEscolhida()) {
        this.ligaEscolhida.set([...ligas].sort((a, b) => a.level - b.level)[0].id);
      }
    });

    effect(() => {
      const id = this.ligaEscolhida();
      if (!id) {
        return;
      }
      this.tabela.set({ tipo: 'a-carregar' });
      this.ligasService.getClassificacao(id).subscribe({
        next: (tabela) => this.tabela.set({ tipo: 'ok', tabela }),
        error: () => this.tabela.set({ tipo: 'erro' }),
      });
    });
  }
}
