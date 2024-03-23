namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class AttachmentDto
    {
        public string Path { get; set; }
        public int MessageID { get; set; }

        public AttachmentDto()
        {
            this.Path = string.Empty;
            this.MessageID = 0;
        }

        public AttachmentDto(string path, int messageID)
        {
            this.Path = path;
            this.MessageID = messageID;
        }
    }
}
