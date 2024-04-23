using Common.Utilities;

namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public Priorities Priority { get; set; }
        public Status Status { get; set; }
        public bool IsAsigned { get; set; }
        public bool HasNewMessages { get; set; }
        public int NewMessagesCount { get; set; }
        public List<MessageDto?> Messages { get; set; } = new List<MessageDto?>();

        public TicketDto()
        {
            this.Id = 0;
            this.Title = string.Empty;
            this.Name = string.Empty;
            this.Email = string.Empty;
            this.Timestamp = DateTime.Now;
            this.UserId = 0;
            this.Priority = Priorities.NOT_SURE;
            this.Status = Status.PENDING;
            this.IsAsigned = false;
            this.HasNewMessages = false;
            this.NewMessagesCount = 0;
        }
    }
}
