namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class TicketDto
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasNewMessages { get; set; }

        public TicketDto()
        {
            this.Title = string.Empty;
            this.Name = string.Empty;
            this.Email = string.Empty;
            this.HasNewMessages = true;
        }

        public TicketDto(string title, string name, string email, bool hasNewMessages)
        {
            this.Title = title;
            this.Name = name;
            this.Email = email;
            this.HasNewMessages = hasNewMessages;
        }
    }
}
