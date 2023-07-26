using libMasterLibaryApi.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SvcAccount.Model;
using System.Security.Claims;
using libMasterObject;
using System.Text;
using Newtonsoft.Json;
using libMasterLibaryApi.Models;
using Newtonsoft.Json.Linq;
using System.Web;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginAccountController : ControllerBase
    {
        private readonly ILogger<LoginAccountController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IJwtToken _jwtToken;
        private readonly IWebApiCalling _webApiCalling;
        private readonly IConfiguration _configuration;
        private readonly IDbService _dbService;
        private readonly IApiURL _apiURL;
        public LoginAccountController(ILogger<LoginAccountController> logger, UserManager<User> userManager, 
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

        [HttpPost("LoginAcc", Name = "LoginAcc")]
        public async Task<IActionResult> LoginAcc([FromBody] LoginAccount value)
        {
            try
            {

                User user = await _userManager.FindByEmailAsync(value.Email);

                if (user == null)
                    return BadRequest("#F00 Invalid username or password");

                //bool isConfirm = await _userManager.IsEmailConfirmedAsync(user);

                //if (!isConfirm) {
                //    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                //    var requestBody = new EmailValidate
                //    {
                //        UserID = returnID,
                //        Name = HttpUtility.HtmlEncode(value.Name),
                //        UserEmail = HttpUtility.HtmlEncode(value.Email.Trim()),
                //        Token = token,
                //        DateTimeExp = DateTime.Now.AddDays(2),
                //    };

                //    HttpResponseMessage requestResult = await _webApiCalling.RequestApi(HttpMethod.Post,
                //        await _apiURL.GetApiURL("EmailVerify"), requestBody, Encoding.UTF8, "application/json");

                //    return BadRequest("Email not verify, please check out your email.");

                //}

                bool result = await _userManager.CheckPasswordAsync(user, value.Password);
                if (result == true)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    string roleName = roles.FirstOrDefault();

                    if (string.IsNullOrEmpty(roleName)) {
                        roleName = "User";
                    }
                    string jsonFormatter = JsonConvert.SerializeObject(user); //JsonConvert.Serialize<User>(user);
                    Claim[] claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, roleName),
                        new Claim(ClaimTypes.Expired, DateTime.Now.AddMinutes(1).ToShortTimeString()),
                    };

                    string token = _jwtToken.GenerateJWTTokenLogin(claims,30);

                    string userName = value.Email;

                    AuthoAcc authoAcc = new AuthoAcc();
                    authoAcc.Email = value.Email;
                    authoAcc.Token = token;
                    //Get User Data
                    RepoData repoData = new RepoData();
                    repoData.Query = "select * from usr_user_details where email=@email";
                    repoData.Param = new { email = value.Email };
                    USR_User_Details UserDetails = await _dbService.GetAsync<USR_User_Details>(repoData);
                    if (UserDetails == null) {
                        return BadRequest("#F01 Invalid username or password");
                    }

                    bool isConfirm = await _userManager.IsEmailConfirmedAsync(user);

                    if (!isConfirm)
                    {
                        var tokenEmail = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                        var requestBody = new EmailValidate
                        {
                            UserID = UserDetails.UserID,
                            Name = HttpUtility.HtmlEncode(UserDetails.Name),
                            UserEmail = HttpUtility.HtmlEncode(value.Email.Trim()),
                            Token = tokenEmail,
                            DateTimeExp = DateTime.Now.AddDays(2),
                        };

                        HttpResponseMessage requestEmailVerify = await _webApiCalling.RequestApi(HttpMethod.Post,
                            await _apiURL.GetApiURL("EmailVerify"), requestBody, Encoding.UTF8, "application/json");

                        return BadRequest("Email not verify, please check out your email.");

                    }

                    authoAcc.UserID = UserDetails.UserID;
                    authoAcc.Name = UserDetails.Name;

                    
                    //Get Company Data
                    string url = await _apiURL.GetApiURL("GetCompanyByUsername");
                    HttpResponseMessage requestResult = await _webApiCalling.RequestApi(HttpMethod.Post,
                                    url, userName, Encoding.UTF8, "application/json");


                    if (requestResult.IsSuccessStatusCode) {
                        string requestData = await requestResult.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(requestData)) {
                            COM_Company Company = JsonConvert.DeserializeObject<COM_Company>(requestData);
                            if (Company != null) {
                                authoAcc.CompanyID = Company.CompanyID;
                                authoAcc.CompanyName = Company.Name;
                            }
                        }
                    }

                    return Ok(authoAcc);
                }
                else { 
                
                    return BadRequest("#F02 Invalid username or password");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("#F03 Invalid username or password.");
            }
           
        }

    }
}