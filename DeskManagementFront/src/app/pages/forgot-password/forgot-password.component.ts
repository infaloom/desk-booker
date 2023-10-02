import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css']
})
export class ForgotPasswordComponent {
  email: string = "";
  message: string = "";
  usernameSent: boolean = false;

  constructor(private authService: AuthService, private router: Router) {}

  forgot(){
    if(!this.email.length){
      this.message = 'User name must not be empty'
      return;
    }

    this.authService.forgotPassword(this.email).subscribe(result => {
      this.usernameSent = true;
    });
  }
}
