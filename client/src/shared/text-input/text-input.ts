import { Component, input, signal, Self } from '@angular/core';
import { ControlValueAccessor, FormControl, NgControl, ReactiveFormsModule } from '@angular/forms';

@Component({
  imports: [ReactiveFormsModule],
  selector: 'app-text-input',
  styleUrl: './text-input.css',
  templateUrl: './text-input.html',
})
export class TextInput implements ControlValueAccessor {
  label = input<string>('');
  type = input<string>('text');
  maxDate = input<string>('');
  value = signal('');
  disabled = signal(false);
  passwordVisible = signal(false);

  private onChange: (value: string) => void = () => {};
  private onTouched: () => void = () => {};
  
  constructor(@Self() public ngControl: NgControl) {
    this.ngControl.valueAccessor = this;
  }

  writeValue(value: string | null): void {
    this.value.set(value ?? '');
  }
  registerOnChange(fn: (value: string) => void): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: () => void): void {
    this.onTouched = fn;
  }
  setDisabledState(isDisabled: boolean): void {
    this.disabled.set(isDisabled);
  }

  handleInput(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.value.set(value);
    this.onChange(value);
  }

  handleBlur(): void {
    this.onTouched();
  }

  togglePasswordVisibility(): void {
    this.passwordVisible.update(visible => !visible);
  }

  get inputType(): string {
    return this.type() === 'password' && !this.passwordVisible() ? 'password' : 'text';
  }

   get control(): FormControl {
    return this.ngControl.control as FormControl;
  }
}
