import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Auth } from '../../service/auth';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-profile-page',
  imports: [CommonModule, RouterLink],
  templateUrl: './profile-page.html',
  styleUrl: './profile-page.css',
})
export class ProfilePage {
  authservice = inject(Auth);
  user = this.authservice.currentUser();
}
