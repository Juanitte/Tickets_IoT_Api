using Common.Utilities;

namespace Tickets.UsersMicroservice.Models.Dtos.CreateDto
{
    public class CreateUserDto
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int RoleId { get; set; }
        public Language Language { get; set; }

    }
}
