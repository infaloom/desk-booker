import { Component, Input, NgZone, OnInit } from '@angular/core';
import { format, parse, addDays } from 'date-fns';
import { AuthService } from 'src/app/services/auth.service';
import { ReservationService } from 'src/app/services/reservation.service';
import { ToastService } from 'src/app/services/toast.service';
import { TableCellData } from 'src/app/interfaces/tableCell';

@Component({
  selector: 'app-table-cell',
  templateUrl: './table-cell.component.html',
  styleUrls: ['./table-cell.component.css']
})
export class TableCellComponent implements OnInit{
  loggedInUserId: string | null = null;
  loggedInUserRole: string | null = null;
  loggedInUserName: string | null = null;
  isUserAdmin: boolean = false;
  showForm: boolean = false;
  showReserveForm: boolean = false;
  myReservation: boolean = false;
  deskName: string = "";
  startDate: string = "";
  endDate: string = "";
  link: string = "";
  assignedReservation: boolean = false;
  myAssignedReservation: boolean = false;

  assignedUsername: string = "";

  // protected _dateDeskInfo!: TableCellData;
  // protected _newInfo?: TableCellData;
  // get dateDeskInfo(): TableCellData {
  //   return this._newInfo || this._dateDeskInfo;
  // }
  // @Input() set dateDeskInfo(v: TableCellData) {
  //   this._dateDeskInfo = v;
  // }

  @Input() dateDeskInfo!: TableCellData;

  constructor(private authService: AuthService, private reservationService: ReservationService, 
    private toastService: ToastService) {}

    async ngOnInit() {
      this.startDate = format(parse(this.dateDeskInfo.date, 'EEE MMM dd yyyy', new Date()), 'yyyyMMdd');
      this.endDate = format(addDays(parse(this.dateDeskInfo.date, 'EEE MMM dd yyyy', new Date()), 1), 'yyyyMMdd');
      this.link = `http://www.google.com/calendar/event?action=TEMPLATE&dates=${this.startDate}/${this.endDate}&text=Desk%20Reservation&location=Office&details=See%20ya%20in%20office!`;  
      
      this.deskName = this.dateDeskInfo.desk.name;    
      this.loggedInUserId = this.authService.getLoggedInUserId();
      this.loggedInUserRole = this.authService.getLoggedInUserRole();
      this.loggedInUserName = this.authService.getLoggedInUser();
      this.isUserAdmin = this.loggedInUserRole === 'Admin';

      if(this.dateDeskInfo.reservation?.userDTO?.userId === this.loggedInUserId && this.dateDeskInfo.reservation?.type == null){
        this.myReservation = true;
      }

      if(!this.myReservation && this.dateDeskInfo.desk.userId && this.dateDeskInfo.reservation == undefined){
        this.assignedReservation = true;
      }

      if(this.authService.getLoggedInUserId() === this.dateDeskInfo.desk.userId && this.assignedReservation){
        this.myAssignedReservation = true;
      }

      if(this.assignedReservation && !this.myAssignedReservation){
        this.assignedUsername = this.dateDeskInfo.desk.username as string;
      }
    }  

  handleXClick(){
    if((this.dateDeskInfo.reservation!.id && this.dateDeskInfo.reservation!.userDTO.userId === this.loggedInUserId) || this.isUserAdmin){
      this.showForm = true;
    } 
  }

  handleFreeClick(){
    this.reservationService.createReservation({userDTO: {userId: this.loggedInUserId as string, userName: this.loggedInUserName as string}, deskId: this.dateDeskInfo.desk.id, type: 'revoke', date: format(parse(this.dateDeskInfo.date, 'EEE MMM dd yyyy', new Date()), 'yyyy-MM-dd')}).subscribe(
      response => {
        this.toastService.show('You have successfully cancelled the reservation!', { classname: 'bg-success text-light', delay: 4000 });
        this.assignedReservation = false;
        this.myAssignedReservation = false;
        this.showReserveForm = false;
        this.myReservation = false;
      },
      error => {
        this.toastService.show('Reservation failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    );
  }

  handleCellClick() {
    if (!this.dateDeskInfo.reservation || this.dateDeskInfo.reservation.type === 'revoke') {
      this.showReserveForm = true;
    }
  }

  createReservation(){
    this.reservationService.createReservation({userDTO: {userId: this.loggedInUserId as string, userName: this.loggedInUserName as string}, deskId: this.dateDeskInfo.desk.id, type: null, date: format(parse(this.dateDeskInfo.date, 'EEE MMM dd yyyy', new Date()), 'yyyy-MM-dd')}).subscribe(
      response => {
        this.toastService.show('You have successfully reserved a desk!', { classname: 'bg-success text-light', delay: 4000 });
        this.showReserveForm = false;
        this.myReservation = true;
      },
      error => {
        this.toastService.show('Reservation failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    );
  }

  cancelReservation() {
    // if(confirm('Are you sure?')){
    //   console.log('cancel');
    // }

    this.reservationService.deleteReservation(this.dateDeskInfo.reservation!.id as number).subscribe({
      next: () => {
        this.toastService.show('You have successfully cancelled a reservation!', { classname: 'bg-success text-light', delay: 4000 });
        this.myReservation = false;
      },
      error: () => {
        this.toastService.show('Cancellation failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    });
    this.showForm = false;
  }

  handleOverlayClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (target.classList.contains('overlay')) {
      this.showForm = false;
      this.showReserveForm = false;
    }
  }
  
  handleFormClick(event: MouseEvent) {
    event.stopPropagation();
  }
}
