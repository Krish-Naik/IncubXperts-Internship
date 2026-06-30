import { Component, Input } from '@angular/core';

export interface TableColumn<T> {
  key: keyof T & string;
  label: string;
}

@Component({
  selector: 'app-data-table',
  standalone: true,
  template: `
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            @for (column of columns; track column.key) {
              <th>{{ column.label }}</th>
            }
            @if (actions) {
              <th>Actions</th>
            }
          </tr>
        </thead>
        <tbody>
          @for (row of rows; track trackBy(row)) {
            <tr>
              @for (column of columns; track column.key) {
                <td>{{ row[column.key] }}</td>
              }
              @if (actions) {
                <td class="actions">
                  <ng-content />
                </td>
              }
            </tr>
          } @empty {
            <tr>
              <td [attr.colspan]="columns.length + (actions ? 1 : 0)">No records found.</td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styles: [
    `
      .table-wrap {
        overflow: auto;
        border: 1px solid #dbe2ea;
        border-radius: 12px;
        background: #fff;
      }
      table {
        width: 100%;
        border-collapse: collapse;
      }
      th,
      td {
        padding: 0.85rem 1rem;
        text-align: left;
        border-bottom: 1px solid #edf1f5;
      }
      th {
        background: #f7f9fc;
        font-size: 0.85rem;
        text-transform: uppercase;
        letter-spacing: 0.04em;
        color: #5b6472;
      }
      .actions {
        white-space: nowrap;
      }
    `
  ]
})
export class DataTableComponent<T extends Record<string, unknown>> {
  @Input({ required: true }) columns!: TableColumn<T>[];
  @Input({ required: true }) rows: T[] = [];
  @Input() actions = false;
  @Input() rowIdKey: keyof T & string = 'id' as keyof T & string;

  trackBy(row: T): string {
    return String(row[this.rowIdKey]);
  }
}
