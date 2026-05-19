import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class StudentService {

  private storageKey = 'students';

  getStudents() {
    const data = localStorage.getItem(this.storageKey);

    return data ? JSON.parse(data) : [];
  }

  saveStudents(students: any[]) {
    localStorage.setItem(this.storageKey, JSON.stringify(students));
  }
}