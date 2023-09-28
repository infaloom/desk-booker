import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Desk } from 'src/app/interfaces/desk.interface';
import { Employee } from 'src/app/interfaces/employee.interface';
import { DeskService } from 'src/app/services/desk.service';
import { EmployeeService } from 'src/app/services/employee.service';
import { ToastService } from 'src/app/services/toast.service';

@Component({
  selector: 'app-assign-desk',
  templateUrl: './assign-desk.component.html',
  styleUrls: ['./assign-desk.component.css']
})
export class AssignDeskComponent implements OnInit {
  showForm: boolean = true;
  deskId: number = 0;
  deskName: string = "";
  deskFloor: number = 0;
  userId: string = "";
  desk: Desk = {id : 0, name : "", floor : 0};
  employees: Employee[] = [];

  constructor( private deskService: DeskService, 
    private route: ActivatedRoute, 
    private toastService: ToastService, 
    private router: Router,
    private employeeService: EmployeeService) {}
  
  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.deskId = params['deskId'];
      this.deskName = params['name'];
      // this.deskService.getDeskById(this.deskId).subscribe(
      //   response => {
      //     this.desk = response;
      //   }
      // );
      this.employeeService.getAllEmployees().subscribe(
        (employees) => { 
          this.employees = employees; 
        },
        (error: any) => {
          console.error('Error fetching desks: ', error);
        }
      )
      ;
    });
  }

  assignDesk() {
    this.deskService.assignDesk(this.deskId, this.userId).subscribe({
      next: () => {
        this.toastService.show('You have successfully assigned the desk!', { classname: 'bg-success text-light', delay: 4000 });

      },
      error: () => {
        this.toastService.show('Assigning the desk failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
      }
    });
    this.showForm = false;
    this.router.navigate(['/desks']);
  }

  handleOverlayClick(event: MouseEvent) {
    const target = event.target as HTMLElement;
    if (target.classList.contains('overlay')) {
      this.deskFloor = 1;
      this.deskName = '';
      this.showForm = false;
      this.router.navigate(['/desks']);
    }
  }
  
  handleFormClick(event: MouseEvent) {
    event.stopPropagation();
  }
}
