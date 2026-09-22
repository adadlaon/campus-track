import { Component, inject } from '@angular/core';
import { UserService } from '../../core/services/user-service';
import { UserManagement } from './user-management/user-management';

@Component({
  imports: [UserManagement],
  selector: 'app-administration',
  styleUrl: './administration.css',
  templateUrl: './administration.html',
})
export class Administration {
  protected userService = inject(UserService);
  activeTab = 'photos';
  tabs = [
    {label: 'Photo moderation', value: 'photos'},
    {label: 'User management', value: 'roles'}
  ]

  setTab(tab: string) {
    this.activeTab = tab;
  }
}
