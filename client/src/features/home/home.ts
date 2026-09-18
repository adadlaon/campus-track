import { Component, inject, signal } from '@angular/core';
import { Register } from '../user/register/register';
import { UserService } from '../../core/services/user-service';

@Component({
  imports: [Register],
  selector: 'app-home',
  styleUrl: './home.css',
  templateUrl: './home.html',
})
export class Home {
  protected registerMode = signal(false);
  protected userService = inject(UserService);

  showRegister(value: boolean) {
    this.registerMode.set(value);
  }
}
