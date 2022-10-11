using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace aspnetapp.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactController : ControllerBase
{
    private readonly DotNetTraining _context;

    public ContactController(DotNetTraining context)
    {
        _context = context;
    }

    // GET: api/contact
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
    {
        return await _context.Contacts.ToListAsync();
    }

    // GET: api/contact/1
    [HttpGet("{id}")]
    public async Task<ActionResult<Contact>> GetContact(int id)
    {
        var contact = await _context.Contacts.FindAsync(id);
        return contact == null ? NotFound() : Ok(contact);
    }

    // POST: api/contact
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPost]
    public async Task<ActionResult<Contact>> AddContact(Contact contact)
    {
        var contactDb = await _context.Contacts.AnyAsync(x => x.Email == contact.Email);
        if (contactDb) return BadRequest();

        _context.Contacts.Add(contact);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
    }

    // PUT: api/contact/2
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateContact(int id, Contact contact)
    {
        if (id != contact.Id) return BadRequest();

        var contactDb = await _context.Contacts.FindAsync(id);
        if (contactDb == null) return NotFound();

        contactDb.FirstName = contact.FirstName;
        contactDb.LastName = contact.LastName;
        contactDb.Email = contact.Email;
        contactDb.Phone = contact.Phone;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!ContactExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/contact/3
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteContact(int id)
    {
        var contactDb = await _context.Contacts.FindAsync(id);
        if (contactDb == null) return NotFound();

        _context.Contacts.Remove(contactDb);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ContactExists(int id)
    {
        return _context.Contacts.Any(e => e.Id == id);
    }

    private static ContactDTO Contact2DTO(Contact contact)
        => new ContactDTO
        {
            Id = contact.Id,
            FirstName = contact.FirstName,
            LastName = contact.LastName,
            Email = contact.Email
        };

}
