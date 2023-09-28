import { Component, Input, OnInit } from '@angular/core';
import { Desk } from '../../interfaces/desk.interface';
import { DeskService } from 'src/app/services/desk.service';
import { ToastService } from 'src/app/services/toast.service';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-desk',
  templateUrl: './desk.component.html',
  styleUrls: ['./desk.component.css']
})
export class DeskComponent implements OnInit {
  isShown: boolean = true;
  showForm: boolean = false;
  deskName: string = '';
  deskFloor: number = 1;
  username: string = '';

  @Input() desk!: Desk;

  constructor( private deskService: DeskService, private toastService: ToastService, private authService: AuthService, private router: Router) {}

  ngOnInit(): void {
    if(this.desk.username){
      this.username = this.desk.username;
    }
    else {
      this.username = 'Not assigned';
    }  
  }

  handleDeleteClick(){
    this.showForm = true;
  }

  handleEditClick(){
    this.router.navigate(['/desks', this.desk.id, this.desk.name, this.desk.floor]);
  }

  handleAssignClick(){
    this.router.navigate(['/desks', this.desk.id, this.desk.name]);
  }

  deleteDesk() {
    this.deskService.deleteDesk(this.desk.id).subscribe({
      next: () => {
        this.toastService.show('You have successfully deleted a desk!', { classname: 'bg-success text-light', delay: 4000 });
      },
      error: () => {
        this.toastService.show('Deleting a desk failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    });
    this.isShown = false;
  }

  handleOverlayClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (target.classList.contains('overlay')) {
      this.showForm = false;
    }
  }
  
  handleFormClick(event: MouseEvent) {
    event.stopPropagation();
  }
  
}
