using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class IconOfAccountResponse
    {
        [Key]
        public int Id { get; set; }
        public bool? IsAvailable { get; set; }
        [IntAttribute]
        public int? AccountId { get; set; }
        [IntAttribute]
        public int? IconId { get; set; }
        public string? UserName { get; set; }
        public string? IconAvatar { get; set; }
        public string? IconName { get; set; }
    }
}
