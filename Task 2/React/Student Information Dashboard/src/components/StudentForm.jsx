import { useEffect, useState } from "react";

function StudentForm({ addStudent, editStudent }) {
  const [formData, setFormData] = useState({
    name: "",
    age: "",
    course: "",
    address: "",
  });

  const [error, setError] = useState("");

  useEffect(() => {
    if (editStudent) {
      setFormData(editStudent);
    }
  }, [editStudent]);

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = (e) => {
    e.preventDefault();

    // AGE VALIDATION
    if (Number(formData.age) < 0) {
      setError("Age cannot be negative");
      return;
    }

    setError("");

    addStudent(formData);

    setFormData({
      name: "",
      age: "",
      course: "",
      address: "",
    });
  };

  return (
    <form className="student-form" onSubmit={handleSubmit}>
      <input
        type="text"
        name="name"
        placeholder="Enter Name"
        value={formData.name}
        onChange={handleChange}
        required
      />

      <input
        type="number"
        name="age"
        placeholder="Enter Age"
        value={formData.age}
        onChange={handleChange}
        required
      />

      <input
        type="text"
        name="course"
        placeholder="Enter Course"
        value={formData.course}
        onChange={handleChange}
        required
      />

      <input
        type="text"
        name="address"
        placeholder="Enter Address"
        value={formData.address}
        onChange={handleChange}
        required
      />

      {error && <p className="error">{error}</p>}

      <button type="submit">
        {editStudent ? "Update Student" : "Add Student"}
      </button>
    </form>
  );
}

export default StudentForm;