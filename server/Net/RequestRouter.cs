using System.IO;
using Newtonsoft.Json.Linq;
using Server.Helpers;

namespace Server.Handlers
{
    public class RequestRouter
    {
        private readonly AuthHandler _auth;
        private readonly AccountHandler _account;

        public RequestRouter(AuthHandler auth, AccountHandler account)
        {
            _auth = auth;
            _account = account;
        }

        public void Route(string op, string sid, JObject data, StreamWriter w)
        {
            switch (op)
            {
                case "register": _auth.Register(data, w); break;
                case "login": _auth.Login(data, w); break;
                case "create": _account.Create(sid, data, w); break;
                case "balance": _account.Balance(sid, data, w); break;
                case "deposit": _account.Deposit(sid, data, w); break;
                case "withdraw": _account.Withdraw(sid, data, w); break;
                default: JsonHelper.WriteErr(w, "BAD_REQUEST", "지원하지 않는 op"); break;
            }
        }
    }
}
