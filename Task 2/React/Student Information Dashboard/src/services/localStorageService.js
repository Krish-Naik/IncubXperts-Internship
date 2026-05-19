const STORAGE_KEY = "students";

export const saveStudents = (students) => {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(students));
};

export const getStudents = () => {
  const students = localStorage.getItem(STORAGE_KEY);

  return students ? JSON.parse(students) : [];
};