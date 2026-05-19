<template>

  <div class="container">

    <h1>Student Dashboard</h1>

    <StudentForm
      :editStudent="editStudent"
      @student-added="addOrUpdateStudent"
    />

    <StudentList
      :students="students"
      @delete-student="deleteStudent"
      @edit-student="editStudentData"
    />

  </div>

</template>

<script>

import StudentForm from './components/StudentForm.vue';

import StudentList from './components/StudentList.vue';

import {
  getStudents,
  saveStudents
} from './services/localStorageService';

export default {

  components: {
    StudentForm,
    StudentList
  },

  data() {
    return {

      students: [],

      editIndex: null,

      editStudent: null
    };
  },

  mounted() {
    this.students = getStudents();
  },

  methods: {

    addOrUpdateStudent(student) {

      if (this.editIndex !== null) {

        this.students[this.editIndex] = student;

        this.editIndex = null;

        this.editStudent = null;

      } else {

        this.students.push(student);
      }

      saveStudents(this.students);
    },

    deleteStudent(index) {

      this.students.splice(index, 1);

      saveStudents(this.students);
    },

    editStudentData(index) {

      this.editIndex = index;

      this.editStudent = {
        ...this.students[index]
      };
    }
  }
};

</script>

<style>

body {
  margin: 0;
  padding: 0;
  font-family: Arial, sans-serif;
  background: #f4f4f4;
}

.container {
  width: 80%;
  max-width: 900px;
  margin: 40px auto;
  background: white;
  padding: 25px;
  border-radius: 12px;

  box-shadow:
    0 2px 10px rgba(0,0,0,0.1);
}

h1,
h2 {
  text-align: center;
}

</style>