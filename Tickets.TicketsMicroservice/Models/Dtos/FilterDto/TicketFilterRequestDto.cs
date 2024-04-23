using Common.Dtos;

namespace Tickets.TicketsMicroservice.Models.Dtos.FilterDto
{
    public class TicketFilterRequestDto : GenericFilterRequestDto
    {
        public int UserId { get; set; }
        public int Priority { get; set; }
        public int Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
