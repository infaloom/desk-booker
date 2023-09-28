import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { RegisterComponent } from './pages/register/register.component';
import { LoginComponent } from './pages/login/login.component';
import { RegisterConfirmationComponent } from './pages/register-confirmation/register-confirmation.component';
import { NavbarComponent } from './components/navbar/navbar.component';

import { RegistrationService } from './services/registration.service';
import { AuthService } from './services/auth.service';
import { DeskService } from './services/desk.service';
import { LogoutComponent } from './pages/logout/logout.component';
import { SplashScreenComponent } from './splash-screen/splash-screen.component';
import { HomeComponent } from './pages/home/home.component';
import { Page404Component } from './pages/page404/page404.component';
import { DeskComponent } from './components/desk/desk.component';
import { DesksComponent } from './pages/desks/desks.component';
import { TokenInterceptor } from './token.interceptor';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgbdToastGlobal } from './components/toast-global/toast-global.component';
import { ToastsContainer } from './components/toasts-container/toasts-container.component';
import { EditDeskComponent } from './pages/edit-desk/edit-desk.component';
import { TableCellComponent } from './components/table-cell/table-cell.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatTableModule } from '@angular/material/table';
import { StoreModule } from '@ngrx/store';
import { EffectsModule } from '@ngrx/effects';
import { ForgotPasswordComponent } from './pages/forgot-password/forgot-password.component';
import { ResetPasswordComponent } from './pages/reset-password/reset-password.component';
import { ConfigurationComponent } from './pages/configuration/configuration.component';
import { EmployeesComponent } from './pages/employees/employees.component';
import { EmployeeComponent } from './components/employee/employee.component';
import { AssignDeskComponent } from './pages/assign-desk/assign-desk.component';

@NgModule({
  declarations: [
    AppComponent,
    RegisterComponent,
    LoginComponent,
    RegisterConfirmationComponent,
    NavbarComponent,
    LogoutComponent,
    SplashScreenComponent,
    HomeComponent,
    Page404Component,
    DeskComponent,
    DesksComponent,
    EditDeskComponent,
    TableCellComponent,
    ForgotPasswordComponent,
    ResetPasswordComponent,
    ConfigurationComponent,
    EmployeesComponent,
    EmployeeComponent,
    AssignDeskComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,
    NgbModule,
    NgbdToastGlobal,
    ToastsContainer,
    BrowserAnimationsModule,
    MatTableModule,
    StoreModule.forRoot({}, {}),
    EffectsModule.forRoot([]),
  ],
  providers: [
    RegistrationService, 
    AuthService,
    DeskService,
    { provide: HTTP_INTERCEPTORS, useClass: TokenInterceptor, multi: true }
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
