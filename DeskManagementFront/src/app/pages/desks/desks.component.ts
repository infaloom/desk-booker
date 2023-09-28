import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { DeskService } from '../../services/desk.service';
import { Desk } from '../../interfaces/desk.interface';
import { ToastService } from 'src/app/services/toast.service';

@Component({
  selector: 'app-desks',
  templateUrl: './desks.component.html',
  styleUrls: ['./desks.component.css']
})
export class DesksComponent {
  loggedInUserRole: string | null = null;
  desks: Desk[] = [];
  showForm: boolean = false;
  deskName: string = '';
  deskFloor: number = 1;
  nameValidation: boolean = false;
  floorValidation: boolean = false;

  constructor(private authService: AuthService, private deskService: DeskService, private toastService: ToastService) {}

  ngOnInit() {
    this.loggedInUserRole = this.authService.getLoggedInUserRole();

    this.deskService.getAllDesks().subscribe(
      (desks) => { 
        this.desks = desks;
      },
      (error: any) => {
        console.error('Error fetching desks: ', error);
      }
    )
    ;
  }

  handleAddClick() {
    this.showForm = true;
  }

  createDesk() {
    this.nameValidation = false;
    this.floorValidation = false;
    const deskName = this.deskName;
    const deskFloor = this.deskFloor;

    if (deskName.length < 3 || deskName.length > 18) {
      this.nameValidation = true;
      return;
    }

    if (deskFloor < 0 || deskFloor > 1) {
      this.floorValidation = true;
      return;
    }

    this.deskService.createDesk(deskName, deskFloor).subscribe({
      next: () => {
        this.toastService.show('You have successfully added a desk!', { classname: 'bg-success text-light', delay: 4000 });
      },
      error: () => {
        this.toastService.show('Adding new desk failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    });

    this.deskName = '';
    this.deskFloor = 1;
    this.showForm = false;
    this.nameValidation = false;
    this.floorValidation = false;
  }

  handleOverlayClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (target.classList.contains('overlay')) {
      this.showForm = false;
      this.nameValidation = false;
      this.floorValidation = false;
      this.deskName = '';
      this.deskFloor = 1;
    }
  }
  
  handleFormClick(event: MouseEvent) {
    event.stopPropagation();
  }
}
