namespace Tickets.TicketsMicroservice.Models.Dtos.CreateDto
{
    public class CreateAttachmentDto
    {
        public string Path { get; set; }
        public int MessageID { get; set; }

        public CreateAttachmentDto()
        {
            Path = string.Empty;
            MessageID = 0;
        }

        public CreateAttachmentDto(string path, int messageID)
        {
            Path = path;
            MessageID = messageID;
        }
    }
}
