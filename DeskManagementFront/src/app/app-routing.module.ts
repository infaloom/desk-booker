import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RegisterConfirmationComponent } from './pages/register-confirmation/register-confirmation.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterComponent } from './pages/register/register.component';
import { LogoutComponent } from './pages/logout/logout.component';
import { HomeComponent } from './pages/home/home.component';
import { Page404Component } from './pages/page404/page404.component';
import { DesksComponent } from './pages/desks/desks.component';
import { EditDeskComponent } from './pages/edit-desk/edit-desk.component';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';
import { EmployeesComponent } from './pages/employees/employees.component';
import { AssignDeskComponent } from './pages/assign-desk/assign-desk.component';

const routes: Routes = [
  { path: 'account-confirmation/:userId/:code', component: RegisterConfirmationComponent },
  { path: 'register', component: RegisterComponent},
  { path: 'login', component: LoginComponent},
  { path: 'logout', component: LogoutComponent },
  { path: 'forgot', component: ForgotPasswordComponent},
  { path: 'reset', component: ResetPasswordComponent },
  { path: 'desks', component: DesksComponent, children: [{ path: ':deskId/:name/:floor', component: EditDeskComponent }, { path: ':deskId/:name', component: AssignDeskComponent }] },
  { path: 'employees', component: EmployeesComponent},
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: '**', component: Page404Component }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
