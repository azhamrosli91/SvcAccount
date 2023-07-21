using libMasterLibaryApi.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SvcAccount.Model;
using System.Security.Claims;
using libMasterObject;
using System.Text;
using Newtonsoft.Json;
using libMasterLibaryApi.Models;

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
        public LoginAccountController(ILogger<LoginAccountController> logger, UserManager<User> userManager, 
            IJwtToken jwtToken, IWebApiCalling webApiCalling, IConfiguration configuration, IDbService dbService)
        {
            _logger = logger;
            _userManager = userManager;
            _jwtToken = jwtToken;
            _webApiCalling = webApiCalling;
            _configuration = configuration;
            _dbService = dbService;
        }

        [HttpPost("LoginAcc", Name = "LoginAcc")]
        public async Task<IActionResult> LoginAcc([FromBody] LoginAccount value)
        {
            try
            {

                User user = await _userManager.FindByEmailAsync(value.Email);

                if (user == null)
                    return BadRequest("#F00 Invalid username or password");

                bool isConfirm = await _userManager.IsEmailConfirmedAsync(user);

                if (!isConfirm)
                    return BadRequest("Email not verify, please check out your email.");


                bool result = await _userManager.CheckPasswordAsync(user, value.Password);
                if (result == true)
                {
                    string jsonFormatter = JsonConvert.SerializeObject(user); //JsonConvert.Serialize<User>(user);
                    Claim[] claims = new[]
                    {
                        new Claim(ClaimTypes.Name, user.Email),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Expired, DateTime.Now.AddMinutes(1).ToShortTimeString()),
                    };

                    string token = _jwtToken.GenerateJWTTokenLogin(claims, 20);

                    string userName = value.Email;

                    AuthoAcc authoAcc = new AuthoAcc();
                    authoAcc.Email = value.Email;
                    authoAcc.Token = token;
                    //Get User Data
                    RepoData repoData = new RepoData();
                    repoData.Query = "select * from usr_user_details where email=@email";
                    repoData.Param = new { email = value.Email };
                    USR_User_Details UserDetails = await _dbService.GetAsync<USR_User_Details>(repoData);
                    if (User == null) {
                        return BadRequest("#F01 Invalid username or password");
                    }

                    authoAcc.UserID = UserDetails.UserID;
                    authoAcc.Name = UserDetails.Name;

                    //Get Company Data
                    string url = _configuration["ApiURL:GetCompanyByUsername"];
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