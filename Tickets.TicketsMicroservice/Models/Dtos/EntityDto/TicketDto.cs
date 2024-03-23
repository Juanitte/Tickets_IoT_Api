namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class TicketDto
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public TicketDto()
        {
            this.Title = string.Empty;
            this.Name = string.Empty;
            this.Email = string.Empty;
        }

        public TicketDto(string title, string name, string email)
        {
            this.Title = title;
            this.Name = name;
            this.Email = email;
        }
    }
}
