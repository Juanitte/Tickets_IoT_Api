using Common.Utilities;
using static Common.Attributes.ModelAttributes;

namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class TicketResumeDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Priorities Priority { get; set; }
        public Status Status { get; set; }
        public DateTime Timestamp { get; set; }
        public int UserId { get; set; }
        public string FullName { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (TicketResumeDto)obj;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
