﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Services.ImpService;
using ThinkTank.Service.Services.IService;

namespace ThinkTank.API.Controllers
{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : Controller
    {
       private readonly INotificationService _notificationService;
        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        /// <summary>
        /// Get list of notification
        /// </summary>
        /// <param name="pagingRequest"></param>
        /// <param name="notificationRequest"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet]
        public async Task<ActionResult<List<NotificationResponse>>> GetNotifications([FromQuery] PagingRequest pagingRequest, [FromQuery] NotificationRequest notificationRequest)
        {
            var rs = await _notificationService.GetNotifications(notificationRequest, pagingRequest);
            return Ok(rs);
        }
        /// <summary>
        /// Get notification by Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Policy = "Player")]
        [HttpGet("{id:int}")]
        public async Task<ActionResult<NotificationResponse>> GetNotification(int id)
        {
            var rs = await _notificationService.GetNotificationById(id);
            return Ok(rs);
        }
    }
}