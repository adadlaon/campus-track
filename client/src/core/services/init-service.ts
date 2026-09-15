import { inject, Injectable } from '@angular/core';
import { UserService } from './user-service';
import { tap } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class InitService {
    private userService = inject(UserService);

    init() {
        return this.userService.refreshToken().pipe(
            tap(user => {
                if (user) {
                    this.userService.setCurrentUser(user);
                    this.userService.startTokenRefreshInterval();
                }
            })
        )
    }
}