import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Desk } from 'src/app/interfaces/desk.interface';
import { DeskService } from 'src/app/services/desk.service';
import { ToastService } from 'src/app/services/toast.service';

@Component({
  selector: 'app-edit-desk',
  templateUrl: './edit-desk.component.html',
  styleUrls: ['./edit-desk.component.css']
})
export class EditDeskComponent implements OnInit {
  showForm: boolean = true;
  deskId: number = 0;
  deskName: string = "";
  deskFloor: number = 0;
  desk: Desk = {id : 0, name : "", floor : 0};
  nameValidation: boolean = false;
  floorValidation: boolean = false;

  constructor( private deskService: DeskService, private route: ActivatedRoute, private toastService: ToastService, private router: Router) {}

  ngOnInit() {
    this.route.params.subscribe(params => {
      this.deskId = params['deskId'];
      this.deskName = params['name'];
      this.deskFloor = params['floor'];
      this.deskService.getDeskById(this.deskId).subscribe(
        response => {
          this.desk = response;
        }
      );
    });
  }

  editDesk() {
    this.nameValidation = false;
    this.floorValidation = false;
    const deskName = this.deskName;
    const deskFloor = this.deskFloor;

    if (deskName.length < 3 || deskName.length > 18) {
      this.nameValidation = true;
      return;
    }

    if (deskFloor != 0 && deskFloor != 1) {
      this.floorValidation = true;
      return;
    }

    this.deskService.updateDeskName(this.desk.id, deskName, deskFloor).subscribe({
      next: () => {
        this.toastService.show('You have successfully edited a desk!', { classname: 'bg-success text-light', delay: 4000 });
      },
      error: () => {
        this.toastService.show('Editing desk failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    });

    this.deskFloor = 1;
    this.deskName = '';
    this.showForm = false;
    this.nameValidation = false;
    this.floorValidation = false;
    this.router.navigate(['/desks']);
  }

  handleOverlayClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (target.classList.contains('overlay')) {
      this.deskFloor = 1;
      this.deskName = '';
      this.showForm = false;
      this.nameValidation = false;
      this.floorValidation = false;
      this.router.navigate(['/desks']);
    }
  }
  
  handleFormClick(event: MouseEvent) {
    event.stopPropagation();
  }
}
