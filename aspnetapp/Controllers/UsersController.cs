using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly NetUserDbContext _context;

    public UsersController(NetUserDbContext context)
    {
        _context = context;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> Getusers()
    {
        return await _context.Users.ToListAsync();
    }

    // GET: api/users/1
    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Getuser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        return user == null ? NotFound() : Ok(user);
    }

    // POST: api/users
    [HttpPost]
    public async Task<ActionResult<User>> Adduser(User user)
    {
        var userDb = await _context.Users.AnyAsync(x => x.Email == user.Email);
        if (userDb) return BadRequest();

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Getuser), new { id = user.Id }, user);
    }

    // PUT: api/users/2
    [HttpPut("{id}")]
    public async Task<IActionResult> Updateuser(int id, User user)
    {
        if (id != user.Id) return BadRequest();

        var userDb = await _context.Users.FindAsync(id);
        if (userDb == null) return NotFound();

        userDb.FirstName = user.FirstName;
        userDb.LastName = user.LastName;
        userDb.Email = user.Email;
        userDb.Phone = user.Phone;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!IsUserExisted(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    // DELETE: api/users/3
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deleteuser(int id)
    {
        var userDb = await _context.Users.FindAsync(id);
        if (userDb == null) return NotFound();

        _context.Users.Remove(userDb);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool IsUserExisted(int id)
    {
        return _context.Users.Any(e => e.Id == id);
    }

    private static UserDto User2Dto(User user) => new UserDto
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email
    };

}
