import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  
  
  getAll(): Team{
    return teams;
  }
}
