using Tickets.UsersMicroservice.Models.Entities;

namespace Tickets.UsersMicroservice.Models.Dtos.EntityDto
{
    public class UserWithRole
    {
        public User User { get; set; }
        public string Role { get; set; }

        public UserWithRole(User user, string role)
        {
            User = user;
            Role = role;
        }
    }
}
