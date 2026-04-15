// User Role Enum
export enum UserRole {
  Admin = 'Admin',
  User = 'User'
}

// User Interface
export interface User {
  userId: number;
  email: string;
  fullName: string;
  role: UserRole;
  profileImage?: string;
}

// Login Request
export interface LoginRequest {
  email: string;
  password: string;
}

// Login Response
export interface LoginResponse {
  success: boolean;
  token: string;
  user: User;
  expiresAt: string;
  message: string;
}

// Register Request
export interface RegisterRequest {
  email: string;
  password: string;
  fullName: string;
}

// API Response
export interface ApiResponse {
  success: boolean;
  message: string;
  data?: any;
}

// Auth State (for localStorage)
export interface AuthState {
  token: string;
  user: User;
  expiresAt: string;
}