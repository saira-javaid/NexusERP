import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { PageEvent } from '@angular/material/paginator';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatChipsModule } from '@angular/material/chips';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { DatePipe } from '@angular/common';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import { UserService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { UserPreferencesService } from '../../../core/services/user-preferences.service';
import { UserListItem } from '../../../core/models/user.model';
import { ListPaginationComponent } from '../../../shared/components/list-pagination/list-pagination.component';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    RouterLink, ReactiveFormsModule, MatTableModule, MatButtonModule,
    MatIconModule, MatInputModule, MatFormFieldModule, MatSelectModule,
    MatChipsModule, MatSnackBarModule, DatePipe, ListPaginationComponent,
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
})
export class UserListComponent implements OnInit {
  private readonly userService = inject(UserService);
  private readonly authService = inject(AuthService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly preferences = inject(UserPreferencesService);

  readonly users = signal<UserListItem[]>([]);
  readonly totalCount = signal(0);
  readonly pageIndex = signal(0);
  readonly pageSize = signal(this.preferences.getPageSize());
  readonly canManage = this.authService.hasPermission('users.manage');

  searchControl = new FormControl('');
  activeControl = new FormControl<boolean | null>(null);

  displayedColumns = ['name', 'email', 'roles', 'status', 'lastLogin', 'actions'];

  ngOnInit(): void {
    this.loadUsers();
    this.searchControl.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.pageIndex.set(0);
      this.loadUsers();
    });
    this.activeControl.valueChanges.subscribe(() => {
      this.pageIndex.set(0);
      this.loadUsers();
    });
  }

  loadUsers(): void {
    const active = this.activeControl.value;
    this.userService.getAll(
      this.pageIndex() + 1,
      this.pageSize(),
      this.searchControl.value ?? undefined,
      active === null ? undefined : active,
    ).subscribe(result => {
      this.users.set(result.items);
      this.totalCount.set(result.totalCount);
    });
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex.set(event.pageIndex);
    this.pageSize.set(event.pageSize);
    if (event.pageSize !== this.preferences.getPageSize()) {
      this.preferences.update({ pageSize: event.pageSize });
    }
    this.loadUsers();
  }

  deactivate(user: UserListItem): void {
    const confirmDelete = this.preferences.preferences().confirmBeforeDelete;
    if (confirmDelete && !confirm(`Deactivate ${user.fullName}?`)) return;
    this.userService.delete(user.id).subscribe({
      next: () => {
        this.snackBar.open('User deactivated', 'Close', { duration: 3000 });
        this.pageIndex.set(0);
        this.loadUsers();
      },
      error: err => this.snackBar.open(err.error?.message ?? 'Failed to deactivate user', 'Close', { duration: 5000 }),
    });
  }
}
