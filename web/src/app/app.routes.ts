import { Routes } from '@angular/router';
import { Home } from './components/pages/home/home';
import { Teams } from './components/pages/teams/teams';

export const routes: Routes = [
    {path: '', component: Home},
    {path: 'teams', component: Teams},
    {path: 'search/:searchTerm', component: Teams},
    // {path: '', component: Profile},
    // {path: 'login', component: Login},
    // {path: 'register', component: Register},
];