using System.Text.Json.Serialization;

namespace StudentSubjectManager.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public int Age { get; set; }
        public List<StudentSubject>? StudentSubjects { get; set; }
    }
}
