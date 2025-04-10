using Microsoft.AspNetCore.Identity;

namespace FilmLibrary.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        //Дополнительные свойства не требуются, так как IdentityUserRole уже содержит UserId и RoleId
    }
}