using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class PrizeOfContestResponse
    {
        [Key]
        public int Id { get; set; }
        public int FromRank { get; set; }
        public int ToRank { get; set; }
        public int Prize { get; set; }
        [IntAttribute]
        public int ContestId { get; set; }
    }
}
