using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebDiary.Entities;
using RestSharp;

public interface IEmailSenderService
{
    public Task<RestResponse> SendEmail(string to, string subject, string body);
}
