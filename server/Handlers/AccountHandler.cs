using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Server.Helpers;
using Server.Models;
using Server.State;

namespace Server.Handlers
{
    public class AccountHandler
    {
        // 공통 보조
        private User RequireUser(string sid)
        {
            if (string.IsNullOrEmpty(sid) || !ServerState.Sessions.TryGetValue(sid, out var uid))
                throw new UnauthorizedAccessException();

            lock (ServerState.UsersLock)
            {
                if (!ServerState.Users.TryGetValue(uid, out var u))
                    throw new UnauthorizedAccessException();
                return u;
            }
        }

        private Account GetAccount(User user, JObject d)
        {
            string type = d.Value<string>("type") ?? "";
            string apw = d.Value<string>("accountPw") ?? "";

            if (!user.Accounts.TryGetValue(type, out var acc))
                throw new Exception("ACCOUNT_NOT_FOUND");
            if (acc.AccountPwHash != HashHelper.Sha256(apw))
                throw new Exception("ACCOUNT_PASSWORD_MISMATCH");
            return acc;
        }

        // op별 처리
        public void Create(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            string type = d.Value<string>("type") ?? "";            // DEMAND | ISA
            string apw = d.Value<string>("accountPw") ?? "";
            decimal initial = d.Value<decimal?>("initial") ?? 0;

            if (type == "" || apw == "")
            { JsonHelper.WriteErr(w, "BAD_REQUEST", "필수값 누락"); return; }

            lock (ServerState.UsersLock)
            {
                if (user.Accounts.ContainsKey(type))
                { JsonHelper.WriteErr(w, "ALREADY_HAS_ACCOUNT", "이미 해당 형식 계좌 보유"); return; }

                var acc = new Account
                {
                    AccountId = "A-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                    Type = type,
                    AccountPwHash = HashHelper.Sha256(apw),
                    Balance = initial
                };
                user.Accounts[type] = acc;

                var data = new JObject { ["accountId"] = acc.AccountId, ["balance"] = acc.Balance };
                JsonHelper.WriteOk(w, data);
            }
        }

        public void Balance(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            var data = new JObject { ["name"] = user.Name, ["type"] = acc.Type, ["balance"] = acc.Balance };
            JsonHelper.WriteOk(w, data);
        }

        public void Deposit(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            decimal amount = d.Value<decimal?>("amount") ?? 0;
            if (amount <= 0) { JsonHelper.WriteErr(w, "BAD_REQUEST", "금액 오류"); return; }

            lock (acc.LockObj)
            {
                acc.Balance += amount;
                JsonHelper.WriteOk(w, new JObject { ["balance"] = acc.Balance });
            }
        }

        public void Withdraw(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            decimal amount = d.Value<decimal?>("amount") ?? 0;
            if (amount <= 0) { JsonHelper.WriteErr(w, "BAD_REQUEST", "금액 오류"); return; }

            lock (acc.LockObj)
            {
                if (acc.Balance < amount)
                { JsonHelper.WriteErr(w, "INSUFFICIENT_FUNDS", "잔액 부족"); return; }
                acc.Balance -= amount;
                JsonHelper.WriteOk(w, new JObject { ["balance"] = acc.Balance });
            }
        }
    }
}
