using libMasterLibaryApi.Interface;
using libMasterLibaryApi.Models;
using libMasterObject;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SvcAccount.Model;
using System.Text;
using System.Web;

namespace SvcAccount.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AddressAccountController : ControllerBase
    {
        private readonly ILogger<AddressAccountController> _logger;
        private readonly IDbService _dbService;
        private readonly IApiURL _apiURL;
        private readonly IDEAddress _dEAddress;

        public AddressAccountController(ILogger<AddressAccountController> logger, 
            IDbService dbService,IDEAddress dEAddress, IApiURL apiURL)
        {
            _logger = logger;
            _dbService = dbService;
            _apiURL = apiURL;
            _dEAddress = dEAddress;
        }
        [HttpGet("CheckExistDeAddressByUserID", Name = "CheckExistDeAddressByUserID")]
        public async Task<IActionResult> CheckExistDeAddressByUserID(string userid)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userid) == true)
                    return BadRequest();

                if (!int.TryParse(userid, out int result))
                {
                    return BadRequest();
                }
                int count = await _dEAddress.GetCountByUserID_DeAddress(Convert.ToInt32(userid));

                return Ok(JsonConvert.SerializeObject(count));
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
        [HttpPost("AddUserAddress", Name = "AddUserAddress")]
        public async Task<IActionResult> AddUserAddress(DE_Address value)
        {
            value.Address1 = HttpUtility.HtmlEncode(value.Address1);
            value.Address2 = HttpUtility.HtmlEncode(value.Address2);
            value.Address3 = HttpUtility.HtmlEncode(value.Address3);
            value.TypeSetting = 2; //User Address

            RepoData repoData = new RepoData();
            repoData.Query = "select * from usr_user_details where userid=@userid";
            repoData.Param = new { userid = value.UserID };
            USR_User_Details user = await _dbService.GetAsync<USR_User_Details>(repoData);

            if (user == null) return BadRequest("#F00 Failed to insert user address.");

            if (string.IsNullOrWhiteSpace(value.Name)) value.Name = user.Name;
            if (string.IsNullOrWhiteSpace(value.Phone1Code)) value.Phone1Code = user.Phone1Code;
            if (string.IsNullOrWhiteSpace(value.Phone1)) value.Phone1 = user.Phone1;

            int result = await _dEAddress.Insert_DeAddress(value);

            if (result > 0)
            {
                return Ok(result);
            }
            else {
                return BadRequest("#F01 Failed to insert user address.");
            }

        }

    }
}