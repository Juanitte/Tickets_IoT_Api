namespace Tickets.UsersMicroservice.Models.Dtos.EntityDto
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginDto()
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        public LoginDto(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
