import { Component } from '@angular/core';
import { ConfigurationService } from 'src/app/services/configuration.service'; 
import { Router } from '@angular/router';

@Component({
  selector: 'app-configuration',
  templateUrl: './configuration.component.html',
  styleUrls: ['./configuration.component.css']
})
export class ConfigurationComponent {
  UserName: string = "";
  Email: string = "";
  Password: string = "";
  ConfirmPassword: string = "";
  invalidRegistration: boolean = false;
  message: string = "";
  created: boolean = false;

  constructor(private configurationService: ConfigurationService, private router: Router) {}

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

    this.configurationService.configure(userData)
      .subscribe({
        next: () => {
           this.created = true;
        },
        error: () => {
          this.message = 'Invalid register attempt';
          this.invalidRegistration = true;
        }
      });
  }
}

