import { Directive, ElementRef, HostListener, inject } from '@angular/core';
import { NgControl } from '@angular/forms';

@Directive({ selector: '[appAutofocus]', standalone: true })
export class AutofocusDirective {
  private readonly el = inject(ElementRef);

  ngAfterViewInit(): void {
    setTimeout(() => this.el.nativeElement.focus(), 100);
  }
}

@Directive({ selector: '[appPermission]', standalone: true })
export class PermissionDirective {
  private readonly el = inject(ElementRef);
  private readonly control = inject(NgControl, { optional: true });

  @HostListener('click', ['$event'])
  onClick(event: Event): void {
    // Permission check handled at route level; directive hides elements
  }

  set permission(value: string) {
    const user = JSON.parse(localStorage.getItem('nexus_user') ?? '{}');
    const hasPermission = user?.permissions?.includes(value) || user?.roles?.includes('Admin');
    if (!hasPermission) {
      this.el.nativeElement.style.display = 'none';
    }
  }
}

@Directive({ selector: '[appInfiniteScroll]', standalone: true })
export class InfiniteScrollDirective {
  private readonly el = inject(ElementRef);
  private loading = false;

  @HostListener('scroll')
  onScroll(): void {
    const element = this.el.nativeElement;
    const threshold = 100;
    const atBottom = element.scrollHeight - element.scrollTop - element.clientHeight < threshold;

    if (atBottom && !this.loading) {
      this.loading = true;
      element.dispatchEvent(new CustomEvent('loadMore', { bubbles: true }));
      setTimeout(() => this.loading = false, 500);
    }
  }
}
