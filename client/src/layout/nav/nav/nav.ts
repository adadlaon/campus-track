import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { HasRole } from '../../../shared/directives/has-role';
import { UserService } from '../../../core/services/user-service';
import { BusyService } from '../../../core/services/busy-service';
import { ToastService } from '../../../core/services/toast-service';
import { themes } from '../../theme';

@Component({
  imports: [FormsModule, RouterLink, RouterLinkActive, HasRole],
  selector: 'app-nav',
  styleUrl: './nav.css',
  templateUrl: './nav.html',
})
export class Nav implements OnInit{
    protected userService = inject(UserService);
    protected busyService = inject(BusyService);
    private router = inject(Router);
    private toast =inject(ToastService);
    protected creds: any = {};
    protected selectedTheme = signal<string>(localStorage.getItem('theme') || 'light')
    protected themes = themes;
    protected loading = signal(false);

  ngOnInit(): void {
    document.documentElement.setAttribute('data-theme', this.selectedTheme());
  }

  handleSelectTheme(theme: string) {
    this.selectedTheme.set(theme);
    localStorage.setItem('theme', theme);
    document.documentElement.setAttribute('data-theme', theme);
    const elem = document.activeElement as HTMLDivElement;
    if (elem) elem.blur();
  }
  
  handleSelectUserItem() {
    const elem = document.activeElement as HTMLDivElement;
    if (elem) elem.blur();
  }

  login() {
    this.loading.set(true);
    this.userService.login(this.creds).subscribe({
      next: () => {
        this.router.navigateByUrl('/members');
        this.toast.success('Logged in sucessfully');
        this.creds = {};
      },
      error: error => {
        this.toast.error(error.error);
      },
      complete: () => this.loading.set(false)
    })
  }

  logout() {
    this.userService.logout();
    this.router.navigateByUrl('/');
  }
}
