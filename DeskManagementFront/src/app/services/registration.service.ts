import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class RegistrationService {
  private registrationUrl = '/api/account/create';

  constructor(private http: HttpClient) { }

  register(userData: any) {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    return this.http.post(this.registrationUrl, userData, { headers });
  }
}

