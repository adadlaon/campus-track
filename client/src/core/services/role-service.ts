import { inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Roles } from '../../types/user';
import { tap } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class RoleService {
    private http = inject(HttpClient);
    private baseUrl = environment.apiUrl;
    roles = signal<Roles>([]);

    getRoles() {
        return this.http.get<Roles>(this.baseUrl + 'roles').pipe(
            tap(roles => this.roles.set(roles))
        );
    }
}
