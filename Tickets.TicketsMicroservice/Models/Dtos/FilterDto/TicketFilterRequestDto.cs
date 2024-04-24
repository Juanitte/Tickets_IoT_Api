using Common.Dtos;
using Common.Utilities;

namespace Tickets.TicketsMicroservice.Models.Dtos.FilterDto
{
    public class TicketFilterRequestDto : GenericFilterRequestDto
    {
        public int UserId { get; set; }
        public Priorities Priority { get; set; }
        public Status Status { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
