using Microsoft.AspNetCore.Identity;

namespace FilmLibrary.Models
{
    public class Role : IdentityRole
    {
        public Role() : base()
        {
        }

        public Role(string roleName) : base(roleName)
        {
        }
    }
}