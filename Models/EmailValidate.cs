namespace SvcAccount.Models
{
    public class EmailValidate
    {
        public int UserID { get; set; }
        public string Name { get; set; }
        public string UserEmail { get; set; }
        public string Token { get; set; }
        public DateTime DateTimeExp { get; set; }
    }
}
