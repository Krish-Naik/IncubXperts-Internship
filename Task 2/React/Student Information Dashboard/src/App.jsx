import { useEffect, useState } from "react";
import StudentForm from "./components/StudentForm";
import StudentList from "./components/StudentList";
import {
  saveStudents,
  getStudents,
} from "./services/localStorageService";

function App() {
  const [students, setStudents] = useState([]);
  const [editStudent, setEditStudent] = useState(null);

  useEffect(() => {
    const storedStudents = getStudents();
    setStudents(storedStudents);
  }, []);

  // ADD OR UPDATE STUDENT
  const addStudent = (student) => {
    let updatedStudents;

    if (editStudent !== null) {
      updatedStudents = students.map((s, index) =>
        index === editStudent ? student : s
      );

      setEditStudent(null);
    } else {
      updatedStudents = [...students, student];
    }

    setStudents(updatedStudents);
    saveStudents(updatedStudents);
  };

  // DELETE STUDENT
  const deleteStudent = (index) => {
    const updatedStudents = students.filter((_, i) => i !== index);

    setStudents(updatedStudents);
    saveStudents(updatedStudents);
  };

  // EDIT STUDENT
  const handleEdit = (index) => {
    setEditStudent(index);
  };

  return (
    <div className="container">
      <h1>Student Dashboard</h1>

      <StudentForm
        addStudent={addStudent}
        editStudent={
          editStudent !== null ? students[editStudent] : null
        }
      />

      <StudentList
        students={students}
        deleteStudent={deleteStudent}
        handleEdit={handleEdit}
      />
    </div>
  );
}

export default App;