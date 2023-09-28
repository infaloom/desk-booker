import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private backendUrl = '/api/account/';
  private isAuthenticated: boolean = false;
  private localStorageToken = 'authToken';
  private localStorageUserName = 'authUserName';
  private localStorageRole = 'authRole';
  private localStorageUserId = 'authUserId';
  usernameChanged: EventEmitter<string | null> = new EventEmitter<string | null>();
  roleChanged: EventEmitter<string | null> = new EventEmitter<string | null>();

  constructor(private http: HttpClient) {
    const authData = localStorage.getItem(this.localStorageToken);
    if (authData) {
      this.isAuthenticated = true;
    }
  }

  setIsAuthenticated(value: boolean) {
    this.isAuthenticated = value;
  }

  getIsAuthenticated(): boolean  {
    const localStorageValue = localStorage.getItem(this.localStorageToken);
    return (this.isAuthenticated || localStorageValue) as boolean;
  }

  getAuthenticationToken(){
    return localStorage.getItem(this.localStorageToken);
  }

  login(UserName: string, Password: string): Observable<boolean> {
    const body = { UserName, Password };
  
    return this.http.post(this.backendUrl+'login', body, { responseType: 'json' }).pipe(
      map((response: any) => {
        const {token, user} = response;
        if (token && user) {
          localStorage.setItem(this.localStorageToken, token);
          localStorage.setItem(this.localStorageUserName, user.userName);
          localStorage.setItem(this.localStorageUserId, user.id);
          localStorage.setItem(this.localStorageRole, user.role);
          this.setIsAuthenticated(true);
          this.usernameChanged.emit(user.userName);
          this.roleChanged.emit(user.role);
          return true;
        } else {
          return false;
        }
      }),
      catchError(() => of(false))
    );
  }

  forgotPassword(username: string): Observable<boolean>{
    return this.http.post(this.backendUrl+'forgot', {username}, { responseType: 'json' }).pipe(
      map((response: any) => {
        if (response) {
          return true;
        } else {
          return false;
        }
      }),
      catchError(() => of(false))
    );
  }

  resetPassword(password: string, passwordConfirm: string, userId: string, token: string): Observable<boolean>{
    const body = {Password: password, ConfirmPassword: passwordConfirm, UserId: userId, Token: token};
    return this.http.post(this.backendUrl+'reset', body, { responseType: 'json' }).pipe(
      map((response: any) => {
        if (response) {
          return true;
        } else {
          return false;
        }
      }),
      catchError(() => of(false))
    );  
  }

  getLoggedInUser(): string | null {
    return localStorage.getItem(this.localStorageUserName);
  }
  getLoggedInUserRole(): string | null {
    return localStorage.getItem(this.localStorageRole);
  }
  getLoggedInUserId(): string | null {
    return localStorage.getItem(this.localStorageUserId);
  }

  getUsernameById(id: string): Observable<any>{
    return this.http.get(`api/account/${id}`);
  }

  logout(): void {
    localStorage.removeItem(this.localStorageToken);
    localStorage.removeItem(this.localStorageUserName);
    localStorage.removeItem(this.localStorageUserId);
    localStorage.removeItem(this.localStorageRole);
    this.isAuthenticated = false;
    this.usernameChanged.emit(null); 
    this.roleChanged.emit(null);
  }
}
