using libMasterLibaryApi.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SvcAccount.Model;
using libMasterObject;
using System.Text;
using System.Web;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InviteFriendController : ControllerBase
    {
        private readonly ILogger<InviteFriendController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IJwtToken _jwtToken;
        private readonly IWebApiCalling _webApiCalling;
        private readonly IConfiguration _configuration;
        private readonly IDbService _dbService;
        private readonly IApiURL _apiURL;
        public InviteFriendController(ILogger<InviteFriendController> logger, UserManager<User> userManager,
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

        [HttpPost("RequestInviteFriend", Name = "RequestInviteFriend")]
        public async Task<IActionResult> RequestInviteFriend([FromBody] EmailInvitationFriend value)
        {
            try
            {
                value.UserEmail = HttpUtility.HtmlEncode(value.UserEmail);


                string url = await _apiURL.GetApiURL("InviteFriend");

                HttpResponseMessage requestResult = await _webApiCalling.RequestApi(HttpMethod.Post,
                       url, value, Encoding.UTF8, "application/json");
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

    }
}