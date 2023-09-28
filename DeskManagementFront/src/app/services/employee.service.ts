import { Injectable, OnDestroy } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from './auth.service';
import { Employee } from '../interfaces/employee.interface'; 
import { Observable, catchError, map, of, BehaviorSubject } from 'rxjs';
import * as signalR from "@microsoft/signalr";

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {
  private backendUrl = '/api/employees';
  private employeesSubject: BehaviorSubject<Employee[]> = new BehaviorSubject<Employee[]>([]);
  employees = this.employeesSubject.asObservable();
  private isAuthenticated: boolean = false;

  private connection: signalR.HubConnection | undefined = undefined;

  constructor(private http: HttpClient, private authService: AuthService) {
    this.isAuthenticated = this.authService.getIsAuthenticated();
    if(this.isAuthenticated){
      this.loadEmployees();
    }

    this.connection = new signalR.HubConnectionBuilder()
    .withUrl("/api/employeeHub")
    .build();

    this.connection.on("EmployeeNotify", (obj: any) => {
      if(obj.action === "create"){
        const currentEmployees = this.employeesSubject.getValue();
        this.employeesSubject.next([...currentEmployees, obj.employeeDTO]);
      }
      else if(obj.action === "delete"){
        const currentEmployees = this.employeesSubject.getValue();
        const updatedEmployees = currentEmployees.filter(employee => employee.id !== obj.employeeDTO.id);
        this.employeesSubject.next(updatedEmployees);
      }
      else if(obj.action === "update"){
        const currentEmployees = this.employeesSubject.getValue();
        const updatedEmployees = currentEmployees.map(employee => {
          if (employee.id === obj.deskDTO.id) {
            return obj.deskDTO;
          }
          return employee;
        });
        this.employeesSubject.next(updatedEmployees);
      }
    });

    this.connection.start().catch((err) => document.write(err));

    this.authService.usernameChanged.subscribe((username: string | null) => {
      if(username){
        this.isAuthenticated = true;
        if(!this.employeesSubject.getValue().length){
          this.loadEmployees();
        }
      }
      else{
        this.isAuthenticated = false;
      }
    });
  }

  private loadEmployees(): void {
    this.http.get<Employee[]>(this.backendUrl).subscribe(employees => {
      this.employeesSubject.next(employees);
    });
  }

  getAllEmployees(): Observable<Employee[]> {
    return this.employees;
  }

  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.backendUrl}/${id}`);
  }

  deleteEmployee(employeeId: number): Observable<boolean> {
    return this.http.delete(`${this.backendUrl}/${employeeId}`).pipe(
      map(() => true),
      catchError(() => of(false))
    );
  }
  
  updateEmployeeRole(employeeId: number, newRole: string): Observable<any> {  
    return this.http.put(`${this.backendUrl}/${employeeId}`, { role: newRole}).pipe(
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
