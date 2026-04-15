import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from '../service/auth';
import { UserRole } from '../models/user.model';

export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(Auth);
    const router = inject(Router);

    if (!authService.checkIsLoggedIn()) {
      router.navigateByUrl('/login');
      return false;
    }

    const userRole = authService.getUserRole();
    
    if (userRole && allowedRoles.includes(userRole)) {
      return true;
    }

    // Redirect based on role
    if (userRole === UserRole.Admin) {
      router.navigateByUrl('/admin');
    } else {
      router.navigateByUrl('/list');
    }

    return false;
  };
};