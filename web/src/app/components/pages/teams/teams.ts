import { Component, DestroyRef, OnInit, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, catchError, combineLatest, of, switchMap, tap } from 'rxjs';
import { PlayerService } from '../../../services/player.service';
import { TeamService } from '../../../services/team.service';
import { InfoTeamDto } from '../../../shared/Dtos/Team/InfoTeamDto';
import { FilterListTeamDto } from '../../../shared/Dtos/Filters/FilterListTeamDto';
import { PlayerDetails } from '../../../shared/Dtos/player.model';
import { SearchTeam } from '../../partials/search-team/search-team';
import { mensagemDeErro } from '../../../shared/http/erros';
import { srcEmblema } from '../../../shared/imagem/imagem';

/**
 * Lista de equipas.
 *
 * - Um jogador sem equipa vê as equipas a que pode pedir para entrar (e pode criar a sua).
 * - Um jogador com equipa vê as outras equipas, possíveis adversários.
 * Em ambos os casos os filtros vêm do URL.
 */
@Component({
  selector: 'app-teams',
  imports: [SearchTeam, RouterLink],
  templateUrl: './teams.html',
  styleUrls: ['./teams.css'],
})
export class Teams implements OnInit {
  private readonly playerService = inject(PlayerService);
  private readonly teamService = inject(TeamService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly teams = signal<InfoTeamDto[]>([]);
  readonly isLoading = signal(true);
  readonly errorMessage = signal<string | null>(null);
  readonly player = signal<PlayerDetails | null>(null);

  protected readonly hasTeam = computed(() => !!this.player()?.team?.idTeam);

  ngOnInit(): void {
    combineLatest([this.playerService.getMyProfile(), this.route.queryParams])
      .pipe(
        tap(() => {
          this.isLoading.set(true);
          this.errorMessage.set(null);
        }),
        // switchMap cancela a pesquisa anterior se os filtros mudarem entretanto.
        // switchMap cancela a pesquisa anterior se os filtros mudarem entretanto. O erro é
        // tratado dentro do switchMap para a página continuar a reagir a novas pesquisas.
        switchMap(([player, params]) => {
          this.player.set(player);
          return this.procurar(player, params as FilterListTeamDto).pipe(
            catchError((err) => {
              this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar as equipas.'));
              return of<InfoTeamDto[]>([]);
            })
          );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: (equipas) => {
          this.teams.set(equipas);
          this.isLoading.set(false);
        },
        error: (err) => {
          // Só chega aqui se falhar o carregamento do perfil.
          this.errorMessage.set(mensagemDeErro(err, 'Não foi possível carregar as equipas.'));
          this.isLoading.set(false);
        },
      });
  }

  private procurar(player: PlayerDetails, filtro: FilterListTeamDto): Observable<InfoTeamDto[]> {
    const teamId = player.team?.idTeam;
    return teamId
      ? this.teamService.searchTeams(teamId, filtro)
      : this.playerService.searchTeams(player.playerId, filtro);
  }

  protected emblema(icone: string | null | undefined): string {
    return srcEmblema(icone);
  }

  idadeMedia(equipa: InfoTeamDto): number {
    return Math.trunc(equipa.averageAge);
  }
}
