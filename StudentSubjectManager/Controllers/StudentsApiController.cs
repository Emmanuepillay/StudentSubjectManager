using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using StudentSubjectManager.Data;
using StudentSubjectManager.Entities;

namespace StudentSubjectManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsApiController: ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public StudentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/StudentsApi
        [HttpGet]
        public async Task<IList<Student>> GetStudents()
        {

            return await _context.Students.Include(s => s.StudentSubjects).ThenInclude(ss => ss.Subject).ToListAsync();
        }

        // GET: api/StudentsApi/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.Include(s => s.StudentSubjects).ThenInclude(ss=> ss.Subject).FirstOrDefaultAsync(s => s.Id == id);

            if (student is null)
            {
                return NotFound();
            }

            return student;
        }

        // PUT: api/StudentsApi/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] Student student)
        {
            if (id != student.Id)
            {
                return BadRequest("Mismatched student ID");
            }

            var existingStudent = await _context.Students.Include(s => s.StudentSubjects)
                                                         .FirstOrDefaultAsync(s => s.Id == id);

            if (existingStudent == null)
            {
                return NotFound();
            }

            existingStudent.Name = student.Name;
            existingStudent.Age = student.Age;

            // Handle the StudentSubjects associations
            _context.StudentSubjects.RemoveRange(existingStudent.StudentSubjects);
            if (student.StudentSubjects != null && student.StudentSubjects.Any())
            {
                foreach (var ss in student.StudentSubjects)
                {
                    existingStudent.StudentSubjects.Add(new StudentSubject
                    {
                        StudentId = existingStudent.Id,
                        SubjectId = ss.SubjectId
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await StudentExistsAsync(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        // POST: api/StudentsApi
        [HttpPost]
        public async Task<ActionResult<Student>> CreateStudent([FromBody] Student student)
        {
            if (student.StudentSubjects != null && student.StudentSubjects.Any())
            {
                // Ensure that all subject IDs provided are valid.
                foreach (var ss in student.StudentSubjects)
                {
                    var subjectExists = await _context.Subjects.AnyAsync(s => s.Id == ss.SubjectId);
                    if (!subjectExists)
                    {
                        return BadRequest($"Subject with ID {ss.SubjectId} does not exist.");
                    }
                }

                // The subject associations are valid and we can proceed with adding the student.
                _context.Students.Add(student);
            }
            else
            {
                // If there are no subject associations, simply add the student.
                _context.Students.Add(student);
            }
                await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetStudent), new { id = student.Id }, student);
        }


        // DELETE: api/StudentsApi/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student is null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return Ok(student);
        }

        [HttpPost("{studentId}/subjects")]
        public async Task<IActionResult> AddSubjectToStudent(int studentId, [FromBody] int subjectId)
        {
            // Check if student exists
            var student = await _context.Students.FindAsync(studentId);
            if (student == null)
            {
                return NotFound("Student not found.");
            }

            // Check if subject exists
            var subject = await _context.Subjects.FindAsync(subjectId);
            if (subject == null)
            {
                return NotFound("Subject not found.");
            }

            // Check if the association already exists
            var existingAssociation = await _context.StudentSubjects
                .FirstOrDefaultAsync(ss => ss.StudentId == studentId && ss.SubjectId == subjectId);
            if (existingAssociation != null)
            {
                return BadRequest("This student is already associated with the given subject.");
            }

            // Create the association
            var studentSubject = new StudentSubject
            {
                StudentId = studentId,
                SubjectId = subjectId
            };

            _context.StudentSubjects.Add(studentSubject);
            await _context.SaveChangesAsync();

            return Ok("Subject added to student successfully.");
        }

        private async Task<bool> StudentExistsAsync(int id)
        {
            return await _context.Students.AnyAsync(e => e.Id == id);
        }
    }
}
