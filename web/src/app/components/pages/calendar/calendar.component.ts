import { Component, OnInit } from '@angular/core';
import { MatchService } from '../../../services/match.service';
import { Router } from '@angular/router';
import { MatchDto } from '../../../shared/Dtos/Match/Match';
import { MatchStatus } from '../../../shared/Dtos/Match/MatchStatus';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './calendar.component.html'
})
export class CalendarComponent implements OnInit {
  matches: MatchDto[] = [];
  newDate: string = '';
  idTeam: string = '';

  constructor(
    private matchService: MatchService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.getCurrentTeamId().subscribe(idTeam => {
      this.idTeam = idTeam || '';
      if (this.idTeam) {
        this.matchService.getMatchesForTeam(this.idTeam).subscribe((data: MatchDto[]) => {
          this.matches = data;
        });
      } else {
        alert('Você não está associado a nenhuma equipe.');
      }
    });
  }

  postponeMatch(match: MatchDto) {
    const postponedMatch = {
      idMatch: match.idMatch,
      postPoneDate: this.newDate,
      idTeam: this.idTeam,
      idOpponent: match.opponent.idTeam
    };

    this.matchService.postponeMatch(this.idTeam, postponedMatch).subscribe(() => {
      alert('Partida adiada com sucesso!');
      this.ngOnInit();
    });
  }

  confirmCancel(match: MatchDto) {
    if (confirm('Tem a certeza de que deseja cancelar esta partida? Esta ação é irreversível.')) {
      this.cancelMatch(match);
    }
  }

  cancelMatch(match: MatchDto) {
    this.matchService.cancelMatch(this.idTeam, match.idMatch).subscribe(() => {
      alert('Partida cancelada com sucesso!');
      this.ngOnInit();
    });
  }
}