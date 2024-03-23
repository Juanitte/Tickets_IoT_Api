using Tickets.MessagesMicroservice.Models.Dtos.EntityDto;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;

namespace Tickets.TicketsMicroservice.Models.Dtos.CreateDto
{
    public class CreateTicketDto
    {
        public TicketDto TicketDto { get; set; }
        public TicketMessageDto MessageDto { get; set; }

        public CreateTicketDto()
        {
            this.TicketDto = new TicketDto();
            this.MessageDto = new TicketMessageDto();
        }
        public CreateTicketDto(TicketDto ticket, TicketMessageDto message)
        {
            this.TicketDto = ticket;
            this.MessageDto = message;
        }
    }
}
