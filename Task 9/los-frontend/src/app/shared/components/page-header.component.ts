import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-page-header',
  standalone: true,
  template: `
    <header class="page-header">
      <div>
        <h1>{{ title }}</h1>
        @if (subtitle) {
          <p>{{ subtitle }}</p>
        }
      </div>
      <ng-content />
    </header>
  `,
  styles: [
    `
      .page-header {
        display: flex;
        justify-content: space-between;
        align-items: flex-start;
        gap: 1rem;
        margin-bottom: 1.5rem;
      }
      h1 {
        margin: 0;
        font-size: 1.75rem;
      }
      p {
        margin: 0.35rem 0 0;
        color: #5b6472;
      }
    `
  ]
})
export class PageHeaderComponent {
  @Input({ required: true }) title!: string;
  @Input() subtitle = '';
}
