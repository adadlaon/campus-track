import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { ServerError } from '../shared/errors/server-error/server-error';
import { NotFound } from '../shared/errors/not-found/not-found';
import { authGuard } from '../core/guards/auth-guard';
import { adminGuard } from '../core/guards/admin-guard';
import { Administration } from '../features/administration/administration';

export const routes: Routes = [
    {path: '', component: Home},
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [authGuard],
        children: [
            {path: 'administration', component: Administration, canActivate: [adminGuard]}
        ]
    },
    {path: 'server-error', component: ServerError},
    {path: '**', component: NotFound}
];
