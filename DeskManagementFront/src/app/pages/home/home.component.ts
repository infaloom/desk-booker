import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { ReservationService } from '../../services/reservation.service';
import { Reservation } from '../../interfaces/reservation.interface';
import { DeskService } from 'src/app/services/desk.service';
import { Desk } from 'src/app/interfaces/desk.interface';
import { ToastService } from 'src/app/services/toast.service';
import { isWeekend, format} from 'date-fns';
import { combineLatest } from 'rxjs';
import { CellStyle } from 'src/app/interfaces/cellStyle';
import { TableCellData } from 'src/app/interfaces/tableCell';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})

export class HomeComponent  implements OnInit {
  loggedInUser: string | null = null;
  loggedInUserId: string | null = null;
  reservations: Reservation[] = [];
  allReservations: Reservation[] = [];
  allDesks: Desk[] = [];
  desks: Desk[] = [];
  dates: string[] = [];
  showForm: boolean = false;
  floor: number = 1;
  filtered: boolean = false;
  numWeeks: any = 1;
  displayedColumns: string[] = [];
  tableRows: any[] = [];
  reservationId: number | null = null;
  resUsername: string | undefined = undefined;

  firstFloorFilteredDesks: Desk[] = [];
  firstFloorAllDesks: Desk[] = [];
  secondFloorFilteredDesks: Desk[] = [];
  secondFloorAllDesks: Desk[] = [];

  constructor(private authService: AuthService, private reservationService: ReservationService, 
    private deskService: DeskService) {}

  ngOnInit() {
    this.loggedInUser = this.authService.getLoggedInUser();
    this.loggedInUserId = this.authService.getLoggedInUserId();

    if(this.authService.getIsAuthenticated()){      
      const allDesks$ = this.deskService.desks;
      const allReservations$ = this.reservationService.reservations;
      combineLatest([allReservations$, allDesks$]).subscribe(
        ([allReservations, allDesks]) => {
          this.reservations = allReservations.filter(reservation => reservation?.userDTO?.userId === this.loggedInUserId);
          this.allReservations = allReservations;
          this.allDesks = allDesks;
          this.firstFloorFilteredDesks = this.allDesks.filter((desk) => desk.userId === null && desk.floor === 0);
          this.firstFloorAllDesks = this.allDesks.filter((desk) => desk.floor === 0);
          this.secondFloorFilteredDesks = this.allDesks.filter((desk) => desk.userId === null && desk.floor === 1);
          this.secondFloorAllDesks = this.allDesks.filter((desk) => desk.floor === 1);
          this.dates = this.getNextWorkingDays(this.numWeeks * 5);
          this.calculateDesks();
        },
        (error: any) => {
          console.error('Error fetching data: ', error);
        }
      );
    }
    else{
      this.loggedInUser = null;
    }
  }

  generateTable() {
    this.tableRows = [];
    var deskNames = this.desks.map(desk => desk.name);
    this.displayedColumns = ['Date', ...deskNames];
    this.dates.forEach((date: string) => {
  
      var e: any = {};
      e.date = date;
  
      this.allDesks.forEach((desk: Desk) => {
        e[desk.name] = {deskId: desk.id, date: date};
      });
  
      this.tableRows.push(e);
    });
  }

  handleSelectClick(){
    if(this.numWeeks === 'Number of weeks shown in table')
      return;
    if(this.numWeeks*5 !== this.dates.length){
      this.dates = this.getNextWorkingDays(this.numWeeks * 5);
      this.generateTable();
    }  
  }

  getNextWorkingDays(numDays: number) {
    const today = new Date();
    let count = 0;
    const workingDays = [];
    
    workingDays.push(format(today, 'EEE MMM dd yyyy'));
    count++;

    while (count < numDays) {
      today.setDate(today.getDate() + 1);
      if (!isWeekend(today)) {
        workingDays.push(format(today, 'EEE MMM dd yyyy'));
        count++;
      }
    }  
    return workingDays;
  }

