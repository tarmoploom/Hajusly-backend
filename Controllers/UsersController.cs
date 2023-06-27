using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hajusly.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Hajusly.Controllers;


[Route("api/[controller]")]
[ApiController]
public class UsersController : Controller {
    private readonly AppDbContext _context;
    private IConfiguration _config;

    public UsersController(IConfiguration config, AppDbContext context) {
        _config = config;
        _context = context;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] User login) {
        var dbUser = _context.Users!.FirstOrDefault(user => user.Username == login.Username);

        if (dbUser == null) return NotFound();

        if (dbUser.Password != HashPassword(login.Password)) return Unauthorized();

        var token = GenerateJSONWebToken(dbUser);

        return Ok(new { token = token });

    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] User user) {
        var dbUser = _context.Users!.FirstOrDefault(usr => usr.Username == user.Username);

        if (dbUser != null) return Conflict($"user {user.Username} already registered!");
        user.isActive = true;
        user.Password = HashPassword(user.Password);

        _context.Add(user);
        _context.SaveChanges();

        return CreatedAtAction(nameof(getSelf), new { Id = user.Id }, user);
    }

    [Authorize]
    [HttpGet]
    [Route("/api/[controller]/getSelf")]
    public IActionResult getSelf() {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        var userId = int.Parse(identity!.FindFirst("UserId")!.Value);


        var person = _context.Users!.Where(x => x.Id == userId);
        if (person == null) {
            return NotFound();
        }
        return Ok(person);
    }


    private string HashPassword(string password) {
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt: new byte[0],
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: 100000,
            numBytesRequested: 256 / 8));

        return hashed;
    }

    private string GenerateJSONWebToken(User user) {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(_config["Jwt:Issuer"],
            _config["Jwt:Issuer"],
            new List<Claim> { new Claim("UserId", user.Id.ToString()) },
            expires: DateTime.Now.AddMinutes(500),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);

    }

}
