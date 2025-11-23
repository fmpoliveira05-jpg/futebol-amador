import { Routes } from '@angular/router';
import { Home } from './components/pages/home/home';
import { PlayerListPageComponent } from './components/pages/player-list/player-list-page.component';
import { PlayerProfilePageComponent } from './components/pages/player-profile/player-profile-page.component';
import { SettingsPageComponent } from './components/pages/settings-page/settings-page.component';
import { PlayerProfileRedirectComponent } from './components/pages/player-profile/player-profile-redirect/player-profile-redirect.component';
import { LoginComponent } from './components/pages/login/login.component';
import { SignupComponent } from './components/pages/signup/signup.component';
import { LogoutComponent } from './components/pages/login/logout.component';
import { AuthGuard } from './shared/auth/auth.guard';
import { PlayerMembershipRequestsPageComponent } from './components/pages/membership-requests/player-membership-requests.component';


export const routes: Routes = [
    {path: '', component: Home},
    // {path: '', component: Team},
    // {path: '', component: Profile},
    // {path: 'login', component: Login},
    // {path: 'register', component: Register},
    {
    path: 'signup',
    component: SignupComponent,
  },
  {
    path: 'login',
    component: LoginComponent,
  },
  {
    path: 'logout',
    component: LogoutComponent,
    canActivate: [AuthGuard]
  },
  {
    path: 'players',
    component: PlayerListPageComponent,
    canActivate: [AuthGuard],
    data: { isAdmin: true }
  },
  {
    path: 'players/me',
    component: PlayerProfileRedirectComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'players/details/:playerId',
    component: PlayerProfilePageComponent,
    canActivate: [AuthGuard],
  },
  {
    path: 'players/membership-requests',
    component: PlayerMembershipRequestsPageComponent,
    canActivate: [AuthGuard],
    data: { requiresNoTeam: true },
  },
  {
    path: 'settings',
    component: SettingsPageComponent,
  },
  {
    path: '**',
    redirectTo: 'players',
  },
];