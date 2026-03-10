import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[asciiOnly]',
  standalone: true,
})
export class AsciiOnlyDirective {
  constructor(private el: ElementRef<HTMLInputElement | HTMLTextAreaElement>) {}

  private sanitize(value: string): string {
    let s = '';
    for (const ch of value ?? '') {
      if (ch.charCodeAt(0) <= 127) s += ch; // ASCII only
    }
    return s;
  }

  @HostListener('input')
  onInput(): void {
    const input = this.el.nativeElement;
    const sanitized = this.sanitize(input.value);

    if (sanitized !== input.value) {
      const pos = input.selectionStart ?? sanitized.length;
      input.value = sanitized;
      // restore cursor position (best-effort)
      input.setSelectionRange(pos - 1, pos - 1);
      // notify Angular forms/ngModel
      input.dispatchEvent(new Event('input', { bubbles: true }));
    }
  }

  @HostListener('paste', ['$event'])
  onPaste(e: ClipboardEvent): void {
    // Let paste happen, then input() handler sanitizes.
    // (This avoids fighting browser paste behavior.)
  }
}
