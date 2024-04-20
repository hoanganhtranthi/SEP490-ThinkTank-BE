using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Service.Commons;

namespace ThinkTank.Service.DTO.Response
{
    public class TopicResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; }
        [IntAttribute]
        public int? GameId { get; set; }
        public virtual ICollection<AssetResponse> Assets { get; set; }
    }
}
