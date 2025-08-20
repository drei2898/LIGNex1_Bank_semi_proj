namespace Server.Models
{
    public class Account
    {
        public string AccountId;
        public string Type;              // DEMAND | ISA
        public string AccountPwHash;
        public decimal Balance;
        public object LockObj = new object();
    }
}
