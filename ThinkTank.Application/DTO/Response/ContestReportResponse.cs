

namespace ThinkTank.Application.DTO.Response
{
    public class ContestReportResponse
    {
        public List<BestContestes> BestContestes { get; set; }
        public List<ContestResponse> Contests { get; set;}
       
    }
    public class BestContestes
    {
        public string NameTopContest { get; set; }
        public double PercentAverageScore { get; set; }
    }
}
