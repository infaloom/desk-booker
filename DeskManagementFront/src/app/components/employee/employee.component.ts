import { Component, Input } from '@angular/core';
import { Router } from '@angular/router';
import { Employee } from 'src/app/interfaces/employee.interface';
import { EmployeeService } from 'src/app/services/employee.service'; 
import { ToastService } from 'src/app/services/toast.service';

@Component({
  selector: 'app-employee',
  templateUrl: './employee.component.html',
  styleUrls: ['./employee.component.css']
})
export class EmployeeComponent {
  isShown: boolean = true;
  showForm: boolean = false;
  isDeskAdmin: boolean = false;
  employeeName: string = '';

  @Input() employee!: Employee;

  constructor( private employeeService: EmployeeService, private toastService: ToastService, private router: Router) {}

  handleDeleteClick(){
    this.showForm = true;
  }

  handleToggleClick(){
    if(this.employee.role !== 'DeskAdmin'){
      this.employeeService.updateEmployeeRole(this.employee.id, "DeskAdmin").subscribe({
        next: () => {
          this.toastService.show('You have successfully assigned the user as Desk Admin!', { classname: 'bg-success text-light', delay: 4000 });
          this.employee.role = 'DeskAdmin';
        },
        error: () => {
          this.toastService.show('Assigning the user as DeskAdmin failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
        }
      });
    }
    else{
      this.employeeService.updateEmployeeRole(this.employee.id, "User").subscribe({
        next: () => {
          this.toastService.show('You have successfully removed the Desk Admin position from the user.', { classname: 'bg-success text-light', delay: 4000 });
          this.employee.role = 'User';
        },
        error: () => {
          this.toastService.show('Removing the DeskAdmin from the user as DeskAdmin failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
        }
      });
    }
  }

  deleteEmployee() {
    this.employeeService.deleteEmployee(this.employee.id).subscribe({
      next: () => {
        this.toastService.show('You have successfully deleted an employee!', { classname: 'bg-success text-light', delay: 4000 });
      },
      error: () => {
        this.toastService.show('Deleting an employee failed. Internal Server Error!', { classname: 'bg-danger text-light', delay: 5000 });
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
