namespace Tickets.UsersMicroservice.Models.Dtos.CreateDto
{
    public class CreateEditRemoveResponseDto
    {
        public int Id { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }

        #region Constructors

        public CreateEditRemoveResponseDto()
        {
            Errors = new List<string>();
        }

        #endregion

        public void IsSuccess(int id)
        {
            Id = id;
            Success = true;
        }
    }
}
