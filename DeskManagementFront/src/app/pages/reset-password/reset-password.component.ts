import { Component } from '@angular/core';
import { ActivatedRoute} from '@angular/router';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-reset-password',
  templateUrl: './reset-password.component.html',
  styleUrls: ['./reset-password.component.css']
})
export class ResetPasswordComponent{
  password: string = "";
  passwordConfirm: string = "";
  message: string= "";
  invalidResetAttempt: boolean = false;
  userId: string = "";
  token: string = "";
  passwordReset: boolean = false;

  constructor(private route: ActivatedRoute, private authService: AuthService) {}

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.userId = params['userId'];
      this.token = params['token'];
    });
  }

  reset(){
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).{6,}$/;
    if(!passwordRegex.test(this.password)){
      this.message = "The password must meet the following criteria: \nAt least one lowercase letter \nAt least one uppercase letter \nAt least one digit \nAt least one non-alphanumeric character \nMinimum length of 6 characters";
      this.invalidResetAttempt = true;
      return;
    }

    if (this.password !== this.passwordConfirm) {
      this.message = 'Passwords do not match.';
      this.invalidResetAttempt = true;
      return;
    }

    this.authService.resetPassword(this.password, this.passwordConfirm, this.userId, this.token).subscribe(result => {
      if (result) {
        this.passwordReset = true;
      } else {
        this.message = "Invalid reset password attempt";
      }
    });
  }
}
