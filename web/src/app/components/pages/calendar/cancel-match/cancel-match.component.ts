import { Component, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CalendarService } from '../../../../services/calendar.service';
import { AuthService } from '../../../../services/auth.service';
import { CancelMatchDto } from '../../../../shared/Dtos/Match/CancelMatchDto';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-cancel-match',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './cancel-match.component.html',
  styleUrl: './cancel-match.component.css'
})
export class CancelMatchComponent {
  matchId!: string;
  idTeam!: string;

  cancelMatchDto: CancelMatchDto = {
    idTeam: '',
    idMatch: '',
    description: ''
  };

  protected readonly errorMessage = signal<string | null>(null);
  protected readonly successMessage = signal<string | null>(null);

  constructor(
    private router: Router,
    private route: ActivatedRoute, 
    private authService: AuthService,
    private calendarService: CalendarService
  ) {
    this.route.paramMap.subscribe(params => {
      this.idTeam = params.get('idTeam')!;
      this.matchId = params.get('idMatch')!;
      
      this.cancelMatchDto.idTeam = this.idTeam;
      this.cancelMatchDto.idMatch = this.matchId;
    });
  }

  confirmCancel(): void {
    if (this.cancelMatchDto.description.trim() === '') {
      alert('Por favor, insira o motivo do cancelamento.');
      return;
    }

    this.authService.getCurrentTeamId().subscribe(idTeam => {
      if (!idTeam || idTeam !== this.idTeam) {
        alert('Não foi possível identificar a tua equipa.');
        return;
      }

      this.calendarService.cancelMatch(this.cancelMatchDto.idTeam, this.cancelMatchDto.idMatch, this.cancelMatchDto.description).subscribe(() => {
        this.successMessage.set('Partida cancelada com sucesso!');
        this.router.navigate(['/players/calendar', this.idTeam]);  
      });
    });
  }
}