  handleFirstFloor(){
    this.floor = 0;
    this.calculateDesks();
    // if(this.filtered){
    //   this.desks = this.firstFloorFilteredDesks;
    // }
    // else{
    //   this.desks = this.firstFloorAllDesks;
    // }
    // if(this.desks.length)
    //   this.generateTable(); 
  }

  handleSecondFloor(){
    this.floor = 1;
    this.calculateDesks();
    // if(this.filtered){
    //   this.desks = this.secondFloorFilteredDesks;
    // }
    // else{
    //   this.desks = this.secondFloorAllDesks;
    // }
    // if(this.desks.length)
    //   this.generateTable(); 
  }

  handleAllDesksClick() {
    this.filtered = false;
    this.calculateDesks();
    // if(this.floor === 0){
    //   this.desks = this.firstFloorAllDesks;
    // }
    // if(this.floor === 1){
    //   this.desks = this.secondFloorAllDesks;
    // }
    // if(this.desks.length)
    //   this.generateTable(); 
  }

  handleFilterDesksClick() {
    // this.filtered = true;
    // if(this.floor === 0){
    //   this.desks = this.firstFloorFilteredDesks;
    // }
    // if(this.floor === 1){
    //   this.desks = this.secondFloorFilteredDesks;
    // }
    // if(this.desks.length)
    //   this.generateTable(); 
    this.filtered = true;
    this.calculateDesks();
  }

  calculateDesks() {
    if(this.floor === 0 && this.filtered){
      this.desks = this.firstFloorFilteredDesks;
    }
    else if(this.floor === 1 && this.filtered){
      this.desks = this.secondFloorFilteredDesks;
    }
    else if(this.floor === 0 && !this.filtered){
      this.desks = this.firstFloorAllDesks;
    }
    else if(this.floor === 1 && !this.filtered){
      this.desks = this.secondFloorAllDesks;
    }
    this.generateTable();
  }

  getDateDeskInfo(desk: Desk, date: string): TableCellData {
    const duplicateReservations = this.allReservations.filter(r => r.deskId === desk.id && format(new Date(r.date), 'EEE MMM dd yyyy') === date);
    const reservationWithNullType = duplicateReservations.find(r => r.type == null);
    const reservationWithRevokeType = duplicateReservations.find(r => r.type === 'revoke');
    const cellStyle = this.calculateCellStyle(desk, date, reservationWithNullType);
    if (reservationWithNullType) {
      const reservation = reservationWithNullType;
      return {desk: desk, date: date, reservation: reservation, resUsername: reservation?.userDTO?.userName, cellStyle: cellStyle}
    }
    else if(reservationWithRevokeType){
      const reservation = reservationWithRevokeType;
      return {desk: desk, date: date, reservation: reservation, resUsername: '', cellStyle: cellStyle};
    }
    else{
      return {desk: desk, date: date, reservation: undefined, resUsername: '', cellStyle: cellStyle};
    }
  }

  calculateCellStyle(desk: Desk, date: string, resNullType: Reservation | undefined): CellStyle {
    if(resNullType){
      if(resNullType?.userDTO?.userId === this.loggedInUserId){
        return {'backgroundColor' : 'green'};
      } else {
        return {'backgroundColor' : 'red'};
      } 
    }
    else {
      const res2 = this.allReservations.find(res => format(new Date(res.date), 'EEE MMM dd yyyy') === date && res?.userDTO?.userId === this.loggedInUserId && res.type == null);
      const desk = this.allDesks.find(d => d?.userId === this.loggedInUserId);
      const res3 = this.allReservations.find(res => format(new Date(res.date), 'EEE MMM dd yyyy') === date && res?.userDTO?.userId === this.loggedInUserId && res.type == 'revoke' && desk?.id === res.deskId);
      if(res2 || (desk && !res3)){
        return {'pointer-events': 'none'};  
      }
      else{
        return {'pointer-events': 'auto'};
      }
    }  
  }
}


