
namespace ThinkTank.Service.DTO.Response
{
    public class TypeOfAssetInContestResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = null!;

        public virtual ICollection<AssetOfContestResponse> AssetOfContests { get; set; }
    }
}
