
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Assets.Commands.DeleteAsset
{
    public class DeleteAssetCommand:ICommand<List<AssetResponse>>
    {
        public List<int> Id { get; }
        public DeleteAssetCommand(List<int> id)
        {
            Id = id;
        }
    }
}
