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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IApiURL _apiURL;

        public CreateAccountController(ILogger<CreateAccountController> logger, UserManager<User> userManager, 
            IConfiguration configuration, IHttpClientFactory httpClientFactory, IDbService dbService, 
            IWebApiCalling webApiCalling, RoleManager<IdentityRole>  roleManager, IApiURL apiURL)
        {
            _logger = logger;
            _userManager = userManager;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _dbService = dbService;
            _webApiCalling = webApiCalling;
            _roleManager = roleManager;
            _apiURL = apiURL;
        }
        [HttpPost("CreateUserRoleAcc", Name = "CreateUserRoleAcc")]
        public async Task<IActionResult> CreateUserRoleAcc()
        {
            string[] roles = { "Admin", "CEO", "Manager", "HR", "User" };

            foreach (var roleName in roles)
            {
                // Check if the role already exists
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    // If the role doesn't exist, create it
                    IdentityRole role = new IdentityRole
                    {
                        Name = roleName
                    };

                    // Create the role using RoleManager
                    IdentityResult result = await _roleManager.CreateAsync(role);

                    if (!result.Succeeded)
                    {
                        // Handle the error if the role creation fails
                        return BadRequest("Error creating roles.");
                    }
                }
            }

            //var user1 = await _userManager.FindByNameAsync("azragreen.order@gmail.com");
            //if (user1 != null && !await _userManager.IsInRoleAsync(user1, "Admin"))
            //{
            //    await _userManager.AddToRoleAsync(user1, "Admin");
            //}
            //var user2 = await _userManager.FindByNameAsync("azhamygl@gmail.com");
            //if (user2 != null && !await _userManager.IsInRoleAsync(user2, "User"))
            //{
            //    await _userManager.AddToRoleAsync(user2, "User");
            //}

            return Ok();
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
                //create role as user
                var user1 = await _userManager.FindByNameAsync(HttpUtility.HtmlEncode(value.Email.Trim()));
                if (user1 != null && !await _userManager.IsInRoleAsync(user1, "User"))
                {
                    await _userManager.AddToRoleAsync(user1, "User");
                }

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
                        await _apiURL.GetApiURL("EmailVerify"), requestBody, Encoding.UTF8, "application/json");

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