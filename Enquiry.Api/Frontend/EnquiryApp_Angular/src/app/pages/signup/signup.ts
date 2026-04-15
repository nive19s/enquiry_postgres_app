import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterModule } from '@angular/router';
import { Auth } from '../../service/auth';
import { RegisterRequest } from '../../models/user.model';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-signup',
    imports: [RouterLink, RouterModule, FormsModule, CommonModule],
    templateUrl: './signup.html',
    styleUrl: './signup.css',
})
export class Signup {

    signupObj: RegisterRequest & { confirmPassword: string } = {
        fullName: '',
        email: '',
        password: '',
        confirmPassword: ''
    };

    isLoading = false;
    errorMessage = '';
    successMessage = '';
    agreeToTerms = false;

    router = inject(Router);
    authService = inject(Auth);

    onSignup() {
        // Reset messages
        this.errorMessage = '';
        this.successMessage = '';

        // Validation
        if (!this.validateForm()) {
            return;
        }

        // Set loading state
        this.isLoading = true;

        // Prepare request (exclude confirmPassword)
        const request: RegisterRequest = {
            email: this.signupObj.email,
            password: this.signupObj.password,
            fullName: this.signupObj.fullName
        };

        // Call auth service
        this.authService.register(request).subscribe({
            next: (response) => {
                this.isLoading = false;

                if (response.success) {
                    this.successMessage = 'Registration successful! Redirecting to login...';

                    // Redirect to login after 2 seconds
                    setTimeout(() => {
                        this.router.navigateByUrl('/login');
                    }, 2000);
                } else {
                    this.errorMessage = response.message || 'Registration failed';
                }
            },
            error: (error) => {
                this.isLoading = false;

                if (error.error?.message) {
                    this.errorMessage = error.error.message;
                } else {
                    this.errorMessage = 'An error occurred. Please try again.';
                }

                console.error('Registration error:', error);
            }
        });
    }

    private validateForm(): boolean {
        // Check required fields
        if (!this.signupObj.fullName || !this.signupObj.email ||
            !this.signupObj.password || !this.signupObj.confirmPassword) {
            this.errorMessage = 'All fields are required';
            return false;
        }

        // Validate email format
        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(this.signupObj.email)) {
            this.errorMessage = 'Please enter a valid email address';
            return false;
        }

        // Validate password strength
        if (this.signupObj.password.length < 8) {
            this.errorMessage = 'Password must be at least 8 characters long';
            return false;
        }

        const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]/;
        if (!passwordRegex.test(this.signupObj.password)) {
            this.errorMessage = 'Password must contain uppercase, lowercase, number, and special character';
            return false;
        }

        // Check password confirmation
        if (this.signupObj.password !== this.signupObj.confirmPassword) {
            this.errorMessage = 'Passwords do not match';
            return false;
        }

        return true;
    }

}
