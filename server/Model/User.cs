namespace Server.Models
{
    public class User
    {
        public string UserId;
        public string Name;
        public string PasswordHash;
        public Dictionary<string, Account> Accounts = new Dictionary<string, Account>(); // key: DEMAND|ISA
    }
}
