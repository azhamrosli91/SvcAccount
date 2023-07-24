using libMasterLibaryApi.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SvcAccount.Model;
using System.Security.Claims;
using libMasterObject;
using System.Text;
using Newtonsoft.Json;
using libMasterLibaryApi.Models;
using System.Web;
using Newtonsoft.Json.Linq;
using SvcAccount.Interface;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly ILogger<ResetPasswordController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IJwtToken _jwtToken;
        private readonly IWebApiCalling _webApiCalling;
        private readonly IConfiguration _configuration;
        private readonly IDbService _dbService;
        private readonly IApiURL _apiURL;
        public ResetPasswordController(ILogger<ResetPasswordController> logger, UserManager<User> userManager,
            IJwtToken jwtToken, IWebApiCalling webApiCalling, IConfiguration configuration, IDbService dbService, IApiURL apiURL)
        {
            _logger = logger;
            _userManager = userManager;
            _jwtToken = jwtToken;
            _webApiCalling = webApiCalling;
            _configuration = configuration;
            _dbService = dbService;
            _apiURL = apiURL;
        }

        [HttpPost("RequestResetPassword", Name = "RequestResetPassword")]
        public async Task<IActionResult> RequestResetPassword([FromBody] string email)
        {
            try
            {
                email = HttpUtility.HtmlEncode(email);

                var user = await _userManager.FindByNameAsync(email);

                if (user == null) return BadRequest("#F00 User not found.");

                RepoData repoData = new RepoData()
                {
                    Query = @"select * from usr_user_details where email=@email",
                    Param = new {
                        email = email
                    }
                };
                USR_User_Details userDetails = await _dbService.GetAsync<USR_User_Details>(repoData);

                if (userDetails == null) return BadRequest("#F01 User not found.");

                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

                var requestBody = new EmailValidate
                {
                    UserID = userDetails.UserID,
                    Name = userDetails.Name,
                    UserEmail = HttpUtility.HtmlEncode(user.Email),
                    Token = resetToken,
                    DateTimeExp = DateTime.Now.AddDays(1),
                };
                string url = await _apiURL.GetApiURL("EmailResetPassword");

                HttpResponseMessage requestResult = await _webApiCalling.RequestApi(HttpMethod.Post,
                       url, requestBody, Encoding.UTF8, "application/json");
                // Reset the user's password
                //var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                if (requestResult.IsSuccessStatusCode)
                {
                    return Ok(requestResult);
                }
                else
                {
                    return BadRequest(requestResult);
                }
            }
            catch (Exception ex)
            {
                return BadRequest("#F03 Failed to request reset password.");
            }

        }
        [HttpPost("ResetPassword", Name = "ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword value) {
            try
            {
                if (string.IsNullOrEmpty(value.Email)) return BadRequest("Invalid email address");

                if (string.IsNullOrEmpty(value.Password) || value.Password == null || value.Password.Length < 6) return BadRequest("Invalid password");

                if (string.IsNullOrEmpty(value.Token)) return BadRequest("Invalid token");

                var user = await _userManager.FindByNameAsync(value.Email);

                if (user == null) return BadRequest("#F00 User not found.");

                var result = await _userManager.ResetPasswordAsync(user, value.Token, value.Password);

                if (result.Succeeded)
                {
                    return Ok();
                }
                else {
                    return BadRequest("#F02 Failed to reset password.");
                }

            }
            catch (Exception)
            {
                return BadRequest("#F03 Failed to reset password.");
            }
        }

    }
}