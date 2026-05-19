function StudentList({
    students,
    deleteStudent,
    handleEdit,
  }) {
    return (
      <div className="student-list">
        <h2>Student List</h2>
  
        {students.length === 0 ? (
          <p>No students added yet.</p>
        ) : (
          <div className="cards-container">
            {students.map((student, index) => (
              <div className="student-card" key={index}>
                <h3>{student.name}</h3>
  
                <div className="student-info">
                  <p>
                    <strong>Age:</strong> {student.age}
                  </p>
  
                  <p>
                    <strong>Course:</strong> {student.course}
                  </p>
  
                  <p>
                    <strong>Address:</strong> {student.address}
                  </p>
                </div>
  
                <div className="button-group">
                  <button
                    className="edit-btn"
                    onClick={() => handleEdit(index)}
                  >
                    Edit
                  </button>
  
                  <button
                    className="delete-btn"
                    onClick={() => deleteStudent(index)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    );
  }
  
  export default StudentList;