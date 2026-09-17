import { Routes } from '@angular/router';
import { Home } from '../features/home/home';
import { ServerError } from '../shared/errors/server-error/server-error';
import { NotFound } from '../shared/errors/not-found/not-found';

export const routes: Routes = [
    {path: '', component: Home},
    {path: 'server-error', component: ServerError},
    {path: '**', component: NotFound}
];
