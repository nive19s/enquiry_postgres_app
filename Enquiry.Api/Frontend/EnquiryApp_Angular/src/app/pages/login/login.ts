import { Component, inject, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../service/auth';
import { LoginRequest } from '../../models/user.model';
import { CommonModule } from '@angular/common';
import { SocialAuthService, GoogleLoginProvider, GoogleSigninButtonModule } from '@abacritt/angularx-social-login';
import { MsalService } from '@azure/msal-angular';
import { SocialLoginModule } from '@abacritt/angularx-social-login';
import { AuthenticationResult } from '@azure/msal-browser';


@Component({
  selector: 'app-login',
  standalone: true,
  imports: [RouterLink, FormsModule, CommonModule, SocialLoginModule, GoogleSigninButtonModule],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login implements OnInit {
  loginObj: LoginRequest = { email: '', password: '' };
  isLoading = false;
  errorMessage = '';
  agreeToTerms = false;

  router = inject(Router);
  authService = inject(Auth);
  socialAuthService = inject(SocialAuthService);
  msalService = inject(MsalService);

  onLogin() {
    this.errorMessage = '';
    if (!this.loginObj.email || !this.loginObj.password) {
      this.errorMessage = 'Please enter both email and password';
      return;
    }
    this.isLoading = true;

    this.authService.login(this.loginObj).subscribe({
      next: (response) => {
        this.isLoading = false;
        if (response.success) this.authService.navigateByRole();
        else this.errorMessage = response.message || 'Login failed';
      },
      error: (error) => {
        this.isLoading = false;
        if (error.status === 401) this.errorMessage = 'Invalid email or password';
        else if (error.error?.message) this.errorMessage = error.error.message;
        else this.errorMessage = 'An error occurred. Please try again.';
        console.error('Login error:', error);
      }
    });
  }

  ngOnInit() {
    // Listener that waits for the Google Button to finish
    this.socialAuthService.authState.subscribe((user) => {
      console.log('Google User returned:', user);
      if (user && user.idToken) {
        this.handleGoogleLogin(user.idToken);
      }
    });

    // Handle Microsoft Redirect Result
  this.msalService.handleRedirectObservable().subscribe({
    next: (result: AuthenticationResult) => {
      if (result && result.account) {
        // Login successful, send token to backend
        const idToken = result.idToken; 
        this.handleMicrosoftLogin(idToken);
      }
    },
    error: (error) => {
      console.error('MSAL Login Error:', error);
      this.errorMessage = 'Microsoft login invalid.';
    }
  });
}
// helper method
handleMicrosoftLogin(token: string) {
  this.isLoading = true;
  this.authService.microsoftLogin(token).subscribe({
    next: (resp) => {
      this.isLoading = false;
      if (resp.success) this.authService.navigateByRole();
      else this.errorMessage = resp.message || 'Microsoft login failed';
    },
    error: (err) => {
      this.isLoading = false;
      this.errorMessage = err?.error?.message || 'Microsoft login failed.';
    }
  });

  }

  // HELPER to send the token to backend
  handleGoogleLogin(idToken: string) {
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.googleLogin(idToken).subscribe({
      next: (resp) => {
        this.isLoading = false;
        if (resp.success) this.authService.navigateByRole();
        else this.errorMessage = resp.message || 'Google login failed';
      },
      error: (err) => {
        console.error('Backend Google login error:', err);
        this.errorMessage = err?.error?.message || 'Google login failed.';
        this.isLoading = false;
      }
    });
  }

  //login with Microsoft
  async loginWithMicrosoft() {
    this.errorMessage = '';
    this.isLoading = true;
    // Ensure MSAL is initialized
    await this.msalService.instance.initialize();
    this.msalService.loginRedirect();
  }
}