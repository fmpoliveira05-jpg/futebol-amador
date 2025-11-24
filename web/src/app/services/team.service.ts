import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class TeamService {
  
  private http = inject(HttpClient);
  private apiUrl = environment.apiBaseUrl + '/api/Team';

  public getById(id: string): Observable<any>{
    return this.http.get(`${this.apiUrl}/${id}`);
  }
}
