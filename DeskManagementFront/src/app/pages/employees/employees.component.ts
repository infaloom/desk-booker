import { Component } from '@angular/core';
import { Employee } from 'src/app/interfaces/employee.interface'; 
import { AuthService } from 'src/app/services/auth.service';
import { EmployeeService } from 'src/app/services/employee.service'; 
import { ToastService } from 'src/app/services/toast.service';

@Component({
  selector: 'app-employees',
  templateUrl: './employees.component.html',
  styleUrls: ['./employees.component.css']
})
export class EmployeesComponent {
  loggedInUserRole: string | null = null;
  employees: Employee[] = [];
  showForm: boolean = false;

  constructor(private authService: AuthService, private employeeService: EmployeeService, private toastService: ToastService) {}

  ngOnInit() {
    this.loggedInUserRole = this.authService.getLoggedInUserRole();

    this.employeeService.getAllEmployees().subscribe(
      (employees) => { 
        this.employees = employees; 
      },
      (error: any) => {
        console.error('Error fetching desks: ', error);
      }
    )
    ;
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
