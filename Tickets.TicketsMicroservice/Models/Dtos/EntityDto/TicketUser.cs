using Common.Utilities;
using System.ComponentModel.DataAnnotations.Schema;
using Tickets.TicketsMicroservice.Models.Entities;
using static Common.Attributes.ModelAttributes;

namespace Tickets.TicketsMicroservice.Models.Dtos.EntityDto
{
    public class TicketUser
    {
        public int Id { get; set; }
        [Filters]
        public string Title { get; set; }
        [Filters]
        public string Name { get; set; }
        [Filters]
        public string Email { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public Priorities Priority { get; set; }
        public Status Status { get; set; }
        public bool IsAssigned { get; set; }
        public bool HasNewMessages { get; set; }
        public int NewMessagesCount { get; set; }
        [Column("FullName")]
        [Filters]
        public string FullName { get; set; }

        public TicketUser()
        {
            this.Id = -1;
            this.Title = string.Empty;
            this.Name = string.Empty;
            this.Email = string.Empty;
            this.Timestamp = DateTime.Now;
            this.UserId = -1;
            this.Priority = Priorities.NOT_SURE;
            this.Status = Status.PENDING;
            this.IsAssigned = false;
            this.HasNewMessages = false;
            this.NewMessagesCount = 0;
            this.FullName = string.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (TicketUser)obj;

            return Id == other.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        ///     Convierte el modelo en un objeto dto
        /// </summary>
        /// <returns></returns>
        public TicketResumeDto ToResumeDto()
        {
            return this.ConvertModel(new TicketResumeDto());
        }
    }
}
