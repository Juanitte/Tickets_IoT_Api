using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Common.Utilities;
using static Common.Attributes.ModelAttributes;
using Tickets.TicketsMicroservice.Models.Dtos.EntityDto;

namespace Tickets.TicketsMicroservice.Models.Entities
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Filters]
        public string Title { get; set; }

        [Filters]
        public string Name { get; set; }

        [Filters]
        public string Email { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string Priority { get; set; }
        public string State { get; set; }

        public bool IsAsigned { get; set; }
        public bool HasNewMessages { get; set; }
        public int newMessagesCount { get; set; }
        public List<Message?> Messages { get; set; } = new List<Message?>();

        public Ticket()
        {
            this.Title = string.Empty;
            this.Name = string.Empty;
            this.Email = string.Empty;
            this.Timestamp = DateTime.Now;
            this.UserId = -1;
            this.Priority = Priorities.NOT_SURE.ToString();
            this.State = States.PENDING.ToString();
            this.IsAsigned = false;
            this.HasNewMessages = true;
            this.newMessagesCount = 1;
        }
        public Ticket(string title, string name, string email)
        {
            this.Title = title;
            this.Name = name;
            this.Email = email;
            this.Timestamp = DateTime.Now;
            this.UserId = -1;
            this.Priority = Priorities.NOT_SURE.ToString();
            this.State = States.PENDING.ToString();
            this.IsAsigned = false;
            this.HasNewMessages = true;
            this.newMessagesCount = 1;
        }

        public Ticket(Message message)
        {
            this.Title = string.Empty;
            this.Name = string.Empty;
            this.Email = string.Empty;
            this.Timestamp = DateTime.Now;
            this.UserId = -1;
            this.Priority = Priorities.NOT_SURE.ToString();
            this.State = States.PENDING.ToString();
            this.Messages.Add(message);
            this.IsAsigned = false;
            this.HasNewMessages = true;
            this.newMessagesCount = 1;
        }
        public Ticket(string title, string name, string email, Message message)
        {
            this.Title = title;
            this.Name = name;
            this.Email = email;
            this.Timestamp = DateTime.Now;
            this.UserId = -1;
            this.Priority = Priorities.NOT_SURE.ToString();
            this.State = States.PENDING.ToString();
            this.Messages.Add(message);
            this.IsAsigned = false;
            this.HasNewMessages = true;
            this.newMessagesCount = 1;
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
