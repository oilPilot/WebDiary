using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebDiary.Entities;

public interface IEmailSenderService
{
    public Task SendEmail(string to, string subject, string body);
}
