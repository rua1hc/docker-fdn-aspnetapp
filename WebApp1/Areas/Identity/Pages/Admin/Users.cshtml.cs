using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WebApp1.Data;

namespace WebApp1.Areas.Identity.Pages.Admin
{
    public class UsersModel : PageModel
    {
        public ApplicationDbContext _DbCtx { get; set; }

        public IEnumerable<IdentityUser> Users { get; set; } = Enumerable.Empty<IdentityUser>();

        public UsersModel(ApplicationDbContext dbCtx)
        {
            _DbCtx = dbCtx;
        }

        public void OnGet()
        {
            Users = _DbCtx.Users.ToList();
        }

    }
}
