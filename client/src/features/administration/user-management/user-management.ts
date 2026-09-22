import { Component, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { RoleService } from '../../../core/services/role-service';
import { ConfirmDialogService } from '../../../core/services/confirm-dialog-service';
import { ManagedUser, ManagedUserRequest } from '../../../types/user';

type RoleOption = { id: string; name: string; description: string | null };

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-user-management',
  styleUrl: './user-management.css',
  templateUrl: './user-management.html',
})
export class UserManagement {
  private http = inject(HttpClient);
  private formBuilder = inject(FormBuilder);
  private roleService = inject(RoleService);
  private confirmDialog = inject(ConfirmDialogService);
  private baseUrl = environment.apiUrl + 'UserManagement';

  protected users = signal<ManagedUser[]>([]);
  protected currentPage = signal(1);
  protected readonly pageSize = 5;
  protected roles = signal<RoleOption[]>([]);
  protected selectedUser = signal<ManagedUser | null>(null);
  protected loading = signal(false);
  protected saving = signal(false);
  protected validationErrors = signal<string[]>([]);
  protected totalPages = computed(() => Math.max(1, Math.ceil(this.users().length / this.pageSize)));
  protected pagedUsers = computed(() => {
    const start = (this.currentPage() - 1) * this.pageSize;
    return this.users().slice(start, start + this.pageSize);
  });

  protected userForm = this.formBuilder.group({
    email: ['', [Validators.required, Validators.email]],
    password: [''],
    displayName: ['', Validators.required],
    imageUrl: [''],
    roles: [[] as string[]],
    houseNumber: [''],
    zone: [''],
    barangay: ['', Validators.required],
    city: ['Legazpi City', Validators.required],
    province: ['Albay', Validators.required],
    phoneNumber: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]]
  });

  constructor() {
    this.loadUsers();
    this.roleService.getRoles().subscribe({
      next: roles => this.roles.set(roles),
      error: error => this.validationErrors.set(this.getErrors(error, 'Unable to load application roles.'))
    });
  }

  protected loadUsers() {
    this.loading.set(true);
    this.http.get<unknown>(this.baseUrl).subscribe({
      next: response => {
        const users = this.getUsersFromResponse(response);
        if (!users) {
          this.validationErrors.set(['The user list response did not contain a user list.']);
          this.loading.set(false);
          return;
        }

        this.users.set(users);
        this.currentPage.set(Math.min(this.currentPage(), Math.max(1, Math.ceil(users.length / this.pageSize))));
        this.loading.set(false);
      },
      error: error => {
        this.validationErrors.set(this.getErrors(error, 'Unable to load users.'));
        this.loading.set(false);
      }
    });
  }

  protected previousPage() {
    this.currentPage.update(page => Math.max(1, page - 1));
  }

  protected nextPage() {
    this.currentPage.update(page => Math.min(this.totalPages(), page + 1));
  }

  protected selectUser(user: ManagedUser) {
    this.selectedUser.set(user);
    this.userForm.patchValue({
      email: user.email,
      password: '',
      displayName: user.displayName,
      imageUrl: user.imageUrl ?? '',
      roles: user.roles,
      houseNumber: user.houseNumber ?? '',
      zone: user.zone ?? '',
      barangay: user.barangay ?? '',
      city: user.city ?? '',
      province: user.province ?? '',
      phoneNumber: user.phoneNumber ?? ''
    });
    this.userForm.controls.password.clearValidators();
    this.userForm.controls.password.updateValueAndValidity();
  }

  protected startCreate() {
    this.selectedUser.set(null);
    this.userForm.reset({
      email: '', password: '', displayName: '', imageUrl: '', roles: [],
      houseNumber: '', zone: '', barangay: '', city: 'Legazpi City',
      province: 'Albay', phoneNumber: ''
    });
    this.userForm.controls.password.setValidators([Validators.required, Validators.minLength(4)]);
    this.userForm.controls.password.updateValueAndValidity();
  }

  protected isRoleSelected(role: string) {
    return this.userForm.controls.roles.value?.includes(role) ?? false;
  }

  protected toggleRole(role: string) {
    const currentRoles = this.userForm.controls.roles.value ?? [];
    const roles = currentRoles.includes(role)
      ? currentRoles.filter(currentRole => currentRole !== role)
      : [...currentRoles, role];
    this.userForm.controls.roles.setValue(roles);
    this.userForm.controls.roles.markAsDirty();
  }

  protected saveUser() {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const value = this.userForm.getRawValue();
    const request: ManagedUserRequest = {
      email: value.email ?? '',
      password: value.password || undefined,
      displayName: value.displayName ?? '',
      imageUrl: value.imageUrl || undefined,
      roles: value.roles ?? [],
      houseNumber: value.houseNumber || undefined,
      zone: value.zone || undefined,
      barangay: value.barangay ?? '',
      city: value.city ?? '',
      province: value.province ?? '',
      phoneNumber: value.phoneNumber ?? ''
    };
    const selectedUser = this.selectedUser();
    const request$ = selectedUser
      ? this.http.put<ManagedUser>(`${this.baseUrl}/${selectedUser.id}`, request)
      : this.http.post<ManagedUser>(this.baseUrl, request);

    request$.subscribe({
      next: user => {
        this.saving.set(false);
        this.validationErrors.set([]);
        this.loadUsers();
        this.selectUser(user);
      },
      error: error => {
        this.saving.set(false);
        this.validationErrors.set(this.getErrors(error, 'Unable to save user.'));
      }
    });
  }

  protected async deleteUser(user: ManagedUser) {
    const confirmed = await this.confirmDialog.confirm(`Delete ${user.displayName}?`);
    if (!confirmed) return;

    this.http.delete(`${this.baseUrl}/${user.id}`).subscribe({
      next: () => {
        this.validationErrors.set([]);
        if (this.selectedUser()?.id === user.id) this.startCreate();
        this.loadUsers();
      },
      error: error => this.validationErrors.set(this.getErrors(error, 'Unable to delete user.'))
    });
  }

  private getErrors(error: any, fallback: string): string[] {
    if (Array.isArray(error)) return error.flatMap(item => this.getErrors(item, fallback));
    if (error?.status === 401) return ['Your session is not authorized to load users. Please log in again.'];
    if (error?.status === 403) return ['You do not have permission to manage users.'];

    const modelErrors = error?.error?.errors;
    if (modelErrors) {
      return Object.values(modelErrors).flatMap(messages => messages as string[]);
    }

    if (error?.error?.message) return [error.error.message];
    if (typeof error?.error === 'string') return [error.error];
    return [fallback];
  }

  private getUsersFromResponse(response: unknown): ManagedUser[] | null {
    if (Array.isArray(response)) return response as ManagedUser[];
    if (!response || typeof response !== 'object') return null;

    const envelope = response as { data?: unknown; items?: unknown; $values?: unknown };
    const users = envelope.data ?? envelope.items ?? envelope.$values;
    return Array.isArray(users) ? users as ManagedUser[] : null;
  }
}
