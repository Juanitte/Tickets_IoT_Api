namespace Tickets.UsersMicroservice.Models.Dtos
{
    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public LoginDto()
        {
            this.Email = string.Empty;
            this.Password = string.Empty;
        }

        public LoginDto(string email, string password)
        {
            Email = email;
            Password = password;
        }
    }
}
