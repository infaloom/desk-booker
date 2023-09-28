import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Desk } from '../interfaces/desk.interface';
import { Observable, catchError, map, of, BehaviorSubject } from 'rxjs';
import * as signalR from "@microsoft/signalr";

@Injectable({
  providedIn: 'root'
})
export class DeskService implements OnDestroy {
  private backendUrl = '/api/desks';
  private desksSubject: BehaviorSubject<Desk[]> = new BehaviorSubject<Desk[]>([]);
  desks = this.desksSubject.asObservable();
  private isAuthenticated: boolean = false;

  private connection: signalR.HubConnection | undefined = undefined;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAuthenticated = this.authService.getIsAuthenticated();
    if(this.isAuthenticated){
      this.loadDesks();
    }

    this.connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/deskHub")
    .build();

    this.connection.on("DeskNotify", (obj: any) => {
      if(obj.action === "create"){
        const currentDesks = this.desksSubject.getValue();
        this.desksSubject.next([...currentDesks, obj.deskDTO]);
      }
      else if(obj.action === "delete"){
        const currentDesks = this.desksSubject.getValue();
        const updatedDesks = currentDesks.filter(desk => desk.id !== obj.deskDTO.id);
        this.desksSubject.next(updatedDesks);
      }
      else if(obj.action === "update"){
        const currentDesks = this.desksSubject.getValue();
        const updatedDesks = currentDesks.map(desk => {
          if (desk.id === obj.deskDTO.id) {
            return obj.deskDTO;
          }
          return desk;
        });
        this.desksSubject.next(updatedDesks);
      }
    });

    this.connection.start().catch((err) => document.write(err));

    this.authService.usernameChanged.subscribe((username: string | null) => {
      if(username){
        this.isAuthenticated = true;
        if(!this.desksSubject.getValue().length){
          this.loadDesks();
        }
      }
      else{
        this.isAuthenticated = false;
      }
    });
  }

  private loadDesks(): void {
    this.http.get<Desk[]>(this.backendUrl).subscribe(desks => {
      this.desksSubject.next(desks);
    });
  }

  getAllDesks(): Observable<Desk[]> {
    return this.desks;
  }

  getDeskById(id: number): Observable<Desk> {
    return this.http.get<Desk>(`${this.backendUrl}/${id}`);
  }

  createDesk(desk: string, floor: number): Observable<any> {
    return this.http.post(this.backendUrl, { Name: desk, Floor: floor });
  }
  

  deleteDesk(deskId: number): Observable<boolean> {
    return this.http.delete(`${this.backendUrl}/${deskId}`).pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }

  updateDeskName(deskId: number, newName: string, newFloor: number): Observable<any> {  
    return this.http.put(`${this.backendUrl}/${deskId}`, { Name: newName, Floor: newFloor }).pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }

  assignDesk(deskId: number, userId: string): Observable<any> {  
    return this.http.patch(`${this.backendUrl}/${deskId}`, { UserId: userId }).pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }
  
  ngOnDestroy(): void {
    if (this.connection) {
      this.connection.stop();
    }
  }
}