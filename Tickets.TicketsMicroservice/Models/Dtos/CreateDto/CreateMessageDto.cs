namespace Tickets.TicketsMicroservice.Models.Dtos.CreateDto
{
    public class CreateMessageDto
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }
        public int TicketId { get; set; }
        public bool IsTechnician { get; set; }

        public CreateMessageDto()
        {
            Author = string.Empty;
            Content = string.Empty;
            Attachments = new List<IFormFile?>();
            TicketId = 0;
        }

        public CreateMessageDto(string author, string content, int ticketId, bool isTechnician)
        {
            Author = author;
            Content = content;
            Attachments = new List<IFormFile?>();
            TicketId = ticketId;
            IsTechnician = isTechnician;
        }

        public CreateMessageDto(string author, string content, List<IFormFile?> attachments, int ticketId, bool isTechnician)
        {
            Author = author;
            Content = content;
            Attachments = attachments;
            TicketId = ticketId;
            IsTechnician = isTechnician;
        }
    }
}
