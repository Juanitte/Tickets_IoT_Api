namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class AttachmentDto
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public int MessageId { get; set; }

        public AttachmentDto()
        {
            this.Id = 0;
            this.Path = string.Empty;
            this.MessageId = 0;
        }

    }
}
