import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SendMatchInviteDto } from '../../../shared/Dtos/Match/SendMatchInviteDto';
import { AuthService } from '../../../services/auth.service';
import { MatchInviteService } from '../../../services/match-invite.service';
import { InfoMatchInviteDto } from '../../../shared/Dtos/Match/InfoMatchInviteDto';

@Component({
  selector: 'app-create-match-invite',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './create-match-invite.html',
  styleUrl: './create-match-invite.css',
})
export class CreateMatchInvite implements OnInit {
  currentTeamId = signal<string | null>(null);
  
  targetTeamId: string | null = null;
  inviteToNegotiate: InfoMatchInviteDto | null = null;
  isNegotiating = false;

  protected readonly isLoading = signal<boolean>(false);
  protected readonly errorMessage = signal<string | null>(null);
  protected form!: FormGroup;
  minDate = '';

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

  ngOnInit() {
    this.form = this.fb.group({
      gameDate: ['', [Validators.required]],
      homePitch: [true, [Validators.required]],
    });

    this.auth.getCurrentTeamId().subscribe({
      next: (data) => {
        this.currentTeamId.set(data);
        this.setupForm();
        console.log(data);
      },
      error: (err) => console.error(err),
    });
  }

 private setupForm() {
    if (this.isNegotiating && this.inviteToNegotiate) {
      
      const myId = this.currentTeamId();
      const originalSenderId = this.inviteToNegotiate.sender.idTeam;
      const originalReceiverId = this.inviteToNegotiate.receiver.idTeam;

      if (myId === originalSenderId) {
        this.targetTeamId = originalReceiverId;
      } else {
        this.targetTeamId = originalSenderId;
      }
      
      console.log('Negociando contra:', this.targetTeamId); 

      const existingDate = new Date(this.inviteToNegotiate.gameDate);
      existingDate.setMinutes(existingDate.getMinutes() - existingDate.getTimezoneOffset());
      const formattedDate = existingDate.toISOString().slice(0, 16);

      this.form.patchValue({
        gameDate: formattedDate,
        homePitch: true 
      });

    } else {
      this.targetTeamId = this.route.snapshot.paramMap.get('teamId');
    }
  }

  private calculateMinDate() {
    const now = new Date();
    now.setHours(now.getHours() + 12);
    const localDate = new Date(now.getTime() - now.getTimezoneOffset() * 60000);
    this.minDate = localDate.toISOString().slice(0, 16);
  }

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
      this.matchInviteService.negotiateMatchInvite(
        this.currentTeamId()!,
        payload
      ).subscribe({
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

  private handleSuccess() {
    this.isLoading.set(false);
    this.router.navigate(['/team/matchInvites']);
  }

  private handleError(err: any) {
    console.error(err);
    this.errorMessage.set('Ocorreu um erro ao processar o convite.');
    this.isLoading.set(false);
  }
}