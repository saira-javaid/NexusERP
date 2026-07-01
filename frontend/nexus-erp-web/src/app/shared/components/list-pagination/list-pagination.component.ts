import { Component, input, output } from '@angular/core';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { PAGE_SIZE_OPTIONS } from '../../../core/constants/pagination';

@Component({
  selector: 'app-list-pagination',
  standalone: true,
  imports: [MatPaginatorModule],
  template: `
  <mat-paginator
    [length]="totalCount()"
    [pageSize]="pageSize()"
    [pageIndex]="pageIndex()"
    [pageSizeOptions]="sizeOptions()"
    [showFirstLastButtons]="true"
    (page)="pageChange.emit($event)"
    aria-label="List pagination">
  </mat-paginator>
  `,
  styles: `:host { display: block; } mat-paginator { background: transparent; }`,
})
export class ListPaginationComponent {
  readonly totalCount = input.required<number>();
  readonly pageIndex = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly sizeOptions = input<number[]>([...PAGE_SIZE_OPTIONS]);
  readonly pageChange = output<PageEvent>();
}
