import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { StudentFormComponent } from './components/student-form/student-form';
import { StudentListComponent } from './components/student-list/student-list';
import { StudentService } from './services/student.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    CommonModule,
    StudentFormComponent,
    StudentListComponent
  ],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {

  students: any[] = [];

  editIndex: number | null = null;

  editStudent: any = null;

  constructor(private studentService: StudentService) {
    this.students = this.studentService.getStudents();
  }

  addOrUpdateStudent(student: any) {

    if (this.editIndex !== null) {
      this.students[this.editIndex] = student;
      this.editIndex = null;
      this.editStudent = null;
    } else {
      this.students.push(student);
    }

    this.studentService.saveStudents(this.students);
  }

  deleteStudent(index: number) {
    this.students.splice(index, 1);

    this.studentService.saveStudents(this.students);
  }

  editStudentData(index: number) {
    this.editIndex = index;

    this.editStudent = {
      ...this.students[index]
    };
  }
}