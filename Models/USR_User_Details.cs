using System.ComponentModel.DataAnnotations;

namespace SvcAccount.Models
{
    public class USR_User_Details
    {
        [Required]
        public int UserID { get; set; } = 0;
        public string IC_NO { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime DateofBirth { get; set; }
        public string Religion { get; set; }
        public string Race { get; set; }
        public int Gender { get; set; }
        public string Phone1 { get; set; }
        public string Phone1Code { get; set; }
        public string Phone2 { get; set; }
        public string Phone2Code { get; set; }
        public string EM_Name { get; set; }
        public string EM_Phone1 { get; set; }
        public string EM_Phone1Code { get; set; }
        public string EM_Phone2 { get; set; }
        public string EM_Phone2Code { get; set; }
        public string EM_Relatioship { get; set; }
        public string Recovery_Email { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
