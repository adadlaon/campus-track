import { CanActivateFn } from '@angular/router';
import { inject } from '@angular/core';
import { ToastService } from '../services/toast-service';
import { UserService } from '../services/user-service';

export const adminGuard: CanActivateFn = (route, state) => {
  const userService = inject(UserService);
  const toast = inject(ToastService);

  if (userService.currentUser()?.roles.includes('Administration')) {
    return true;
  }

  toast.error('Enter this area, you cannot');
  return false;
};