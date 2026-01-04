using WebDiary.Data;
using WebDiary.Entities;
using WebDiary.Helpers;
using RestSharp;
using RestSharp.Authenticators;

public class EmailSenderService : IEmailSenderService
{
    private readonly IConfiguration config;

    public EmailSenderService(IConfiguration config)
    {
        this.config = config;
    }

    public async Task<RestResponse> SendEmail(string to, string subject, string body)
    {
        var options = new RestClientOptions("https://api.mailgun.net") {
            Authenticator = new HttpBasicAuthenticator("api", config["MailgunApiKey"] ?? throw new ArgumentNullException("MailgunApiKey is not configured"))
        };

        RestClient client = new RestClient(options);
        RestRequest request = new RestRequest("/v3/sandbox8ee94f10576c4ee6b2bd056edb3f1107.mailgun.org/messages", Method.Post);
        request.AlwaysMultipartFormData = true;

        request.AddParameter("from", "WebDiary <postmaster@sandbox8ee94f10576c4ee6b2bd056edb3f1107.mailgun.org>");
        request.AddParameter("to", to);
        request.AddParameter("subject", subject);
        request.AddParameter("text", body);

        return await client.ExecuteAsync(request);
    }
}
