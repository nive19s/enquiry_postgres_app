import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Auth } from './service/auth';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('EnquiryApp_Angular');

  // Inject auth service
  authService = inject(Auth);

  // Method to handle logout
  onLogout() {
    this.authService.logout();
  }
}