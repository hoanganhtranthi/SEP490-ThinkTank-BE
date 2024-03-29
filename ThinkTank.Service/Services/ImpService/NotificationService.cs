﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ThinkTank.Data.Entities;
using ThinkTank.Data.UnitOfWork;
using ThinkTank.Service.DTO.Request;
using ThinkTank.Service.DTO.Response;
using ThinkTank.Service.Exceptions;
using ThinkTank.Service.Helpers;
using ThinkTank.Service.Services.IService;
using ThinkTank.Service.Utilities;

namespace ThinkTank.Service.Services.ImpService
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotificationResponse> GetNotificationById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Notification Invalid", "");
                }
                var response = _unitOfWork.Repository<Notification>().GetAll().Include(x => x.Account).FirstOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found notification with id {id.ToString()}", "");
                }

                var rs = _mapper.Map<NotificationResponse>(response);
                rs.Username = response.Account.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get Notification By ID Error!!!", ex.InnerException?.Message);
            }
        }

        public async Task<PagedResults<NotificationResponse>> GetNotifications(NotificationRequest request, PagingRequest paging)
        {
            try
            {

                var filter = _mapper.Map<NotificationResponse>(request);
                var notifications = _unitOfWork.Repository<Notification>().GetAll().Include(a => a.Account).Select(x=>new NotificationResponse
                {
                    AccountId=x.AccountId,
                    Title=x.Title,
                    DateTime=x.DateTime,
                    Description = x.Description,
                    Id = x.Id,
                    Avatar=x.Avatar,
                    Status=x.Status,
                    Username=x.Account.UserName
                })     
                    .DynamicFilter(filter).ToList();
                var sort = PageHelper<NotificationResponse>.Sorting(paging.SortType, notifications, paging.ColName);
                var result = PageHelper<NotificationResponse>.Paging(sort, paging.Page, paging.PageSize);
                return result;
            }
            catch (CrudException ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Get notification list error!!!!!", ex.Message);
            }
        }
        public async Task<NotificationResponse> GetToUpdateStatus(int id)
        {
            try
            {
                if (id <= 0)
                {
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Notificationship Invalid", "");
                }
                Notification notification = _unitOfWork.Repository<Notification>().GetAll().Include(x => x.Account).FirstOrDefault(u => u.Id == id);

                if (notification == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found notificationship with id{id.ToString()}", "");
                }
                notification.Status = true;
                await _unitOfWork.Repository<Notification>().Update(notification, id);
                await _unitOfWork.CommitAsync();
                var rs = _mapper.Map<NotificationResponse>(notification);
                rs.Username = notification.Account.UserName;
                return rs;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Update status notification error!!!!!", ex.Message);
            }
        }
        public async Task<List<NotificationResponse>> DeleteNotification(List<int> id)
        {
            try
            {
                var result = new List<NotificationResponse>();
                foreach (var i in id)
                {
                    if (i <= 0)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "Id Notificationship Invalid", "");
                    }
                    Notification notification = _unitOfWork.Repository<Notification>().GetAll().Include(x => x.Account).FirstOrDefault(u => u.Id == i);

                    if (notification == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, $"Not found notificationship with id{id.ToString()}", "");
                    }
                    _unitOfWork.Repository<Notification>().Delete(notification);
                    await _unitOfWork.CommitAsync();
                    var rs = _mapper.Map<NotificationResponse>(notification);
                    rs.Username = notification.Account.UserName;
                    result.Add(rs);
                }
                return result;
            }
            catch (CrudException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete notification error!!!!!", ex.Message);
            }
        }
    }
}
