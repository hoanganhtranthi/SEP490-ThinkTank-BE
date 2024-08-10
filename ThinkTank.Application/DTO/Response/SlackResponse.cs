

namespace ThinkTank.Application.DTO.Response
{
    public class SlackResponse
    {
        public bool ok { get; set; }
        public string error { get; set; }
        public string channel { get; set; }
        public string ts { get; set; }
    }
}
