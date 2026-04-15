import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, tap, catchError, throwError } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
  ApiResponse,
  User,
  UserRole,
  AuthState
} from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class Auth {
  // Reactive state using signals
  currentUser = signal<User | null>(null);
  isLoggedIn = signal<boolean>(false);
  userRole = signal<UserRole | null>(null);

  private readonly AUTH_STATE_KEY = 'authState';

  constructor(
    private router: Router,
    private http: HttpClient
  ) {
    // Check if user is already logged in on service initialization
    this.checkLoginStatus();
  }

  // Check localStorage on app load
  private checkLoginStatus(): void {
    const authState = this.getAuthState();

    if (authState) {
      // Check if token is expired
      if (this.isTokenExpired(authState.expiresAt)) {
        this.logout();
        return;
      }

      // Restore user state
      this.currentUser.set(authState.user);
      this.isLoggedIn.set(true);
      this.userRole.set(authState.user.role);
    }
  }

  // Register new user
  register(request: RegisterRequest): Observable<ApiResponse> {
    return this.http.post<ApiResponse>(
      `${environment.authApiUrl}/register`,
      request
    ).pipe(
      catchError(error => {
        console.error('Registration error:', error);
        return throwError(() => error);
      })
    );
  }

  // Login user
  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${environment.authApiUrl}/login`,
      request
    ).pipe(
      tap(response => {
        if (response.success) {
          // Store auth state
          const authState: AuthState = {
            token: response.token,
            user: response.user,
            expiresAt: response.expiresAt
          };

          localStorage.setItem(this.AUTH_STATE_KEY, JSON.stringify(authState));

          // Update signals
          this.currentUser.set(response.user);
          this.isLoggedIn.set(true);
          this.userRole.set(response.user.role);
        }
      }),
      catchError(error => {
        console.error('Login error:', error);
        return throwError(() => error);
      })
    );
  }

  // Google Login
  googleLogin(idToken: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${environment.authApiUrl}/google-login`,
      { idToken }
    ).pipe(
      tap(response => {
        if (response.success) {
          const authState: AuthState = {
            token: response.token,
            user: response.user,
            expiresAt: response.expiresAt
          };

          localStorage.setItem(this.AUTH_STATE_KEY, JSON.stringify(authState));
          this.currentUser.set(response.user);
          this.isLoggedIn.set(true);
          this.userRole.set(response.user.role);
        }
      }),
      catchError(error => {
        console.error('Google login error:', error);
        return throwError(() => error);
      })
    );
  }

  // Handle microsoft login
  microsoftLogin(token: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(
      `${environment.authApiUrl}/microsoft-login`,
      { token }
    ).pipe(
      tap(response => {
        if (response.success) {
          const authState: AuthState = {
            token: response.token,
            user: response.user,
            expiresAt: response.expiresAt
          };
          localStorage.setItem(this.AUTH_STATE_KEY, JSON.stringify(authState));
          this.currentUser.set(response.user);
          this.isLoggedIn.set(true);
          this.userRole.set(response.user.role);
        }
      }),
      catchError(error => {
        console.error('Microsoft login error:', error);
        return throwError(() => error);
      })
    );
  }

  // Logout user
  logout(): void {
    localStorage.removeItem(this.AUTH_STATE_KEY);
    this.currentUser.set(null);
    this.isLoggedIn.set(false);
    this.userRole.set(null);
    this.router.navigateByUrl('/login');
  }

  // Get current user
  getCurrentUser(): User | null {
    return this.currentUser();
  }

  // Check if logged in
  checkIsLoggedIn(): boolean {
    return this.isLoggedIn();
  }

  // Get user role
  getUserRole(): UserRole | null {
    return this.userRole();
  }

  // Check if user is admin
  isAdmin(): boolean {
    return this.userRole() === UserRole.Admin;
  }

  // Check if user is regular user
  isUser(): boolean {
    return this.userRole() === UserRole.User;
  }

  // Get JWT token
  getToken(): string | null {
    const authState = this.getAuthState();
    return authState ? authState.token : null;
  }

  // Check if token is expired
  isTokenExpired(expiresAt: string): boolean {
    const expirationDate = new Date(expiresAt);
    const now = new Date();
    return now >= expirationDate;
  }

  // Get auth state from localStorage
  private getAuthState(): AuthState | null {
    const authStateStr = localStorage.getItem(this.AUTH_STATE_KEY);

    if (!authStateStr) {
      return null;
    }

    try {
      return JSON.parse(authStateStr) as AuthState;
    } catch (e) {
      console.error('Error parsing auth state:', e);
      return null;
    }
  }

  // Navigate based on role
  navigateByRole(): void {
    const role = this.getUserRole();

    if (role === UserRole.Admin) {
      this.router.navigateByUrl('/admin');
    } else if (role === UserRole.User) {
      this.router.navigateByUrl('/list');
    } else {
      this.router.navigateByUrl('/login');
    }
  }

  // Update Profile
  updateProfile(userId: number, fullName: string, profileImage?: string): Observable<ApiResponse> {
    const payload = { userId, fullName, profileImage };
    return this.http.post<ApiResponse>(
      `${environment.authApiUrl}/update-profile`,
      payload
    ).pipe(
      catchError(error => {
        console.error('Update profile error:', error);
        return throwError(() => error);
      })
    );
  }

  // Manually update the stored session (Signal + LocalStorage)
  updateUserSession(user: User): void {
     // Update Signal
     this.currentUser.set(user);
     
     // Update LocalStorage
     const currentAuth = this.getAuthState();
     if (currentAuth) {
         currentAuth.user = user; // Update user object inside the state
         localStorage.setItem(this.AUTH_STATE_KEY, JSON.stringify(currentAuth));
     }
  }
}