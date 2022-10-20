using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace WebApp1.Config
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<IdentityUser, UserViewModel>();
        }
    }

    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        //public string UserName { get; set; }
        public string Email { get; set; } = null!;
    }
}
