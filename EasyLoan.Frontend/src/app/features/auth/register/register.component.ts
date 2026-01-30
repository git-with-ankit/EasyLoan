import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule
} from '@angular/forms';
import { AuthService } from '../auth.service';
import { CommonModule } from '@angular/common';

import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';

type RegisterRole = 'Customer' | 'Manager';

@Component({
  standalone: true,
  selector: 'app-register',
  templateUrl: './register.component.html',
  imports: [ CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule]
})
export class RegisterComponent implements OnInit {

  form!: FormGroup;

  private readonly PASSWORD_REGEX =
    /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,20}$/;

  private readonly PHONE_REGEX = /^[6-9]\d{9}$/;
  private readonly PAN_REGEX = /^[A-Z]{5}[0-9]{4}[A-Z]$/;

  constructor(
    private fb: FormBuilder,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.buildForm('Customer');
  }

  buildForm(role: RegisterRole): void {
    // base controls (common to both DTOs)
    this.form = this.fb.group({
      role: [role, Validators.required],
      name: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required, Validators.pattern(this.PHONE_REGEX)]],
      password: ['', [Validators.required, Validators.pattern(this.PASSWORD_REGEX)]],
    });

    // customer-only DTO fields
    if (role === 'Customer') {
      this.form.addControl(
        'dateOfBirth',
        this.fb.control('', Validators.required)
      );

      this.form.addControl(
        'annualSalary',
        this.fb.control(0, [Validators.required, Validators.min(0)])
      );

      this.form.addControl(
        'panNumber',
        this.fb.control('', [Validators.required, Validators.pattern(this.PAN_REGEX)])
      );
    }
  }

  onRoleChange(): void {
    const role = this.form.get('role')!.value as RegisterRole;
    this.buildForm(role);
  }

  submit(): void {
    if (this.form.invalid) return;

    const { role, ...dto } = this.form.value;
    this.auth.register(role, dto).subscribe();
  }
}
