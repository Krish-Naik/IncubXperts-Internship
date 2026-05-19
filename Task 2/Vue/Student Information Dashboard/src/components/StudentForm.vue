<template>
  <form class="student-form">

    <input
      type="text"
      placeholder="Enter Name"
      v-model="student.name"
    />

    <input
      type="number"
      placeholder="Enter Age"
      v-model="student.age"
    />

    <input
      type="text"
      placeholder="Enter Course"
      v-model="student.course"
    />

    <input
      type="text"
      placeholder="Enter Address"
      v-model="student.address"
    />

    <p class="error" v-if="error">
      {{ error }}
    </p>

    <button type="button" @click="submitForm">
      {{ editStudent ? 'Update Student' : 'Add Student' }}
    </button>

  </form>
</template>

<script>
export default {

  props: ['editStudent'],

  data() {
    return {
      student: {
        name: '',
        age: '',
        course: '',
        address: ''
      },

      error: ''
    };
  },

  watch: {
    editStudent: {
      immediate: true,

      handler(newValue) {
        if (newValue) {
          this.student = { ...newValue };
        }
      }
    }
  },

  methods: {

    submitForm() {

      if (Number(this.student.age) < 0) {
        this.error = 'Age cannot be negative';
        return;
      }

      this.error = '';

      this.$emit('student-added', {
        ...this.student
      });

      this.student = {
        name: '',
        age: '',
        course: '',
        address: ''
      };
    }
  }
};
</script>

<style scoped>
.student-form {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.student-form input {
  padding: 12px;
  border: 1px solid #ccc;
  border-radius: 6px;
}

.student-form button {
  padding: 12px;
  background: black;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
}

.error {
  color: red;
}
</style>