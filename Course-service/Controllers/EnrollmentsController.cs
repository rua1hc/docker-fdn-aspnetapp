using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Course_service.Models;
using MassTransit;
using Course_service.Models.DTOs;
using SharedModels;
using System.Net.Http;
using System.Text.Json;

namespace Course_service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly NetCourseDbContext _context;
        readonly IPublishEndpoint _publishEndpoint;
        private readonly IHttpClientFactory _httpClientFactory;

        public EnrollmentsController(NetCourseDbContext context,
                                    IPublishEndpoint publishEndpoint,
                                    IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _httpClientFactory = httpClientFactory;
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
            await _publishEndpoint.Publish<CourseEnrolled>(new
            {
                enrollment.Id,
                enrollment.UserId,
                enrollment.CourseId,
                DateTime.Now
            });
            return Ok(new CourseEnrolled()
            {
                Id = enrollment.Id,
                UserId = enrollment.UserId,
                CourseId = enrollment.CourseId,
                EnrolledDate = DateTime.Now
            });

            //_context.Enrollments.Add(enrollment);
            //await _context.SaveChangesAsync();

            //return CreatedAtAction("GetEnrollment", new { id = enrollment.Id }, enrollment);

            var httpClient = _httpClientFactory.CreateClient("UserApi");
            var httpResponseMessage = await httpClient.GetAsync($"api/users/{enrollment.UserId}");

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();

                var UserApiReturned = await JsonSerializer.DeserializeAsync<UserApiDto>(contentStream);
                return Ok(UserApiReturned);
            }
            else
            {
                return NotFound("UserId not found");
            }
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
    }
}
