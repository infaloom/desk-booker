import { Component } from '@angular/core';
import { ConfigurationService } from './services/configuration.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title: string = 'DeskManagementFront';
  showSplashScreen: boolean = true;
  systemConfigured: boolean = false;

  constructor(private configurationService: ConfigurationService) {}

  ngOnInit() {
    setTimeout(() => {
      this.showSplashScreen = false;
    }, 500);

    this.configurationService.getConfigurationStatus().subscribe(isConfigured => {
      this.systemConfigured = isConfigured;
    });
  }
}