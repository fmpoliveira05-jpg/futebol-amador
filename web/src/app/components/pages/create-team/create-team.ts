import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { TeamService } from '../../../services/team.service';
import { CreateTeamDto } from '../../../shared/Dtos/Team/CreateTeamDto';
import { mensagemDeErro } from '../../../shared/http/erros';
import { EquipaForm } from '../../partials/equipa-form/equipa-form';
import { novaChaveIdempotencia } from '../../../shared/http/idempotencia';

/**
 * Criação de uma equipa. Só aparece a quem ainda não tem equipa (o guard da rota trata disso);
 * quem cria fica administrador.
 */
@Component({
  selector: 'app-create-team',
  imports: [EquipaForm],
  templateUrl: './create-team.html',
})
export class CreateTeam {
  private readonly auth = inject(AuthService);
  private readonly teams = inject(TeamService);
  private readonly router = inject(Router);

  protected readonly aGuardar = signal(false);
  protected readonly erro = signal<string | null>(null);
  /** A mesma chave enquanto a equipa não for criada: repetir depois de um erro não cria duas. */
  private chave = novaChaveIdempotencia();

  protected criar(dados: CreateTeamDto): void {
    if (this.aGuardar()) {
      return;
    }
    this.aGuardar.set(true);
    this.erro.set(null);

    this.teams.createTeam(dados, this.chave).subscribe({
      next: (criada) => {
        this.aGuardar.set(false);
        this.chave = novaChaveIdempotencia();
        if (!criada?.id) {
          this.erro.set('A equipa foi criada, mas a resposta da API não trouxe o id.');
          return;
        }
        this.auth.setTeam(criada.id, true);
        this.router.navigate(['/team/details', criada.id]);
      },
      error: (e) => {
        this.aGuardar.set(false);
        this.erro.set(mensagemDeErro(e, 'Não foi possível criar a equipa.'));
      },
    });
  }
}
