using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class GameLevelResponse
    {
        public int Level { get; set; }
        public int AmoutPlayer { get; set; }
        public String GameMode { get; set; }
        [JsonIgnore]
        public List<int> AccountIdsChecked { get; set; }
    }
}
