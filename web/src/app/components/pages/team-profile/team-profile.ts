import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { TeamService } from '../../../services/team.service';
import { MembershipRequestService } from '../../../services/membership-request.service';
import { TeamDetailsDto } from '../../../shared/Dtos/Team/TeamDetailsDto';
import { CreateTeamDto } from '../../../shared/Dtos/Team/CreateTeamDto';
import { POSITION_MAP } from '../../../shared/constants/position-map';
import { mensagemDeErro } from '../../../shared/http/erros';
import { srcEmblema } from '../../../shared/imagem/imagem';
import { EquipaForm } from '../../partials/equipa-form/equipa-form';

/**
 * Perfil de uma equipa. O que se pode fazer depende de quem vê:
 * - administrador da equipa: editar, apagar e ir para a gestão;
 * - administrador de outra equipa: convidar para um jogo;
 * - jogador sem equipa: pedir para entrar.
 */
@Component({
  selector: 'app-team-profile',
  imports: [DatePipe, RouterLink, EquipaForm],
  templateUrl: './team-profile.html',
  styleUrl: './team-profile.css',
})
export class TeamProfile {
  private readonly auth = inject(AuthService);
  private readonly teams = inject(TeamService);
  private readonly membership = inject(MembershipRequestService);
  private readonly router = inject(Router);

  /** Vem do parâmetro `:teamId` da rota. */
  readonly teamId = input.required<string>();

  protected readonly POSITION_MAP = POSITION_MAP;
  protected readonly sessao = this.auth.sessao;

  protected readonly equipa = signal<TeamDetailsDto | null>(null);
  protected readonly aCarregar = signal(true);
  protected readonly naoEncontrada = signal(false);
  protected readonly editar = signal(false);
  protected readonly aGuardar = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly sucesso = signal<string | null>(null);
  protected readonly pedidoEnviado = signal(false);

  protected readonly souMembro = computed(() => this.sessao().equipaId === this.teamId());
  protected readonly souAdmin = computed(() => this.souMembro() && this.sessao().admin);
  protected readonly possoConvidar = computed(
    () => this.sessao().admin && !!this.sessao().equipaId && !this.souMembro()
  );
  protected readonly possoPedirAdesao = computed(() => !this.sessao().equipaId);

  protected readonly emblema = computed(() => srcEmblema(this.equipa()?.icon));

  /** Dados no formato do formulário de edição. */
  protected readonly dadosFormulario = computed<CreateTeamDto | null>(() => {
    const t = this.equipa();
    return t
      ? { name: t.name, description: t.description ?? '', icon: t.icon, homePitch: t.pitchDto }
      : null;
  });

  /** Administradores primeiro, depois por nome. */
  protected readonly plantel = computed(() =>
    [...(this.equipa()?.players ?? [])].sort(
      (a, b) => Number(!!b.isAdmin) - Number(!!a.isAdmin) || a.name.localeCompare(b.name, 'pt')
    )
  );

  constructor() {
    effect(() => this.carregar(this.teamId()));
  }

  private carregar(id: string): void {
    this.aCarregar.set(true);
    this.naoEncontrada.set(false);
    this.editar.set(false);
    this.teams.getTeamById(id).subscribe({
      next: (t) => {
        this.equipa.set(t);
        this.aCarregar.set(false);
      },
      error: (e) => {
        this.aCarregar.set(false);
        if (e?.status === 404) {
          this.naoEncontrada.set(true);
        } else {
          this.erro.set(mensagemDeErro(e, 'Não foi possível carregar a equipa.'));
        }
      },
    });
  }

  protected guardar(dados: CreateTeamDto): void {
    this.aGuardar.set(true);
    this.erro.set(null);
    this.sucesso.set(null);
    this.teams.updateTeam(this.teamId(), dados).subscribe({
      next: () => {
        this.aGuardar.set(false);
        this.sucesso.set('Equipa atualizada.');
        this.carregar(this.teamId());
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível guardar as alterações.'));
      },
    });
  }

  protected pedirAdesao(): void {
    this.aGuardar.set(true);
    this.erro.set(null);
    this.membership.sendMembershipRequestPlayer(this.teamId()).subscribe({
      next: () => {
        this.aGuardar.set(false);
        this.pedidoEnviado.set(true);
        this.sucesso.set('Pedido enviado. Um administrador da equipa vai responder.');
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível enviar o pedido de adesão.'));
      },
    });
  }

  protected apagar(): void {
    const t = this.equipa();
    if (!t || !confirm(`Apagar a equipa "${t.name}"? Todos os membros ficam sem equipa. Não é possível desfazer.`)) {
      return;
    }
    this.aGuardar.set(true);
    this.erro.set(null);
    this.teams.deleteTeam(t.id).subscribe({
      next: () => {
        this.aGuardar.set(false);
        this.auth.setTeam(null, false);
        this.router.navigate(['/']);
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível apagar a equipa.'));
      },
    });
  }
}
