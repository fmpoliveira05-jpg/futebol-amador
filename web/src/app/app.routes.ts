import { inject } from '@angular/core';
import { Routes } from '@angular/router';
import { Home } from './components/pages/home/home';
import { authGuard } from './shared/auth/auth.guard';
import { AuthService } from './services/auth.service';

/**
 * Rotas da aplicação. As páginas são carregadas a pedido (`loadComponent`), por isso o bundle
 * inicial só tem a página inicial, o menu e os serviços.
 *
 * O `authGuard` exige sessão; os `data` restringem mais:
 * `isAdmin` (administrador de uma equipa), `requiresTeam` e `requiresNoTeam`.
 * Os parâmetros das rotas chegam aos componentes como `input()` (`withComponentInputBinding`).
 */
export const routes: Routes = [
  { path: '', component: Home, title: 'Futebol Amador' },

  // Conta
  {
    path: 'login',
    loadComponent: () => import('./components/pages/login/login.component').then((m) => m.LoginComponent),
    title: 'Entrar · Futebol Amador',
  },
  {
    path: 'signup',
    loadComponent: () => import('./components/pages/signup/signup.component').then((m) => m.SignupComponent),
    title: 'Criar conta · Futebol Amador',
  },
  {
    path: 'recuperar',
    loadComponent: () => import('./components/pages/login/recuperar.component').then((m) => m.RecuperarComponent),
    title: 'Recuperar a palavra-passe · Futebol Amador',
  },
  {
    path: 'logout',
    loadComponent: () => import('./components/pages/login/logout.component').then((m) => m.LogoutComponent),
  },
  {
    path: 'settings',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/pages/settings-page/settings-page.component').then((m) => m.SettingsPageComponent),
    title: 'Definições · Futebol Amador',
  },
  {
    path: 'leaderboard',
    loadComponent: () => import('./components/pages/leaderboard/leaderboard').then((m) => m.Leaderboard),
    title: 'Classificação · Futebol Amador',
  },
  {
    path: 'ligas',
    loadComponent: () => import('./components/pages/ligas/ligas').then((m) => m.Ligas),
    title: 'Ligas · Futebol Amador',
  },

  // Jogadores
  {
    path: 'players/me',
    canActivate: [authGuard],
    redirectTo: () => `/players/details/${inject(AuthService).getCurrentPlayerId()}`,
  },
  {
    path: 'players/details/:playerId',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./components/pages/player-profile/player-profile-page.component').then(
        (m) => m.PlayerProfilePageComponent
      ),
    title: 'Jogador · Futebol Amador',
  },
  {
    path: 'players/membership-requests',
    canActivate: [authGuard],
    data: { requiresNoTeam: true },
    loadComponent: () =>
      import('./components/pages/membership-requests/player-membership-requests.component').then(
        (m) => m.PlayerMembershipRequestsPageComponent
      ),
    title: 'Convites e pedidos · Futebol Amador',
  },
  {
    path: 'players',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/player-list/player-list-page.component').then((m) => m.PlayerListPageComponent),
    title: 'Jogadores livres · Futebol Amador',
  },
  {
    path: 'mercado',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () => import('./components/pages/mercado/mercado').then((m) => m.Mercado),
    title: 'Mercado · Futebol Amador',
  },
  {
    path: 'transferencias',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/transferencias/transferencias-equipa').then((m) => m.TransferenciasEquipa),
    title: 'Transferências · Futebol Amador',
  },
  {
    path: 'players/transferencias',
    canActivate: [authGuard],
    data: { requiresTeam: true },
    loadComponent: () =>
      import('./components/pages/transferencias/transferencias-jogador').then((m) => m.TransferenciasJogador),
    title: 'Propostas de transferência · Futebol Amador',
  },

  // Equipas
  {
    path: 'teams',
    canActivate: [authGuard],
    loadComponent: () => import('./components/pages/teams/teams').then((m) => m.Teams),
    title: 'Equipas · Futebol Amador',
  },
  {
    path: 'createTeam',
    canActivate: [authGuard],
    data: { requiresNoTeam: true },
    loadComponent: () => import('./components/pages/create-team/create-team').then((m) => m.CreateTeam),
    title: 'Criar equipa · Futebol Amador',
  },
  {
    path: 'team/details/:teamId',
    canActivate: [authGuard],
    loadComponent: () => import('./components/pages/team-profile/team-profile').then((m) => m.TeamProfile),
    title: 'Equipa · Futebol Amador',
  },
  {
    path: 'team/members',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/team-members/team-members.component').then((m) => m.TeamMembersPageComponent),
    title: 'Membros · Futebol Amador',
  },
  {
    path: 'team/membership-requests',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/membership-requests/team-membership-requests.component').then(
        (m) => m.TeamMembershipRequestsPageComponent
      ),
    title: 'Jogadores livres: pedidos e convites · Futebol Amador',
  },

  // Jogos
  {
    path: 'team/matchInvites',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () => import('./components/pages/match-invites/match-invites').then((m) => m.MatchInvites),
    title: 'Convites de jogo · Futebol Amador',
  },
  {
    // `teamId` é a equipa convidada; `?convite=` indica que é uma contraproposta a um convite.
    path: 'team/createMatchInvite/:teamId',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/create-match-invite/create-match-invite').then((m) => m.CreateMatchInvite),
    title: 'Convidar para jogo · Futebol Amador',
  },
  {
    path: 'players/calendar/:idTeam',
    canActivate: [authGuard],
    data: { requiresTeam: true },
    loadComponent: () =>
      import('./components/pages/calendar/calendar.component').then((m) => m.CalendarComponent),
    title: 'Calendário · Futebol Amador',
  },
  {
    path: 'players/calendar/:idTeam/postpone-requests',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/calendar/postpone-requests/postpone-requests.component').then(
        (m) => m.PostponeRequestsComponent
      ),
    title: 'Pedidos de adiamento · Futebol Amador',
  },
  {
    path: 'players/calendar/:idTeam/postpone/:idMatch',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/calendar/postpone-match/postpone-match.component').then(
        (m) => m.PostponeMatchComponent
      ),
    title: 'Pedir adiamento · Futebol Amador',
  },
  {
    path: 'players/calendar/:idTeam/cancel-match/:idMatch',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () =>
      import('./components/pages/calendar/cancel-match/cancel-match.component').then(
        (m) => m.CancelMatchComponent
      ),
    title: 'Cancelar jogo · Futebol Amador',
  },

  {
    path: 'jogos/:matchId',
    canActivate: [authGuard],
    loadComponent: () => import('./components/pages/jogo/relatorio/relatorio').then((m) => m.Relatorio),
    title: 'Jogo · Futebol Amador',
  },
  {
    path: 'jogos/:matchId/onze',
    canActivate: [authGuard],
    data: { requiresTeam: true },
    loadComponent: () => import('./components/pages/jogo/onze/onze').then((m) => m.Onze),
    title: 'Onze inicial · Futebol Amador',
  },
  {
    path: 'jogos/:matchId/resultado',
    canActivate: [authGuard],
    data: { isAdmin: true },
    loadComponent: () => import('./components/pages/jogo/resultado/resultado').then((m) => m.Resultado),
    title: 'Resultado do jogo · Futebol Amador',
  },

  { path: '**', redirectTo: '' },
];
