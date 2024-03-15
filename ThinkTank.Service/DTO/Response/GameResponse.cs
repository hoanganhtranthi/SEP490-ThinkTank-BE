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
    public class GameResponse
    {
        [Key]
        public int Id { get; set; }
        [StringAttribute]
        public string? Name { get; set; }
        public int? AmoutPlayer { get; set; }
        public virtual ICollection<TopicResponse> Topics { get; set; }
    }
}