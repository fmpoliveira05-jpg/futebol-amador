import { Component, computed, effect, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LigaService } from '../../../services/liga.service';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { TransferenciasService } from '../../../services/transferencias.service';
import { LeagueDto, MarketFilter, MarketPlayerDto } from '../../../shared/Dtos/Competicao/competicao';
import { POSICOES, POSITION_MAP } from '../../../shared/constants/position-map';
import { mensagemDeErro } from '../../../shared/http/erros';

/** Valores do formulário de filtros ("todos" é a opção vazia). */
interface Filtros {
  temEquipa: '' | 'sim' | 'nao';
  liga: string;
  nacionalidade: string;
  posicao: string;
  nome: string;
  soListados: boolean;
}

const VAZIO: Filtros = { temEquipa: '', liga: '', nacionalidade: '', posicao: '', nome: '', soListados: false };

/**
 * Mercado de transferências: jogadores de outras equipas e jogadores livres, com filtros por equipa,
 * liga, nacionalidade e posição. Aos jogadores com equipa faz-se uma proposta (sem dinheiro); aos
 * livres, um convite como antes.
 */
@Component({
  selector: 'app-mercado',
  imports: [FormsModule, RouterLink],
  templateUrl: './mercado.html',
  styleUrl: './mercado.css',
})
export class Mercado {
  private readonly transferencias = inject(TransferenciasService);
  private readonly convites = inject(MembershipRequestService);
  protected readonly sessao = inject(AuthService).sessao;
  protected readonly posicoes = POSICOES;
  protected readonly nomePosicao = POSITION_MAP;

  protected readonly ligas = signal<LeagueDto[]>([]);
  protected formulario: Filtros = { ...VAZIO };
  protected readonly filtros = signal<Filtros>({ ...VAZIO });
  protected readonly jogadores = signal<MarketPlayerDto[]>([]);
  protected readonly aCarregar = signal(true);
  protected readonly erro = signal<string | null>(null);
  protected readonly feito = signal<Record<string, string>>({});
  protected readonly aEnviar = signal<string | null>(null);
  protected readonly propostaAberta = signal<string | null>(null);
  protected mensagemProposta = '';

  protected readonly nacionalidades = computed(() =>
    [...new Set(this.jogadores().map((j) => j.nationality).filter((n): n is string => !!n))].sort((a, b) =>
      a.localeCompare(b, 'pt')
    )
  );

  constructor() {
    inject(LigaService)
      .getLigas()
      .subscribe({ next: (l) => this.ligas.set(l ?? []), error: () => this.ligas.set([]) });

    effect(() => {
      const f = this.filtros();
      const equipa = this.sessao().equipaId;
      if (!equipa) {
        return;
      }
      const filtro: MarketFilter = {
        hasTeam: f.temEquipa === '' ? null : f.temEquipa === 'sim',
        leagueId: f.temEquipa === 'sim' && f.liga ? f.liga : null,
        nationality: f.nacionalidade || null,
        position: f.posicao === '' ? null : Number(f.posicao),
        name: f.nome || null,
        onlyListed: f.soListados || null,
      };
      this.aCarregar.set(true);
      this.transferencias.getMercado(equipa, filtro).subscribe({
        next: (j) => {
          this.jogadores.set(j);
          this.aCarregar.set(false);
        },
        error: (e) => {
          this.erro.set(mensagemDeErro(e, 'Não foi possível carregar o mercado.'));
          this.aCarregar.set(false);
        },
      });
    });
  }

  protected aplicar(): void {
    this.erro.set(null);
    this.filtros.set({ ...this.formulario });
  }

  protected limpar(): void {
    this.formulario = { ...VAZIO };
    this.aplicar();
  }

  protected proporTransferencia(j: MarketPlayerDto): void {
    const equipa = this.sessao().equipaId;
    if (!equipa) {
      return;
    }
    this.aEnviar.set(j.playerId);
    this.transferencias.proporTransferencia(equipa, j.playerId, this.mensagemProposta.trim()).subscribe({
      next: (p) => {
        this.feito.update((f) => ({
          ...f,
          [j.playerId]: p.status === 1 ? 'Proposta enviada ao jogador' : 'Proposta enviada ao clube',
        }));
        this.aEnviar.set(null);
        this.propostaAberta.set(null);
        this.mensagemProposta = '';
      },
      error: (e) => {
        this.erro.set(mensagemDeErro(e, `Não foi possível fazer a proposta por ${j.name}.`));
        this.aEnviar.set(null);
      },
    });
  }

  protected convidar(j: MarketPlayerDto): void {
    this.aEnviar.set(j.playerId);
    this.convites.sendMembershipRequestTeam(j.playerId).subscribe({
      next: () => {
        this.feito.update((f) => ({ ...f, [j.playerId]: 'Convite enviado' }));
        this.aEnviar.set(null);
      },
      error: (e) => {
        this.erro.set(mensagemDeErro(e, `Não foi possível convidar ${j.name}.`));
        this.aEnviar.set(null);
      },
    });
  }
}
