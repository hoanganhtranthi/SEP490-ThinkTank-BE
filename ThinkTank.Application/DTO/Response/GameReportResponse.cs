

namespace ThinkTank.Application.DTO.Response
{
    public class GameReportResponse
    {
        public double TotalSinglePlayerMode { get; set; }
        public double Total1vs1Mode { get; set; }
        public double TotalMultiplayerMode { get; set; }
        public int TotalContest { get; set; }
        public int TotalRoom { get; set; }
        public int TotalUser { get; set; }
        public int TotalNewbieUser { get; set; }
    }
}
