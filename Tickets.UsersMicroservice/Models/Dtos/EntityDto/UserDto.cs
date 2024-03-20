namespace Tickets.UsersMicroservice.Models.Dtos.EntityDto
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int LanguageId { get; set; }
    }
}
