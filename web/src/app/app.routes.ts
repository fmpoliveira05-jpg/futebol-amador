import { Routes } from '@angular/router';
import { Home } from './components/pages/home/home';
import { Teams } from './components/pages/teams/teams';
import { PlayerListPageComponent } from './components/pages/player-list/player-list-page.component';
import { PlayerProfilePageComponent } from './components/pages/player-profile/player-profile-page.component';
import { SettingsPageComponent } from './components/pages/settings-page/settings-page.component';
import { PlayerProfileRedirectComponent } from './components/pages/player-profile/player-profile-redirect/player-profile-redirect.component';
import { LoginComponent } from './components/pages/login/login.component';
import { SignupComponent } from './components/pages/signup/signup.component';
import { LogoutComponent } from './components/pages/login/logout.component';
import { AuthGuard } from './shared/auth/auth.guard';
import { PlayerMembershipRequestsPageComponent } from './components/pages/membership-requests/player-membership-requests.component';
import { TeamMembershipRequestsPageComponent } from './components/pages/membership-requests/team-membership-requests.component';
import { TeamProfile } from './components/pages/team-profile/team-profile';
import { CreateTeam } from './components/pages/create-team/create-team';
import { MatchInvites } from './components/pages/match-invites/match-invites';
import { TeamMembersPageComponent } from './components/pages/team-members/team-members.component';
import { CalendarComponent } from './components/pages/calendar/calendar.component';

/**
 * Configuração de rotas da aplicação.
 * Define as rotas e os componentes associados, incluindo proteção com guardas de autenticação e permissões.
 */
export const routes: Routes = [
  { path: '', component: Home},
  { path: 'teams', component: Teams, canActivate: [AuthGuard]},
  { path: 'teams/:searchTerm', component: Teams, canActivate: [AuthGuard]},
  { path: 'team/details/:teamId', component: TeamProfile, canActivate: [AuthGuard]}, /* cena de is admin aq tb */
  { path: 'team/membership-requests', component: TeamMembershipRequestsPageComponent, canActivate: [AuthGuard], /*data: { requiresTeam: true },*/},
  { path: 'team/members', component: TeamMembersPageComponent, canActivate: [AuthGuard]},
  { path: 'team/matchInvites', component: MatchInvites, canActivate: [AuthGuard]},
  { path: 'createTeam', component: CreateTeam, canActivate: [AuthGuard]},
  { path: 'signup', component: SignupComponent },
  { path: 'login', component: LoginComponent },
  
  // Logout (proteção de acesso com AuthGuard)
  { path: 'logout', component: LogoutComponent, canActivate: [AuthGuard] },

  // Páginas relacionadas aos jogadores
  { path: 'players', component: PlayerListPageComponent, canActivate: [AuthGuard] },
  { path: 'players/me', component: PlayerProfileRedirectComponent, canActivate: [AuthGuard] },
  { path: 'players/details/:playerId', component: PlayerProfilePageComponent, canActivate: [AuthGuard] },
  { path: 'players/calendar/:idTeam', component: CalendarComponent, canActivate: [AuthGuard] },

  // Página de pedidos de adesão de jogadores
  { path: 'players/membership-requests', component: PlayerMembershipRequestsPageComponent, canActivate: [AuthGuard], data: { requiresTeam: false } },

  // Página de definições do utilizador
  { path: 'settings', component: SettingsPageComponent },

  // Redirecionamento para a página de jogadores em caso de rota inválida
  { path: '**', redirectTo: 'players' },
];