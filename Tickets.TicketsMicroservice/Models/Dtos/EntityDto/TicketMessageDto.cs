namespace Tickets.MessagesMicroservice.Models.Dtos.EntityDto
{
    public class TicketMessageDto
    {
        public string Content { get; set; }
        public List<IFormFile?> Attachments { get; set; }

        public TicketMessageDto()
        {
            Content = string.Empty;
            Attachments = new List<IFormFile?>();
        }

        public TicketMessageDto(string content)
        {
            Content = content;
            Attachments = new List<IFormFile?>();
        }

        public TicketMessageDto(string content, List<IFormFile?> attachments)
        {
            Content = content;
            Attachments = attachments;
        }
    }
}
