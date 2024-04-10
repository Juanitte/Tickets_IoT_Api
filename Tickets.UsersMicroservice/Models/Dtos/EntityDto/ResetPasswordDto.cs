namespace Tickets.UsersMicroservice.Models.Dtos.EntityDto
{
    public class ResetPasswordDto
    {
        public string Username { get; set; }
        public string Domain { get; set; }
        public string Tld { get; set; }
        public string Password { get; set; }
    }
}
