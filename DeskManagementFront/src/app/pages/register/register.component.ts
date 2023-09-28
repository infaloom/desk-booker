import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { RegistrationService } from '../../services/registration.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  UserName: string = "";
  Email: string = "";
  Password: string = "";
  ConfirmPassword: string = "";
  invalidRegistration: boolean = false;
  message: string = "";

  constructor(private registrationService: RegistrationService, private router: Router) {}

  submitForm() {
    if (this.Password !== this.ConfirmPassword) {
      this.message = 'Passwords do not match.';
      this.invalidRegistration = true;
      return;
    }

    const emailParts = this.Email.split('@');
    if (emailParts.length !== 2) {
      this.message = 'Email should have a single @ symbol separating the local part and domain part';
      this.invalidRegistration = true;
      return;
    }

    const domain = emailParts[1];
    if(domain !== 'infaloom.com'){
      this.message = "Invalid email address. Please make sure to use an email address with the 'infaloom.com' domain. Email addresses from other domains are not allowed.";
      this.invalidRegistration = true;
      return;
    };

    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$/;
    if(!passwordRegex.test(this.Password)){
      this.message = "The password must meet the following criteria: \nAt least one lowercase letter \nAt least one uppercase letter \nAt least one digit \nAt least one non-alphanumeric character \nMinimum length of 6 characters";
      this.invalidRegistration = true;
      return;
    }

    if(!this.UserName.length){
      this.message = 'User name must not be empty'
      this.invalidRegistration = true;
      return;
    }

    const userData = {
      UserName: this.UserName,
      Email: this.Email,
      Password: this.Password,
      ConfirmPassword: this.ConfirmPassword
    };

    this.registrationService.register(userData)
      .subscribe({
        next: () => {
          const email = encodeURIComponent(this.Email);
          const returnUrl = encodeURIComponent('/'); 
          this.router.navigate(['/account-confirmation/:userId/:code'], { queryParams: { email, returnUrl } });
        },
        error: () => {
          this.message = 'Invalid register attempt';
          this.invalidRegistration = true;
        }
      });
  }
}
