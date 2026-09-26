import { Component, computed, effect, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LigaService } from '../../../services/liga.service';
import { ESTADO_EPOCA, EstadoEpoca, FixtureRoundDto, LeagueDto } from '../../../shared/Dtos/Competicao/competicao';
import { MATCH_STATUS } from '../../../shared/constants/match-status-map';
import { mensagemDeErro } from '../../../shared/http/erros';

/**
 * Ligas (escalões) com a época atual e o calendário por jornadas, sorteado a duas voltas. Os
 * administradores inscrevem a equipa quando as inscrições estão abertas. As ligas e as épocas são
 * geridas pelo super administrador na API.
 */
@Component({
  selector: 'app-ligas',
  imports: [DatePipe, RouterLink],
  templateUrl: './ligas.html',
  styleUrl: './ligas.css',
})
export class Ligas {
  private readonly servico = inject(LigaService);
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly estados = ESTADO_EPOCA;
  protected readonly estadoJogo = MATCH_STATUS;
  protected readonly EstadoEpoca = EstadoEpoca;

  protected readonly ligas = signal<LeagueDto[] | null>(null);
  protected readonly escolhida = signal<string | null>(null);
  protected readonly jornadas = signal<FixtureRoundDto[] | null>(null);
  protected readonly erro = signal<string | null>(null);
  protected readonly info = signal<string | null>(null);

  protected readonly liga = computed(() => this.ligas()?.find((l) => l.id === this.escolhida()) ?? null);

  constructor() {
    this.servico.getLigas().subscribe({
      next: (l) => {
        const ordenadas = [...(l ?? [])].sort((a, b) => a.level - b.level);
        this.ligas.set(ordenadas);
        if (ordenadas.length) {
          this.escolhida.set(ordenadas[0].id);
        }
      },
      error: (e) => this.erro.set(mensagemDeErro(e, 'Não foi possível carregar as ligas.')),
    });

    effect(() => {
      const epoca = this.liga()?.currentSeason;
      this.jornadas.set(null);
      if (epoca && epoca.status !== EstadoEpoca.Inscricoes) {
        this.servico.getJornadas(epoca.id).subscribe({
          next: (j) => this.jornadas.set(j),
          error: () => this.jornadas.set([]),
        });
      }
    });
  }

  protected inscrever(liga: LeagueDto): void {
    const equipa = this.sessao().equipaId;
    if (!equipa) {
      return;
    }
    this.erro.set(null);
    this.servico.inscrever(liga.id, equipa).subscribe({
      next: (epoca) => {
        this.info.set(`Equipa inscrita na ${liga.name} ${epoca.name}.`);
        this.ligas.update((ls) => ls?.map((l) => (l.id === liga.id ? { ...l, currentSeason: epoca } : l)) ?? null);
      },
      error: (e) => this.erro.set(mensagemDeErro(e, 'Não foi possível inscrever a equipa.')),
    });
  }
}
