using Common.Dtos;
using Tickets.UsersMicroservice.Models.Dtos.EntityDto;

namespace Tickets.UsersMicroservice.Models.Dtos.ResponseDto
{
    public class ResponseFilterUserDto : GenericFilterRequestDto
    {
        public IEnumerable<UserDto> Users { get; set; }
    }
}
