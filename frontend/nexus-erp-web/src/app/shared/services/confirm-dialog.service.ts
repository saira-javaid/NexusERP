import { inject, Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { map, Observable, of } from 'rxjs';
import { UserPreferencesService } from '../../core/services/user-preferences.service';
import { ConfirmDialogComponent } from '../components/confirm-dialog/confirm-dialog.component';

export interface ConfirmDialogData {
  title?: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmColor?: 'primary' | 'warn';
  icon?: string;
}

@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly dialog = inject(MatDialog);
  private readonly preferences = inject(UserPreferencesService);

  open(data: ConfirmDialogData): Observable<boolean> {
    return this.dialog
      .open(ConfirmDialogComponent, {
        data,
        width: '400px',
        maxWidth: 'calc(100vw - 32px)',
        autoFocus: 'dialog',
        disableClose: false,
        hasBackdrop: true,
        panelClass: 'confirm-dialog-panel',
      })
      .afterClosed()
      .pipe(map(result => result === true));
  }

  confirmIfEnabled(data: ConfirmDialogData): Observable<boolean> {
    if (!this.preferences.preferences().confirmBeforeDelete) {
      return of(true);
    }
    return this.open(data);
  }
}
