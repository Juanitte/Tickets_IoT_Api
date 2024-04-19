using Common.Dtos;
using Common.Utilities;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Entities;

namespace Tickets.TicketsMicroservice.Models.Dtos.ResponseDto
{
    public class ResponseFilterTicketDto : GenericFilterDto
    {
        public ResponseFilterTicketDto()
        {
            this.Tickets = new List<TicketResumeDto>();
            FilterablesFields = Extensions.GetFilterablesFields<Ticket>();
        }

        public List<TicketResumeDto> Tickets { get; set; }
    }
}
