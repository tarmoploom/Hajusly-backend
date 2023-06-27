using System.Net.Mail;
using Hajusly.Model;

namespace Hajusly;

public class EmailService
{
    private SmtpClient _smtpClient = new SmtpClient("172.31.38.139")
    {
        Port = 2525,
        EnableSsl = false,
    };
    private MailAddress _emailFrom = new MailAddress("app@hajusly.info", "Hajusly App");

    public async Task sendEmail(string recipient, string subject, string messageBody)
    {
        
        MailAddress to = new MailAddress(recipient);
        MailMessage email = new MailMessage(_emailFrom, to);
        email.Subject = subject;
        email.Body = messageBody;
        string state = "ok";
        _smtpClient.SendAsync(email, state);
        
    }

    public async Task testEmail(Email? message)
    {
        
        //smtpClient.SendAsync("Hajusly App app@hajusly.info", message.recipient, message.subject, message.message);
        MailAddress to = new MailAddress(message.recipient);
        MailMessage email = new MailMessage(_emailFrom, to);
        email.Subject = message.subject;
        email.Body = message.message;
        string state = "aa";
        _smtpClient.SendAsync(email, state);
        
    }

    
}