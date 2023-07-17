using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SvcAccount.Interface;
using SvcAccount.Model;
using SvcAccount.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VerifyAccountController : ControllerBase
    {
        private readonly ILogger<VerifyAccountController> _logger;
        private readonly UserManager<User> _userManager;

        public VerifyAccountController(ILogger<VerifyAccountController> logger, UserManager<User> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        [HttpPost("VerifyUserAcc", Name = "VerifyUserAcc")]
        public async Task<IActionResult> VerifyUserAcc([FromBody] EmailValidate value)
        {
            try
            {
                if (value.UserEmail == "" || value.UserEmail == null) return NotFound();

                User user = await _userManager.FindByEmailAsync(value.UserEmail);
                if (user == null) return NotFound();

                var token = value.Token; // Encoding.UTF8.GetString(bytes: Convert.FromBase64String(value.Token));

                var result = await _userManager.ConfirmEmailAsync(user, token);


                //  IdentityResult result = await _userManager.GetUserAsy
                if (result.Succeeded)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex);
            }
           
        }

    }
}