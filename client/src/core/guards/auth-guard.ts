import { inject } from '@angular/core';
import { CanActivateFn } from '@angular/router';
import { ToastService } from '../services/toast-service';
import { UserService } from '../services/user-service';

export const authGuard: CanActivateFn = () => {
  const userService = inject(UserService);
  const toast = inject(ToastService);

  if (userService.currentUser()) return true;
  else {
    toast.error('You shall not pass');
    return false;
  }
};

