import { Component } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  email: string = "";
  password: string = "";
  message: string= "";
  invalidRegistration: boolean = false;

  constructor(private authService: AuthService, private router: Router) {}

  login() {

    if(!this.email.length){
      this.message = 'User name must not be empty'
      this.invalidRegistration = true;
      return;
    }

    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$/;
    if(!passwordRegex.test(this.password)){
      this.message = "The password must meet the following criteria: \nAt least one lowercase letter \nAt least one uppercase letter \nAt least one digit \nAt least one non-alphanumeric character \nMinimum length of 6 characters";
      this.invalidRegistration = true;
      return;
    }

    this.authService.login(this.email, this.password).subscribe(isAuthenticated => {
      if (isAuthenticated) {
        this.router.navigateByUrl('/');
      } else {
        this.invalidRegistration = true;
        this.message = "Invalid login attempt";
      }
    });
  }
}
