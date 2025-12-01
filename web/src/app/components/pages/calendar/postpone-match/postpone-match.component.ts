import { Component, OnInit } from '@angular/core';
import { CalendarService } from '../../../../services/calendar.service';
import { ActivatedRoute } from '@angular/router';
import { MatchDto } from '../../../../shared/Dtos/Match/MatchDto';
import { PostponeMatchDto } from '../../../../shared/Dtos/Match/PostponeMatchDto';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-postpone-match',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './postpone-match.component.html'
})
export class PostponeMatchComponent implements OnInit {
  match!: MatchDto;
  newDate: string = '';
  idTeam: string = '';

  constructor(private calendarService: CalendarService, private route: ActivatedRoute) {}

  ngOnInit() {
    const matchId = this.route.snapshot.paramMap.get('id');
    if (matchId) {
      this.idTeam = this.route.snapshot.paramMap.get('idTeam') || '';

      this.calendarService.getMatchById(this.idTeam, matchId).subscribe((data: MatchDto) => {
        this.match = data;
      });
    }
  }

  submitPostpone() {
    if (this.newDate) {
      const postponedMatch: PostponeMatchDto = {
        idMatch: this.match.idMatch,
        postPoneDate: this.newDate,
        idTeam: this.idTeam,
        idOpponent: this.match.opponent.idTeam
      };

      this.calendarService.postponeMatch(this.idTeam, postponedMatch).subscribe(() => {
        alert('Partida adiada com sucesso!');
      }, error => {
        console.error('Erro ao adiar a partida', error);
        alert('Houve um erro ao adiar a partida.');
      });
    } else {
      alert('Por favor, selecione uma nova data.');
    }
  }
}