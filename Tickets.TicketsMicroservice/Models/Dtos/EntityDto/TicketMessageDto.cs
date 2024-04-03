namespace Tickets.MessagesMicroservice.Models.Dtos.EntityDto
{
    public class TicketMessageDto
    {
        public string Author { get; set; }
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }

        public TicketMessageDto()
        {
            Author = string.Empty;
            Content = string.Empty;
            Attachments = new List<IFormFile?>();
        }

        public TicketMessageDto(string author, string content)
        {
            Author = author;
            Content = content;
            Attachments = new List<IFormFile?>();
        }

        public TicketMessageDto(string author, string content, List<IFormFile?> attachments)
        {
            Author = author;
            Content = content;
            Attachments = attachments;
        }
    }
}
