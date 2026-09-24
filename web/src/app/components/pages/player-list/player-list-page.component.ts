import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { PlayerService } from '../../../services/player.service';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { PlayerListItem } from '../../../shared/Dtos/player-list-item.model';
import { PlayerFilterDto } from '../../../shared/Dtos/Player/PlayerFilterDto';
import { POSICOES, POSITION_MAP } from '../../../shared/constants/position-map';
import { mensagemDeErro } from '../../../shared/http/erros';

/**
 * Lista de jogadores, para o administrador de uma equipa encontrar e convidar jogadores sem
 * equipa. A API devolve a lista completa; os filtros e a paginação são feitos no browser.
 */
@Component({
  selector: 'app-player-list-page',
  imports: [RouterLink, FormsModule],
  templateUrl: './player-list-page.component.html',
  styleUrls: ['./player-list-page.component.css'],
})
export class PlayerListPageComponent implements OnInit {
  private readonly playerService = inject(PlayerService);
  private readonly membershipRequestService = inject(MembershipRequestService);

  protected static readonly POR_PAGINA = 10;

  protected readonly players = signal<PlayerListItem[]>([]);
  protected readonly isLoading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly showFilters = signal(false);
  protected readonly currentPage = signal(1);
  /** Jogadores já convidados nesta visita (para mostrar a confirmação na própria linha). */
  protected readonly convidados = signal<ReadonlySet<string>>(new Set());
  protected readonly aConvidar = signal<string | null>(null);

  /** Valores do formulário; só passam a contar ao carregar em "Aplicar". */
  protected formulario = new PlayerFilterDto();
  protected readonly filtro = signal(new PlayerFilterDto());

  protected readonly posicoes = POSICOES;
  protected readonly nomePosicao = POSITION_MAP;

  protected readonly filteredPlayers = computed(() => {
    const f = this.filtro();
    const texto = (valor: string, procura: string) => !procura || valor.toLowerCase().includes(procura.toLowerCase());
    return this.players().filter(
      (p) =>
        texto(p.address, f.city) &&
        texto(p.name, f.name) &&
        (f.position === null || p.position === f.position) &&
        p.age >= f.minAge &&
        p.age <= f.maxAge &&
        p.heigth >= f.minHeight &&
        p.heigth <= f.maxHeight
    );
  });

  protected readonly totalPaginas = computed(() =>
    Math.max(1, Math.ceil(this.filteredPlayers().length / PlayerListPageComponent.POR_PAGINA))
  );

  protected readonly visiblePlayers = computed(() => {
    const inicio = (this.currentPage() - 1) * PlayerListPageComponent.POR_PAGINA;
    return this.filteredPlayers().slice(inicio, inicio + PlayerListPageComponent.POR_PAGINA);
  });

  ngOnInit(): void {
    this.playerService.getPlayers().subscribe({
      next: (jogadores) => {
        this.players.set(jogadores);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar os jogadores.'));
        this.isLoading.set(false);
      },
    });
  }

  protected applyFilters(): void {
    this.filtro.set({ ...this.formulario, position: this.formulario.position === null ? null : Number(this.formulario.position) });
    this.currentPage.set(1);
  }

  protected limparFiltros(): void {
    this.formulario = new PlayerFilterDto();
    this.applyFilters();
  }

  protected mudarPagina(delta: number): void {
    this.currentPage.update((p) => Math.min(this.totalPaginas(), Math.max(1, p + delta)));
  }

  protected sendMembershipRequest(jogador: PlayerListItem): void {
    this.aConvidar.set(jogador.id);
    this.errorMessage.set(null);
    this.membershipRequestService.sendMembershipRequestTeam(jogador.id).subscribe({
      next: () => {
        this.aConvidar.set(null);
        this.convidados.update((s) => new Set(s).add(jogador.id));
      },
      error: (err) => {
        this.aConvidar.set(null);
        this.errorMessage.set(mensagemDeErro(err, `Não foi possível convidar ${jogador.name}.`));
      },
    });
  }
}
