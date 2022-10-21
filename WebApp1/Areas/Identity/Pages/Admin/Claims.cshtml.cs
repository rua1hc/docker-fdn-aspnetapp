using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

using System.Collections.Generic;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Linq;


namespace WebApp1.Areas.Identity.Pages.Admin
{

    public class ClaimsModel : PageModel
    {
        public ClaimsModel(UserManager<IdentityUser> mgr)
        {
            UserManager = mgr;
        }


        public UserManager<IdentityUser> UserManager { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } = null!;

        public IEnumerable<Claim>? Claims { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(Id))
            {
                //Redirect to NotFound
                return RedirectToPage("/");
            }
            IdentityUser user = await UserManager.FindByIdAsync(Id);
            Claims = await UserManager.GetClaimsAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAddClaimAsync([Required] string type,
                                                             [Required] string value)
        {

            IdentityUser user = await UserManager.FindByIdAsync(Id);

            if (ModelState.IsValid)
            {
                var claim = new Claim(type, value);
                var result = await UserManager.AddClaimAsync(user, claim);
                if (!result.Succeeded)
                {
                    foreach (var err in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, err.Description);
                    }
                }
            }
            Claims = await UserManager.GetClaimsAsync(user);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEditClaimAsync([Required] string type,
                                                              [Required] string value,
                                                              [Required] string oldValue)
        {
            IdentityUser user = await UserManager.FindByIdAsync(Id);
            if (ModelState.IsValid)
            {
                var claimNew = new Claim(type, value);
                var claimOld = new Claim(type, oldValue);
                await UserManager.ReplaceClaimAsync(user, claimOld, claimNew);
            }
            Claims = await UserManager.GetClaimsAsync(user);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteClaimAsync([Required] string type,
                                                                [Required] string value)
        {
            IdentityUser user = await UserManager.FindByIdAsync(Id);
            if (ModelState.IsValid)
            {
                var claim = new Claim(type, value);
                await UserManager.RemoveClaimAsync(user, claim);
            }
            Claims = await UserManager.GetClaimsAsync(user);
            return RedirectToPage();

        }
    }
}

