using libMasterLibaryApi.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using SvcAccount.Model;
using SvcAccount.Models;
using System.Security.Claims;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginAccountController : ControllerBase
    {
        private readonly ILogger<LoginAccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IJwtToken _jwtToken;

        public LoginAccountController(ILogger<LoginAccountController> logger, UserManager<User> userManager, IJwtToken jwtToken)
        {
            _logger = logger;
            _userManager = userManager;
            _jwtToken = jwtToken;
        }

        [HttpPost("LoginAcc", Name = "LoginAcc")]
        public async Task<IActionResult> LoginAcc([FromBody] LoginAccount value)
        {
            try
            {

                User user = await _userManager.FindByEmailAsync(value.Email);

                if (user == null)
                    return BadRequest("Invalid username or password");

                bool isConfirm = await _userManager.IsEmailConfirmedAsync(user);

                if (!isConfirm)
                    return BadRequest("Email not verify, please check out your email.");


                bool result = await _userManager.CheckPasswordAsync(user, value.Password);
                if (result == true)
                {
                    string jsonFormatter = JsonSerializer.Serialize<User>(user);
                    Claim[] claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Expired, DateTime.Now.AddMinutes(1).ToShortTimeString()),
                    };

                    string token = _jwtToken.GenerateJWTTokenLogin(claims, 1);

                    AuthoAcc authoAcc = new AuthoAcc
                    {
                        Email = value.Email,
                        Token = token
                    };
                    return Ok(authoAcc);
                }
                else { 
                
                    return BadRequest("Invalid username or password");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Invalid username or password.");
            }
           
        }

    }
}