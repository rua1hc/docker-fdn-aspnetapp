using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Course_service.Models;
using MassTransit;
using Course_service.Models.DTOs;
using SharedModels;
using System.Net.Http;
using System.Text.Json;
using static MassTransit.ValidationResultExtensions;

namespace Course_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly NetCourseDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        readonly IPublishEndpoint _publishEndpoint;
        readonly ILogger<EnrollmentsController> _logger;

        public EnrollmentsController(NetCourseDbContext context,
                                    IHttpClientFactory httpClientFactory,
                                    IPublishEndpoint publishEndpoint,
                                    ILogger<EnrollmentsController> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        // GET: api/Enrollments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
        {
            return await _context.Enrollments.ToListAsync();
        }

        // GET: api/Enrollments/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Enrollment>> GetEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);

            if (enrollment == null)
            {
                return NotFound();
            }

            return enrollment;
        }

        // PUT: api/Enrollments/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEnrollment(int id, Enrollment enrollment)
        {
            if (id != enrollment.Id)
            {
                return BadRequest();
            }

            _context.Entry(enrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnrollmentExists(id))
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

        // POST: api/Enrollments
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult> PostEnrollment(EnrollmentDto enrollment)
        {
            string userName, userEmail; //from UsersApi

            var httpClient = _httpClientFactory.CreateClient("UsersApi");
            var httpResponseMessage = await httpClient.GetAsync($"api/users/{enrollment.UserId}");
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                var user = await JsonSerializer.DeserializeAsync<UsersApiReturnedDto>(contentStream);
                if (user == null) return NotFound("UserId not found.");
                userName = user.UserName;
                userEmail = user.Email;
            }
            else
            {
                return BadRequest(new { error = $"Error: Users-Service returned {httpResponseMessage.StatusCode}." });
            }

            var course = await _context.Courses.FindAsync(enrollment.CourseId);
            if (course == null) return NotFound("CourseId not found.");

            var newEnrollment = Dto2Enrollment(enrollment);
            _context.Enrollments.Add(newEnrollment);
            await _context.SaveChangesAsync();

            _logger.LogInformation("CourseInfo: {Code}-{Price}", course.Code, course.Price);
            await _publishEndpoint.Publish<CourseEnrolled>(new
            {
                UserName = userName,
                UserEmail = userEmail,
                CourseName = course.Code,
                CoursePrice = course.Price,
                EnrolledDate = newEnrollment.EnrolledDate
            });

            return CreatedAtAction("GetEnrollment", new { id = newEnrollment.Id }, newEnrollment);
        }

        // DELETE: api/Enrollments/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnrollment(int id)
        {
            var enrollment = await _context.Enrollments.FindAsync(id);
            if (enrollment == null)
            {
                return NotFound();
            }

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EnrollmentExists(int id)
        {
            return _context.Enrollments.Any(e => e.Id == id);
        }
        private static Enrollment Dto2Enrollment(EnrollmentDto e) => new Enrollment
        {
            UserId = e.UserId,
            CourseId = e.CourseId,
            EnrolledDate = DateTime.Now
        };

    }
}
