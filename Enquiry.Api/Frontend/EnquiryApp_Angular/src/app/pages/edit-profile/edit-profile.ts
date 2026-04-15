import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Auth } from '../../service/auth';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-profile.html',
  styleUrl: './edit-profile.css'
})
export class EditProfile implements OnInit {
  authService = inject(Auth);
  router = inject(Router);

  user = this.authService.currentUser();
  
  formData = {
    fullName: '',
    profileImage: ''
  };

  isLoading = false;
  errorMessage = '';

  ngOnInit() {
    if (this.user) {
      this.formData.fullName = this.user.fullName;
      this.formData.profileImage = this.user.profileImage || '';
    }
  }

  // Handle File Selection (Convert to Base64)
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.formData.profileImage = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit() {
    if (!this.user) return;

    this.isLoading = true;
    this.errorMessage = '';

    this.authService.updateProfile(
      this.user.userId,
      this.formData.fullName,
      this.formData.profileImage
    ).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) {
          // Update local state
          if (response.data) {
            // Use the FRESH data from the backend response
            this.authService.updateUserSession(response.data); 
          }
          
          this.router.navigate(['/profile']);
        }
      },
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = 'Failed to update profile';
        console.error(error);
      }
    });
  }

  onCancel() {
    this.router.navigate(['/profile']);
  }
}