import { Component, inject, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { ConfirmDialog } from '../shared/confirm-dialog/confirm-dialog';
import { Nav } from '../layout/nav/nav/nav';

@Component({
  imports: [Nav, RouterOutlet, ConfirmDialog],
  selector: 'app-root',
  styleUrl: './app.css',
  templateUrl: './app.html',
})
export class App {
  protected router = inject(Router);
}
