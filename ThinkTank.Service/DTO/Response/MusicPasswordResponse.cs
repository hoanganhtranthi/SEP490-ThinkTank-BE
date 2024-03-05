using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class MusicPasswordResponse
    {
        [Key]
        public int Id { get; set; }
        public string? Password { get; set; }
        [IntAttribute]
        public int? TopicOfGameId { get; set; }
        public string? SoundLink { get; set; } = null!;

        public string? TopicName { get; set; }
    }
}
