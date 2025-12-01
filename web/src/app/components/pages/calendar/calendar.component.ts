import { Component, OnInit } from '@angular/core';
import { CalendarService } from '../../../services/calendar.service';
import { Router } from '@angular/router';
import { CalendarDto } from '../../../shared/Dtos/Calendar/CalendarDto';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../../../services/auth.service';
import { MatchStatus } from '../../../shared/Dtos/Match/MatchStatus';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './calendar.component.html',
})
export class CalendarComponent implements OnInit {
  matches: CalendarDto[] = [];
  newDate: string = '';
  idTeam: string = '';

  constructor(
    private calendarService: CalendarService,
    private router: Router,
    private authService: AuthService
  ) {}

  ngOnInit() {
    this.authService.getCurrentTeamId().subscribe(idTeam => {
      this.idTeam = idTeam || '';
      if (this.idTeam) {
        this.calendarService.getMatchesForTeam(this.idTeam).subscribe((data: CalendarDto[]) => {
          this.matches = data;
        });
      } else {
        alert('Não estás associado a nenhuma equipa.');
      }
    });
  }

  postponeMatch(match: CalendarDto) {
    const postponedMatch = {
      idMatch: match.idMatch,
      postPoneDate: this.newDate,
      idTeam: this.idTeam,
      idOpponent: match.opponent.idTeam,
    };

    this.calendarService.postponeMatch(this.idTeam, postponedMatch).subscribe(() => {
      alert('Partida adiada com sucesso!');
      this.ngOnInit();
    });
  }

  confirmCancel(match: CalendarDto) {
    if (confirm('Tens a certeza de que queres cancelar esta partida? Esta ação é irreversível.')) {
      this.cancelMatch(match);
    }
  }

  cancelMatch(match: CalendarDto) {
    this.calendarService.cancelMatch(this.idTeam, match.idMatch).subscribe(() => {
      alert('Partida cancelada com sucesso!');
      this.ngOnInit();
    });
  }
}