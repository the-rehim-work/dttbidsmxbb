using Microsoft.AspNetCore.Identity;

namespace dttbidsmxbb.Models
{
    public class AppUser : IdentityUser<int>
    {
        public string? FullName { get; set; }
    }
}
