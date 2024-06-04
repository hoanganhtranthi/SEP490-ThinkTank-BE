

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.Achieviements.Commands.CreateAchievement
{
    public class CreateAchievementCommand : ICommand<AchievementResponse>
    {
        public CreateAchievementRequest CreateAchievementRequest { get; }
        public CreateAchievementCommand(CreateAchievementRequest createAchievementRequest)
        {
            CreateAchievementRequest = createAchievementRequest;
        }
    }
}
