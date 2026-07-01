import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { KeyValuePipe } from '@angular/common';
import { RoleService } from '../../../core/services/api.service';
import { Permission } from '../../../core/models/role.model';

@Component({
  selector: 'app-role-form',
  standalone: true,
  imports: [
    ReactiveFormsModule, RouterLink, MatCardModule, MatFormFieldModule,
    MatInputModule, MatButtonModule, MatCheckboxModule, MatSnackBarModule, KeyValuePipe,
  ],
  templateUrl: './role-form.component.html',
})
export class RoleFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly roleService = inject(RoleService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly snackBar = inject(MatSnackBar);

  isEdit = false;
  roleId?: string;
  isSystemRole = false;
  allPermissions: Permission[] = [];
  permissionsByModule: Record<string, Permission[]> = {};
  selectedPermissionIds = new Set<string>();

  form = this.fb.nonNullable.group({
    name: ['', Validators.required],
    description: [''],
  });

  ngOnInit(): void {
    this.roleService.getPermissions().subscribe(perms => {
      this.allPermissions = perms;
      this.permissionsByModule = perms.reduce((acc, p) => {
        (acc[p.module] ??= []).push(p);
        return acc;
      }, {} as Record<string, Permission[]>);
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.roleId = id;
      this.roleService.getById(id).subscribe(role => {
        this.isSystemRole = ['Admin', 'Manager', 'Member', 'Viewer'].includes(role.name);
        this.form.patchValue({ name: role.name, description: role.description ?? '' });
        if (this.isSystemRole) {
          this.form.controls.name.disable();
        }
        role.permissionIds.forEach(pid => this.selectedPermissionIds.add(pid));
      });
    }
  }

  isSelected(id: string): boolean {
    return this.selectedPermissionIds.has(id);
  }

  togglePermission(id: string, checked: boolean): void {
    if (checked) this.selectedPermissionIds.add(id);
    else this.selectedPermissionIds.delete(id);
  }

  toggleModule(module: string, checked: boolean): void {
    const perms = this.permissionsByModule[module] ?? [];
    perms.forEach(p => {
      if (checked) this.selectedPermissionIds.add(p.id);
      else this.selectedPermissionIds.delete(p.id);
    });
  }

  isModuleFullySelected(module: string): boolean {
    const perms = this.permissionsByModule[module] ?? [];
    return perms.length > 0 && perms.every(p => this.selectedPermissionIds.has(p.id));
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    const permissionIds = [...this.selectedPermissionIds];

    if (this.isEdit && this.roleId) {
      this.roleService.update(this.roleId, {
        id: this.roleId,
        name: value.name,
        description: value.description,
      }).subscribe({
        next: () => this.savePermissions(this.roleId!, permissionIds),
        error: err => this.snackBar.open(err.error?.message ?? 'Update failed', 'Close', { duration: 5000 }),
      });
    } else {
      this.roleService.create({ name: value.name, description: value.description }).subscribe({
        next: role => this.savePermissions(role.id, permissionIds),
        error: err => this.snackBar.open(err.error?.message ?? 'Create failed', 'Close', { duration: 5000 }),
      });
    }
  }

  private savePermissions(roleId: string, permissionIds: string[]): void {
    this.roleService.updatePermissions(roleId, { permissionIds }).subscribe({
      next: () => this.router.navigate(['/roles']),
      error: err => this.snackBar.open(err.error?.message ?? 'Failed to save permissions', 'Close', { duration: 5000 }),
    });
  }
}
