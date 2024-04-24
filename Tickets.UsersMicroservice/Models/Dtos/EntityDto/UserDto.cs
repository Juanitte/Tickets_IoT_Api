namespace Tickets.UsersMicroservice.Models.Dtos.EntityDto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public int Language { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }

        public UserDto() 
        {
            Id = 0;
            FullName = string.Empty;
            Language = 0;
            UserName = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Role = string.Empty;
        }
    }
}
