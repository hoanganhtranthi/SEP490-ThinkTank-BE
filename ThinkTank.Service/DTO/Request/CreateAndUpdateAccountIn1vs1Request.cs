
using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Service.DTO.Request
{
    public class CreateAndUpdateAccountIn1vs1Request
    {
        [Range(20, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Coin { get; set; }
        public int WinnerId { get; set; }
        public int AccountId1 { get; set; }
        public int AccountId2 { get; set; }
        public int GameId { get; set; }
        public string RoomOfAccountIn1vs1Id { get; set; }
    }
}
