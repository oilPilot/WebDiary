using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.Helpers;

public class EmailSenderService : IEmailSenderService
{
    private readonly DiariesContext dbContext;
    private readonly IConfiguration config;

    public EmailSenderService(DiariesContext dbContext, IConfiguration config)
    {
        this.dbContext = dbContext;
        this.config = config;
    }

    public Task SendEmail(string to, string subject, string body)
    {
        throw new NotImplementedException();
    }
}
