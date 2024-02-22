using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class AnonymityOfContestResponse
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; } = null!;
        public int Characteristic { get; set; }
        public string LinkImg { get; set; } = null!;
        [IntAttribute]
        public int ContestId { get; set; }
    }
}
