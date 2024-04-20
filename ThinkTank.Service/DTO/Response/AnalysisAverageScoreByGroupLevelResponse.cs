using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThinkTank.Service.DTO.Response
{
    public class AnalysisAverageScoreByGroupLevelResponse
    {
        public string GroupLevel { get; set; }
        public double AverageOfGroup { get; set; }
        public double AverageOfPlayer { get; set; }
    }
}
