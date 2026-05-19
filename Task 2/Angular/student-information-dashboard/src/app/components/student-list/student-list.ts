import {
    Component,
    Input,
    Output,
    EventEmitter
  } from '@angular/core';
  
  import { CommonModule } from '@angular/common';
  
  @Component({
    selector: 'app-student-list',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './student-list.html',
    styleUrl: './student-list.css'
  })
  export class StudentListComponent {
  
    @Input() students: any[] = [];
  
    @Output() deleteStudentEvent =
      new EventEmitter<number>();
  
    @Output() editStudentEvent =
      new EventEmitter<number>();
  
  }