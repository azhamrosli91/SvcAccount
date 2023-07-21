using libMasterLibaryApi.Interface;
using libMasterObject;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SvcAccount.Model;
using System.Text;
using System.Web;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateAccountController : ControllerBase
    {
        private readonly ILogger<CreateAccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IDbService _dbService;
        private readonly IWebApiCalling _webApiCalling;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CreateAccountController(ILogger<CreateAccountController> logger, UserManager<User> userManager, 
            IConfiguration configuration, IHttpClientFactory httpClientFactory, IDbService dbService, IWebApiCalling webApiCalling)
        {
            _logger = logger;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _dbService = dbService;
            _webApiCalling = webApiCalling;
        }

        [HttpPost("CreateUserAcc", Name = "CreateUserAcc")]
        public async Task<IActionResult> CreateUserAcc([FromBody] USR_User_CreateAcc value)
        {
            
            var user = new User
            {
                UserName = HttpUtility.HtmlEncode(value.Email.Trim()),
                Email = HttpUtility.HtmlEncode(value.Email.Trim()),
                PhoneNumber = value.Phone1Code.Trim() + value.Phone1.Trim()
            };

            var result = await _userManager.CreateAsync(user, value.Password);
            if (result.Succeeded)
            {
                string Query = @"insert into usr_user_details (name, email, " +
                "phone1,phone1code) VALUES (@name, @email, " +
                "@phone1,@phone1code) RETURNING userid";

                int returnID = await _dbService.ExecuteBuildAsync(Query, value);
                if (returnID > 0)
                {
                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                    //Email the verifyAcc
                    var requestBody = new EmailValidate
                    {
                        UserID = returnID,
                        Name = HttpUtility.HtmlEncode(value.Name),
                        UserEmail = HttpUtility.HtmlEncode(value.Email.Trim()),
                        Token = token,
                        DateTimeExp = DateTime.Now.AddDays(2),
                    };

                    HttpResponseMessage requestResult = await _webApiCalling.RequestApi(HttpMethod.Post, 
                        _configuration["ApiURL:EmailVerify"], requestBody, Encoding.UTF8, "application/json");

                    if (requestResult.IsSuccessStatusCode)
                    {
                        //var responseBody = await response.Content.ReadAsStringAsync();
                        //var responseModel = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(responseBody);
                        return Ok(returnID);
                    }
                    else {
                        return BadRequest("Ops!! Failed to send email verification");
                    }

                }
                else
                {
                    return BadRequest("Ops!! Failed to save information user");
                }
            }
            else {
                foreach (IdentityError error in result.Errors)
                {
                    return BadRequest(error.Description);
                   
                }
                return BadRequest(result.Errors);
            }
        }

    }
}