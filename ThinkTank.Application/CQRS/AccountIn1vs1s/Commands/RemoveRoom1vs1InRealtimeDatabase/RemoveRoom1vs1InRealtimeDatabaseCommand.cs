

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.RemoveRoom1vs1InRealtimeDatabase
{
    public class RemoveRoom1vs1InRealtimeDatabaseCommand:ICommand<bool>
    {
        public string Room1vs1Id { get; }
        public int DelayTime { get; }
        public RemoveRoom1vs1InRealtimeDatabaseCommand(string room1vs1Id, int delayTime)
        {
            Room1vs1Id = room1vs1Id;
            DelayTime = delayTime;
        }
    }
}
