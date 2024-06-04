

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Assets.Commands.CreateAsset
{
    public class CreateAssetCommand:ICommand<List<AssetResponse>>
    {
        public List<CreateAssetRequest> AssetRequests { get; }
        public CreateAssetCommand(List<CreateAssetRequest> assetRequests)
        {
            AssetRequests = assetRequests;
        }
    }
}
