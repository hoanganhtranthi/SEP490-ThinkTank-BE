﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using ThinkTank.Application.DTO.Request;
using ThinkTank.Application.DTO.Response;
using ThinkTank.Application.GlobalExceptionHandling.Exceptions;
using ThinkTank.Application.Helpers;
using ThinkTank.Application.Services.IService;
using ThinkTank.Application.UnitOfWork;
using ThinkTank.Domain.Entities;

namespace ThinkTank.Application.Services.ImpService
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
                var response = _unitOfWork.Repository<Notification>().GetAll().AsNoTracking().Include(x => x.Account).FirstOrDefault(u => u.Id == id);

                if (response == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found notification with id {id}", "");
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
                var notifications = _unitOfWork.Repository<Notification>().GetAll().AsNoTracking().Include(a => a.Account).Select(x=>new NotificationResponse
                {
                    AccountId=x.AccountId,
                    Title=x.Title,
                    DateNotification=x.DateNotification,
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
                    throw new CrudException(HttpStatusCode.BadRequest, "Id Notification Invalid", "");
                }

                var notification = _unitOfWork.Repository<Notification>().GetAll()
                    .Include(x => x.Account).FirstOrDefault(u => u.Id == id);

                if (notification == null)
                {
                    throw new CrudException(HttpStatusCode.NotFound, $"Not found notification with id{id}", "");
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
        public async Task<List<NotificationResponse>> DeleteNotification(List<int> listId)
        {
            try
            {
                var result = new List<NotificationResponse>();
                foreach (var id in listId)
                {
                    if (id <= 0)
                    {
                        throw new CrudException(HttpStatusCode.BadRequest, "Id Notification Invalid", "");
                    }
                    var notification = _unitOfWork.Repository<Notification>().GetAll()
                        .Include(x => x.Account).FirstOrDefault(u => u.Id == id);

                    if (notification == null)
                    {
                        throw new CrudException(HttpStatusCode.NotFound, $"Not found notification with id{id}", "");
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
                throw new CrudException(HttpStatusCode.InternalServerError, "Delete list notifications error!!!!!", ex.Message);
            }
        }
    }
}