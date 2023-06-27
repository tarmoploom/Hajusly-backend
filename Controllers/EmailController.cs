using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using Hajusly.Model;


namespace Hajusly.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase {
    private readonly AppDbContext _context;

    public EmailController(AppDbContext context) {
        _context = context;
    }


    [HttpPost]
    public IActionResult testEmail([FromBody] Email? message) {
        var smtpClient = new SmtpClient("localhost")

        //var smtpClient = new SmtpClient("172.31.38.139")
        {
            Port = 2525,
            //Credentials = new NetworkCredential("username", "H$ntwTy]<kLOMMYALgWw"),
            EnableSsl = false,
        };

        //smtpClient.SendAsync("Hajusly App app@hajusly.info", message.recipient, message.subject, message.message);
        MailAddress from = new MailAddress("app@hajusly.info", "Hajusly App");
        MailAddress to = new MailAddress(message?.recipient!);

        MailMessage email = new MailMessage(from, to);

        email.Subject = message?.subject;
        email.Body = message?.message;
        string state = "aa";
        smtpClient.SendAsync(email, state);
        return Ok(state);
    }
}