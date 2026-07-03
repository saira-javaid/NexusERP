import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { RoleService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { Role } from '../../../core/models/role.model';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';
import { ConfirmDialogService } from '../../../shared/services/confirm-dialog.service';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    RouterLink, ReactiveFormsModule, MatTableModule, MatButtonModule,
    MatIconModule, MatInputModule, MatFormFieldModule, MatSnackBarModule,
    ListPaginationComponent,
  ],
  templateUrl: './role-list.component.html',
  styleUrl: './role-list.component.scss',
})
export class RoleListComponent implements OnInit {
  private readonly roleService = inject(RoleService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly preferences = inject(UserPreferencesService);
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly roles = signal<Role[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly canManage = this.authService.hasPermission('roles.manage');

  searchControl = new FormControl('');
  displayedColumns = ['name', 'description', 'users', 'permissions', 'actions'];

  private readonly systemRoles = new Set(['Admin', 'Manager', 'Member', 'Viewer']);

  ngOnInit(): void {
    this.loadRoles();
    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex.set(0);
      this.loadRoles();
    });
  }

  loadRoles(): void {
    this.roleService.getAll(this.pageIndex() + 1, this.pageSize(), this.searchControl.value ?? undefined)
      .subscribe(result => {
        this.roles.set(result.items);
        this.totalCount.set(result.totalCount);
      });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadRoles();
  }

  isSystemRole(name: string): boolean {
    return this.systemRoles.has(name);
  }

  deleteRole(role: Role): void {
    this.confirmDialog.confirmIfEnabled({
      title: 'Delete role',
      message: `Delete role "${role.name}"? Users assigned to this role may lose permissions.`,
      confirmText: 'Delete',
      confirmColor: 'warn',
      icon: 'delete',
    }).subscribe(confirmed => {
      if (!confirmed) return;
      this.roleService.delete(role.id).subscribe({
        next: () => {
          this.snackBar.open('Role deleted', 'Close', { duration: 3000 });
          this.pageIndex.set(0);
          this.loadRoles();
        },
        error: err => this.snackBar.open(err.error?.message ?? 'Failed to delete role', 'Close', { duration: 5000 }),
      });
    });
  }
}
