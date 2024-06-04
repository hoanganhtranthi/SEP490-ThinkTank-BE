﻿

using System.Windows.Input;
using ThinkTank.Application.Configuration.Commands;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;

namespace ThinkTank.Application.CQRS.AccountIn1vs1s.Commands.CreateAccountIn1vs1
{
    public class CreateAccountIn1vs1Command:ICommand<AccountIn1vs1Response>
    {
        public CreateAndUpdateAccountIn1vs1Request CreateAndUpdateAccountIn1Vs1Request { get; }
        public CreateAccountIn1vs1Command(CreateAndUpdateAccountIn1vs1Request createAndUpdateAccountIn1Vs1Request)
        {
            CreateAndUpdateAccountIn1Vs1Request = createAndUpdateAccountIn1Vs1Request;
        }
    }
}
