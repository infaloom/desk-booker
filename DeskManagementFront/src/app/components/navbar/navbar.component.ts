import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.css']
})
export class NavbarComponent implements OnInit {
  loggedInUser: string | null = null;
  loggedInUserRole: string | null = null;
  isUserDeskAdmin: boolean = false;
  isUserAdmin: boolean = false;

  constructor(private authService: AuthService) {}

  ngOnInit() {
    this.loggedInUser = this.authService.getLoggedInUser();
    this.loggedInUserRole = this.authService.getLoggedInUserRole();
    
    this.authService.usernameChanged.subscribe((username: string | null) => {
      this.loggedInUser = username;
    });

    this.authService.roleChanged.subscribe((role: string | null) => {
      this.loggedInUserRole = role;
      this.isUserAdmin = this.loggedInUserRole == 'Admin';
      this.isUserDeskAdmin = this.loggedInUserRole == 'DeskAdmin';
    });

    this.isUserAdmin = this.loggedInUserRole == 'Admin';
    this.isUserDeskAdmin = this.loggedInUserRole == 'DeskAdmin';
  }
}
