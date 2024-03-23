using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class AccountInRoomResponse
    {
        [Key]
        public int Id { get; set; }
        [BooleanAttribute]
        public bool? IsAdmin { get; set; }
        [IntAttribute]
        public int? AccountId { get; set; }
        public string? Username { get; set; }
        [IntAttribute]
        public int? RoomId { get; set; }
        public DateTime? CompletedTime { get; set; }
        public decimal? Duration { get; set; }
        public int? Mark { get; set; }
        public int? PieceOfInformation { get; set; }
       
    }
}
