import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfigurationService {
  private configUrl = '/config';

  constructor(private http: HttpClient) { }

  configure(userData: any) {
    return this.http.post(this.configUrl+'/create', userData);
  }

  getConfigurationStatus(): Observable<boolean> {
    return this.http.get<boolean>(this.configUrl+'/status');
  }
}
