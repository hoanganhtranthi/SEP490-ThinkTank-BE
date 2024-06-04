

using System.ComponentModel.DataAnnotations;
using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.StartRoomIn1vs1
{
    public class StartRoomIn1vs1Command:ICommand<bool>
    {
        public string RoomIn1vs1Id { get; }
        public bool IsUser1 { get; }
        [Range(0, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Time { get; }
        public int ProgressTime { get; }
        public StartRoomIn1vs1Command(string roomIn1vs1Id, bool isUser1, int time, int progressTime)
        {
            RoomIn1vs1Id = roomIn1vs1Id;
            IsUser1 = isUser1;
            Time = time;
            ProgressTime = progressTime;
        }
    }
}
