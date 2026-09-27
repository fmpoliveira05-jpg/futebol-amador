import { Component, computed, effect, inject, input, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { PlayerDetails, UpdatePlayerRequest } from '../../../shared/Dtos/player.model';
import { PlayerService } from '../../../services/player.service';
import { PE_PREFERIDO, PlayerProfileDto, SITUACAO_JOGADOR } from '../../../shared/Dtos/Competicao/competicao';
import { minutos } from '../../../shared/competicao/competicao';
import { AuthService } from '../../../services/auth.service';
import { POSICOES, POSITION_MAP } from '../../../shared/constants/position-map';
import { mensagemDeErro } from '../../../shared/http/erros';
import { idadeMinima } from '../signup/signup.component';

/**
 * Perfil de um jogador. No próprio perfil mostra os contactos e permite editar os dados, sair da
 * equipa e apagar a conta; no perfil de outra pessoa mostra só os dados desportivos.
 */
@Component({
  selector: 'app-player-profile-page',
  imports: [ReactiveFormsModule, RouterLink, DatePipe],
  templateUrl: './player-profile-page.component.html',
  styleUrl: './player-profile-page.component.css',
})
export class PlayerProfilePageComponent {
  private readonly router = inject(Router);
  private readonly players = inject(PlayerService);
  private readonly auth = inject(AuthService);

  /** Vem do parâmetro `:playerId` da rota. */
  readonly playerId = input.required<string>();

  protected readonly POSITION_MAP = POSITION_MAP;
  protected readonly posicoes = POSICOES;
  protected readonly sessao = this.auth.sessao;

  protected readonly pe = PE_PREFERIDO;
  protected readonly situacoes = SITUACAO_JOGADOR;
  protected readonly minutos = minutos;
  protected readonly tipoTransferencia: Record<string, string> = {
    TRANSFERENCIA: 'Transferência',
    ADESAO: 'Entrada (jogador livre)',
    SAIDA: 'Saída',
  };

  protected readonly jogador = signal<PlayerDetails | null>(null);
  /** Estatísticas e histórico (pedido à parte: se falhar, o resto do perfil aparece na mesma). */
  protected readonly perfil = signal<PlayerProfileDto | null>(null);
  protected readonly aCarregar = signal(true);
  protected readonly aGuardar = signal(false);
  protected readonly editar = signal(false);
  protected readonly erro = signal<string | null>(null);
  protected readonly sucesso = signal<string | null>(null);

  protected readonly meuPerfil = computed(() => this.jogador()?.playerId === this.sessao().jogadorId);
  protected readonly temEquipa = computed(() => !!this.jogador()?.team?.idTeam);

  protected readonly form = inject(FormBuilder).nonNullable.group({
    name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(/^\+\d{3}\d{9}$/)]],
    dateOfBirth: ['', [Validators.required, idadeMinima]],
    address: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(250)]],
    position: [0, Validators.required],
    height: [175, [Validators.required, Validators.min(100), Validators.max(250)]],
    weight: [null as number | null, [Validators.min(40), Validators.max(150)]],
    preferredFoot: [null as number | null],
    status: [0],
    nationality: ['', Validators.maxLength(56)],
    countryOfBirth: ['', Validators.maxLength(56)],
    /** Só pedida quando o e-mail muda. */
    palavraPasseAtual: ['', Validators.maxLength(128)],
  });

  /** O e-mail do formulário é diferente do atual (a API exige a palavra-passe). */
  protected emailMudou(): boolean {
    const atual = this.jogador()?.email ?? '';
    return this.editar() && this.form.controls.email.value.trim().toLowerCase() !== atual.toLowerCase();
  }

  constructor() {
    effect(() => this.carregar(this.playerId()));
  }

  private carregar(id: string): void {
    this.carregarPerfil(id);
    this.aCarregar.set(true);
    this.editar.set(false);
    this.erro.set(null);
    // `getPlayerData` também atualiza a sessão quando o perfil é o do próprio utilizador.
    this.auth.getPlayerData(id).subscribe({
      next: (p) => {
        this.jogador.set(p);
        this.aCarregar.set(false);
      },
      error: (e) => {
        this.aCarregar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível carregar o perfil.'));
      },
    });
  }

  private carregarPerfil(id: string): void {
    this.perfil.set(null);
    this.players.getPerfil(id).subscribe({ next: (p) => this.perfil.set(p), error: () => this.perfil.set(null) });
  }

  protected abrirEdicao(): void {
    const p = this.jogador();
    if (!p) {
      return;
    }
    this.form.reset({
      name: p.name,
      email: p.email,
      phone: p.phoneNumber,
      dateOfBirth: p.dateOfBirth,
      address: p.address,
      position: p.position,
      height: p.height,
      weight: this.perfil()?.weight ?? null,
      preferredFoot: this.perfil()?.preferredFoot ?? null,
      status: this.perfil()?.status ?? 0,
      nationality: this.perfil()?.nationality ?? '',
      countryOfBirth: this.perfil()?.countryOfBirth ?? '',
      palavraPasseAtual: '',
    });
    this.sucesso.set(null);
    this.editar.set(true);
  }

  protected invalido(campo: keyof typeof this.form.controls): boolean {
    const c = this.form.controls[campo];
    return c.invalid && c.touched;
  }

  protected guardar(): void {
    const p = this.jogador();
    if (!p || this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const v = this.form.getRawValue();
    const mudaEmail = this.emailMudou();
    if (mudaEmail && !v.palavraPasseAtual) {
      this.erro.set('Para mudar o e-mail indica a palavra-passe atual.');
      return;
    }
    const dados: UpdatePlayerRequest = {
      CurrentPassword: mudaEmail ? v.palavraPasseAtual : null,
      Name: v.name.trim(),
      Email: v.email.trim(),
      Phone: v.phone.trim(),
      DateOfBirth: v.dateOfBirth,
      Address: v.address.trim(),
      Position: Number(v.position),
      Height: Number(v.height),
      Weight: v.weight === null || (v.weight as unknown) === '' ? null : Number(v.weight),
      PreferredFoot: v.preferredFoot === null || (v.preferredFoot as unknown) === '' ? null : Number(v.preferredFoot),
      Status: Number(v.status),
      Nationality: v.nationality.trim() || null,
      CountryOfBirth: v.countryOfBirth.trim() || null,
    };
    this.executar(this.players.updatePlayer(p.playerId, dados), 'Perfil atualizado.', 'Não foi possível guardar as alterações.', () => {
      if (mudaEmail) {
        // A API terminou as sessões: é preciso confirmar o e-mail novo e voltar a entrar.
        this.auth.clearSession();
        this.router.navigate(['/login'], { queryParams: { registo: 'pendente' } });
      }
    });
  }

  protected sairDaEquipa(): void {
    const p = this.jogador();
    if (!p?.team || !confirm(`Sair da equipa ${p.team.name}?`)) {
      return;
    }
    this.executar(this.players.leaveTeam(p.playerId), `Saíste da equipa ${p.team.name}.`, 'Não foi possível sair da equipa.', () =>
      this.auth.setTeam(null, false)
    );
  }

  protected apagarConta(): void {
    const p = this.jogador();
    if (!p || !confirm('Apagar a tua conta? Perdes o acesso e não é possível desfazer.')) {
      return;
    }
    this.aGuardar.set(true);
    this.players.deletePlayer(p.playerId).subscribe({
      next: () => {
        this.auth.clearSession();
        this.router.navigate(['/']);
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível apagar a conta.'));
      },
    });
  }

  private executar(pedido: ReturnType<PlayerService['leaveTeam']>, ok: string, falha: string, depois?: () => void): void {
    this.aGuardar.set(true);
    this.erro.set(null);
    this.sucesso.set(null);
    pedido.subscribe({
      next: () => {
        this.aGuardar.set(false);
        depois?.();
        this.carregar(this.playerId());
        this.sucesso.set(ok);
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, falha));
      },
    });
  }
}
