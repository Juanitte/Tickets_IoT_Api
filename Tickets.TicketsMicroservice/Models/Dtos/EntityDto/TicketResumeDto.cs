using static Common.Attributes.ModelAttributes;

namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class TicketResumeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Priority { get; set; }
        public string State { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
    }
}
