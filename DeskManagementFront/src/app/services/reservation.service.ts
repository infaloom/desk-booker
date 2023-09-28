import { HttpClient } from '@angular/common/http';
import { Injectable, OnDestroy } from '@angular/core';
import { AuthService } from './auth.service';
import { Reservation } from '../interfaces/reservation.interface'; 
import { Observable, map, BehaviorSubject, catchError, of } from 'rxjs';
import * as signalR from "@microsoft/signalr";

@Injectable({
  providedIn: 'root'
})
export class ReservationService implements OnDestroy {
  private backendUrl = '/api/reservations';
  private reservationsSubject: BehaviorSubject<Reservation[]> = new BehaviorSubject<Reservation[]>([]);
  reservations = this.reservationsSubject.asObservable();
  private isAuthenticated: boolean = false;

  private connection: signalR.HubConnection | undefined = undefined;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAuthenticated = this.authService.getIsAuthenticated();
    if(this.isAuthenticated){
      this.loadReservations();
    }
  
    this.connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/reservationHub")
    .build();

    this.connection.on("ReservationNotify", (obj: any) => {
      if(obj.action === "create"){
        const currentReservations = this.reservationsSubject.getValue();
        this.reservationsSubject.next([...currentReservations, obj.reservationDTO]);
      }
      else if(obj.action === "delete"){
        const currentReservations = this.reservationsSubject.getValue();
        const updatedReservations = currentReservations.filter(reservation => reservation.id !== obj.reservationDTO.id);
        this.reservationsSubject.next(updatedReservations);
      }
    });

    this.connection.start().catch((err) => document.write(err));

    this.authService.usernameChanged.subscribe((username: string | null) => {
      if(username){
        this.isAuthenticated = true;
        if(!this.reservationsSubject.getValue().length){
          this.loadReservations();
        }
      }
      else{
        this.isAuthenticated = false;
      }
    });
  }

  private loadReservations(): void {
    this.http.get<Reservation[]>(this.backendUrl).subscribe(reservations => {
      this.reservationsSubject.next(reservations);
    });
  }
  getAllReservations(): Observable<Reservation[]> {
    return this.reservations;
  }

  getReservationById(id: number): Observable<Reservation> {
    return this.http.get<Reservation>(this.backendUrl);
  }
  
  getReservationsByUser(userId: string): Observable<Reservation[]> {
    const endpointUrl = `${this.backendUrl}/user/${userId}`;
    return this.http.get<Reservation[]>(endpointUrl);
  }

  createReservation(reservation: Reservation): Observable<any> {
    return this.http.post(this.backendUrl, reservation);
  }

  deleteReservation(reservationId: number): Observable<boolean> {
    return this.http.delete(`${this.backendUrl}/${reservationId}`).pipe(
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