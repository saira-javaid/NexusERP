import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { RoleService, UserService } from '../../../core/services/api.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule,
    MatInputModule, MatSelectModule, MatButtonModule, MatSlideToggleModule,
    MatSnackBarModule,
  ],
  templateUrl: './user-form.component.html',
})
export class UserFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly userService = inject(UserService);
  private readonly roleService = inject(RoleService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  isEdit = false;
  userId?: string;
  availableRoles: string[] = [];

  form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    isActive: [true],
    roles: [[] as string[]],
  });

  ngOnInit(): void {
    this.roleService.getAll(1, 100).subscribe(result => {
      this.availableRoles = result.items.map(r => r.name);
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.userId = id;
      this.form.controls.password.clearValidators();
      this.form.controls.password.updateValueAndValidity();
      this.userService.getById(id).subscribe(user => {
        this.form.patchValue({
          email: user.email,
          firstName: user.firstName,
          lastName: user.lastName,
          isActive: user.isActive,
          roles: user.roles,
        });
      });
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();

    if (this.isEdit && this.userId) {
      this.userService.update(this.userId, {
        id: this.userId,
        email: value.email,
        firstName: value.firstName,
        lastName: value.lastName,
        isActive: value.isActive,
        roles: value.roles,
      }).subscribe({
        next: () => this.router.navigate(['/users']),
        error: err => this.snackBar.open(err.error?.message ?? 'Update failed', 'Close', { duration: 5000 }),
      });
    } else {
      this.userService.create({
        email: value.email,
        password: value.password,
        firstName: value.firstName,
        lastName: value.lastName,
        isActive: value.isActive,
        roles: value.roles.length ? value.roles : ['Member'],
      }).subscribe({
        next: () => this.router.navigate(['/users']),
        error: err => this.snackBar.open(err.error?.message ?? 'Create failed', 'Close', { duration: 5000 }),
      });
    }
  }
}
