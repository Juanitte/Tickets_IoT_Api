namespace Tickets.TicketsMicroservice.Models.Dtos.CreateDto
{
    public class CreateTicketDataDto
    {
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public bool HasNewMessages { get; set; }
        public int NewMessagesCount { get; set; }

        public CreateTicketDataDto()
        {
            Title = string.Empty;
            Name = string.Empty;
            Email = string.Empty;
            HasNewMessages = true;
            NewMessagesCount = 1;
        }

        public CreateTicketDataDto(string title, string name, string email, int newMessagesCount)
        {
            Title = title;
            Name = name;
            Email = email;
            HasNewMessages = true;
            NewMessagesCount = newMessagesCount;

        }
    }
}
