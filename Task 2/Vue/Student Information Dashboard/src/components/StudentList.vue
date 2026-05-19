<template>

  <div class="student-list">

    <h2>Student List</h2>

    <div
      class="cards-container"
      v-if="students.length > 0"
    >

      <div
        class="student-card"
        v-for="(student, index) in students"
        :key="index"
      >

        <h3>{{ student.name }}</h3>

        <p>
          <strong>Age:</strong>
          {{ student.age }}
        </p>

        <p>
          <strong>Course:</strong>
          {{ student.course }}
        </p>

        <p>
          <strong>Address:</strong>
          {{ student.address }}
        </p>

        <div class="button-group">

          <button
            class="edit-btn"
            @click="$emit('edit-student', index)"
          >
            Edit
          </button>

          <button
            class="delete-btn"
            @click="$emit('delete-student', index)"
          >
            Delete
          </button>

        </div>

      </div>

    </div>

    <p v-else>
      No students added yet.
    </p>

  </div>

</template>

<script>
export default {
  props: ['students']
};
</script>

<style scoped>

.student-list {
  margin-top: 30px;
}

.cards-container {
  display: grid;
  grid-template-columns:
    repeat(auto-fit, minmax(250px, 1fr));

  gap: 20px;
}

.student-card {
  background: #fafafa;
  border: 1px solid #ddd;
  border-radius: 10px;
  padding: 18px;
  box-shadow:
    0 2px 6px rgba(0, 0, 0, 0.08);
}

.student-card h3 {
  margin-top: 0;
  border-bottom: 1px solid #ddd;
  padding-bottom: 8px;
}

.button-group {
  display: flex;
  gap: 10px;
  margin-top: 15px;
}

.edit-btn,
.delete-btn {
  flex: 1;
  padding: 10px;
  border: none;
  border-radius: 6px;
  color: white;
  cursor: pointer;
}

.edit-btn {
  background: #3498db;
}

.delete-btn {
  background: #e74c3c;
}

</style>