import {
    Component,
    EventEmitter,
    Input,
    Output,
    OnChanges
  } from '@angular/core';
  
  import { FormsModule } from '@angular/forms';
  import { CommonModule } from '@angular/common';
  
  @Component({
    selector: 'app-student-form',
    standalone: true,
    imports: [FormsModule, CommonModule],
    templateUrl: './student-form.html',
    styleUrl: './student-form.css'
  })
  export class StudentFormComponent implements OnChanges {
  
    @Input() editStudent: any;
  
    @Output() studentAdded = new EventEmitter<any>();
  
    student = {
      name: '',
      age: '',
      course: '',
      address: ''
    };
  
    error = '';
  
    ngOnChanges() {
      if (this.editStudent) {
        this.student = { ...this.editStudent };
      }
    }
  
    submitForm() {
  
      if (Number(this.student.age) < 0) {
        this.error = 'Age cannot be negative';
        return;
      }
  
      this.error = '';
  
      this.studentAdded.emit(this.student);
  
      this.student = {
        name: '',
        age: '',
        course: '',
        address: ''
      };
    }
  }