using Microsoft.AspNetCore.Identity;

namespace FilmLibrary.Models
{
    public class UserRole : IdentityUserRole<string>
    {
        //�������������� �������� �� ���������, ��� ��� IdentityUserRole ��� �������� UserId � RoleId
    }
}