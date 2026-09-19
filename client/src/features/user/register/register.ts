import { Component, computed, inject, output, signal } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, ReactiveFormsModule, ValidationErrors, ValidatorFn, Validators } from '@angular/forms';
import { TextInput } from '../../../shared/text-input/text-input';
import { UserService } from '../../../core/services/user-service';
import { RoleService } from '../../../core/services/role-service';
import { Router } from '@angular/router';
import { RegisterCreds } from '../../../types/user';

@Component({
  imports: [ReactiveFormsModule, TextInput],
  selector: 'app-register',
  styleUrl: './register.css',
  templateUrl: './register.html',
})
export class Register {
  private userService = inject(UserService);
  private roleService = inject(RoleService);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  cancelRegister = output<boolean>();
  protected creds = {} as RegisterCreds;
  protected credentialsForm: FormGroup;
  protected profileForm: FormGroup;
  protected currentStep = signal(1);
  protected validationErrors = signal<string[]>([]);
  private readonly roleOrder = ['Parent', 'Guardian', 'Teacher', 'Security', 'Administration'];
  protected roles = computed(() => {
    const roles = this.roleService.roles();
    return roles
      .filter(role => role.name === 'Parent' || role.name === 'Guardian')
      .sort((left, right) =>
        this.roleOrder.indexOf(left.name) - this.roleOrder.indexOf(right.name)
      );
  });

  constructor() {
    this.credentialsForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: ['', Validators.required],
      password: ['', [Validators.required, 
        Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', [Validators.required, this.matchVlues('password')]]
    });

    this.profileForm = this.fb.group({
      roleId: ['', Validators.required],
      houseNumber: [''],
      zone: ['', Validators.pattern(/^[0-9]+$/)],
      barangay: ['', Validators.required],
      city: ['Legazpi City', Validators.required],
      province: ['Albay', Validators.required],
      phoneNumber: ['', [
        Validators.required,
        Validators.pattern(/^[0-9]+$/),
        Validators.minLength(11),
        Validators.maxLength(11)
      ]]
    })

    this.roleService.getRoles().subscribe({
      next: roles => {
        const parentRole = roles.find(role => role.name === 'Parent');
        if (parentRole) {
          this.profileForm.patchValue({ roleId: parentRole.id });
        }
      },
      error: () => this.validationErrors.set(['Unable to load roles.'])
    });

    this.credentialsForm.controls['password'].valueChanges.subscribe(() => {
      this.credentialsForm.controls['confirmPassword'].updateValueAndValidity();
    }) 
  }

  matchVlues(matchTo: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const parent = control.parent;
      if (!parent) return null;

      const matchValue = parent.get(matchTo)?.value;
      return control.value === matchValue ? null : {passwordMismatch: true}
    }
  }

  nextStep() {
    if (this.credentialsForm.valid) {
      this.currentStep.update(prevStep => prevStep + 1);
    }
  }

  prevStep() {
    this.currentStep.update(prevStep => prevStep - 1);
  }

  getMaxDate() {
    const today = new Date();
    today.setFullYear(today.getFullYear() - 18);
    return today.toISOString().split('T')[0];
  }

  register() {
    if (this.profileForm.valid && this.credentialsForm.valid) {
      const formData = { ...this.credentialsForm.value, ...this.profileForm.value };

      this.userService.register(formData).subscribe({
        next: () => {
          this.router.navigateByUrl('/members');
        },
        error: error => {
          console.log(error);
          this.validationErrors.set(error);
        }
      })
    }
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
