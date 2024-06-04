using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using ThinkTank.Application.CQRS.Notifications.Commands.DeleteNotification;
using ThinkTank.Application.CQRS.Notifications.Commands.UpdateStatusNotification;
using ThinkTank.Application.CQRS.Notifications.Queries.GetNotifications;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : Controller
    {
       private readonly IMediator _mediator;
        public NotificationsController(IMediator mediator)
        {
            _mediator=mediator;
        }
        /// <summary>
        /// Get list of notification
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="notificationRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet]
        [ProducesResponseType(typeof(PagedResults<NotificationResponse>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetNotifications([FromQuery] PagingRequest pagingRequest, [FromQuery] NotificationRequest notificationRequest)
        {
            var rs = await _mediator.Send(new GetNotificationsQuery(pagingRequest,notificationRequest));
            return Ok(rs);
        }
        /// <summary>
        /// Update status notification
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{notificationId:int}/status")]
        [ProducesResponseType(typeof(IconResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetToUpdateStatus(int notificationId)
        {
            var rs = await _mediator.Send(new UpdateStatusNotificationCommand(notificationId));
            return Ok(rs);
        }
        /// <summary>
        /// Delete list of notification
        /// </summary>
        /// <param name="notificationId"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpDelete()]
        [ProducesResponseType(typeof(NotificationResponse), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> DeleteNotification([FromBody] List<int> notificationId)
        {
            var rs = await _mediator.Send(new DeleteNotificationCommand(notificationId));
            return Ok(rs);
        }
    }
}
