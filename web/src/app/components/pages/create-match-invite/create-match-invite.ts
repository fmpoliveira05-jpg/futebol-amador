import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SendMatchInviteDto } from '../../../shared/Dtos/Match/SendMatchInviteDto';
import { AuthService } from '../../../services/auth.service';
import { MatchInviteService } from '../../../services/match-invite.service';
import { InfoMatchInviteDto } from '../../../shared/Dtos/Match/InfoMatchInviteDto';

/**
 * Componente responsável pela criação e negociação de convites de partida.
 * Opera em dois modos: Criação (POST) ou Negociação (PUT/Contraproposta).
 */
@Component({
  selector: 'app-create-match-invite',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-match-invite.html',
  styleUrl: './create-match-invite.css',
})
export class CreateMatchInvite implements OnInit {
  
  /** ID da equipa do utilizador logado (remetente ou negociador). */
  currentTeamId = signal<string | null>(null);

  /** ID da equipa adversária (recetor ou alvo da negociação). */
  targetTeamId: string | null = null;
  
  /** Dados do convite original, presentes apenas no modo Negociação (passado via Router State). */
  inviteToNegotiate: InfoMatchInviteDto | null = null;
  
  /** Flag que indica se o componente está no modo de Negociação. */
  isNegotiating = false;

  /** Estado de carregamento da requisição. */
  protected readonly isLoading = signal<boolean>(false);
  
  /** Mensagem de erro a ser exibida na UI. */
  protected readonly errorMessage = signal<string | null>(null);
  
  /** O grupo de formulário reativo que contém os inputs de data e local. */
  protected form!: FormGroup;
  
  /** A data mínima permitida para o jogo (atual + 12 horas), formatada para o input HTML5. */
  minDate = '';

  /**
   * Construtor do componente.
   * Verifica o estado da navegação para determinar o modo de operação e calcula a data mínima.
   * @param auth Serviço de autenticação para obter o ID do utilizador/equipa.
   * @param fb Serviço para construção de formulários reativos.
   * @param route Serviço para acesso aos parâmetros da rota.
   * @param router Serviço para navegação entre páginas.
   * @param matchInviteService Serviço para operações com convites de partida.
   */
  constructor(
    private auth: AuthService,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private matchInviteService: MatchInviteService
  ) {
    this.calculateMinDate();

    const navigation = this.router.getCurrentNavigation();
    if (navigation?.extras.state && navigation.extras.state['invite']) {
      this.inviteToNegotiate = navigation.extras.state['invite'];
      this.isNegotiating = true;
    }
  }

  /**
   * @method ngOnInit
   * Inicializa o componente.
   * Configura o formulário e subscreve à obtenção do ID da equipa do utilizador logado.
   */
  ngOnInit() {
    this.form = this.fb.group({
      gameDate: ['', [Validators.required]],
      homePitch: [true, [Validators.required]],
    });

    this.auth.getCurrentTeamId().subscribe({
      next: (data) => {
        this.currentTeamId.set(data);
        this.targetTeamId = this.route.snapshot.paramMap.get('teamId');
      },
      error: (err) => console.error(err),
    });
  }

  /**
   * @method calculateMinDate
   * Calcula e formata a data mínima permitida para a partida (atual + 12 horas) no formato exigido pelo input HTML5 (`datetime-local`).
   * @private
   */
  private calculateMinDate() {
    const now = new Date();
    now.setHours(now.getHours() + 12);
    const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
    this.minDate = localDate.toISOString().slice(0, 16);
  }
  
  /**
   * @method submitAction
   * Processa a submissão do formulário.
   * Determina a rota da API (Negociação ou Envio) com base no estado do componente.
   * @returns {void}
   */
  submitAction() {
    if (this.form.invalid) return;
    if (!this.targetTeamId || !this.currentTeamId()) return;

    const payload: SendMatchInviteDto = {
      idSender: this.currentTeamId()!,
      idReceiver: this.targetTeamId,
      gameDate: this.form.value.gameDate,
      homePitch: this.form.value.homePitch,
    };

    this.isLoading.set(true);
    this.errorMessage.set(null);

    if (this.isNegotiating && this.inviteToNegotiate) {
      this.matchInviteService.negotiateMatchInvite(this.currentTeamId()!, payload).subscribe({
        next: () => this.handleSuccess(),
        error: (err) => this.handleError(err),
      });
    } else {
      this.matchInviteService.sendMatchInvite(this.currentTeamId()!, payload).subscribe({
        next: () => this.handleSuccess(),
        error: (err) => this.handleError(err),
      });
    }
  }

  /**
   * @method handleSuccess
   * Trata a conclusão bem-sucedida de uma requisição, limpando o estado de loading e navegando.
   * @private
   * @returns {void}
   */
  private handleSuccess() {
    this.isLoading.set(false);
    this.router.navigate(['/team/matchInvites']);
  }

  /**
   * @method handleError
   * Trata erros de requisição, exibindo a mensagem na UI.
   * @private
   * @param {any} err - O erro retornado pela API.
   * @returns {void}
   */
  private handleError(err: any) {
    console.error(err);
    this.errorMessage.set('Ocorreu um erro ao processar o convite.');
    this.isLoading.set(false);
  }
}