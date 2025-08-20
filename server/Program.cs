using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    internal class Program
    {
        static TcpListener listener;
        static readonly Dictionary<string, User> Users = new Dictionary<string, User>();     // userId -> User
        static readonly Dictionary<string, string> Sessions = new Dictionary<string, string>(); // sid -> userId
        static readonly object UsersLock = new object();

        static void Main(string[] args)
        {
            try
            {
                int port = 3333;
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine("서버 시작 : 포트 " + port);

                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine("클라이언트 연결됨");
                    var th = new Thread(HandleClient) { IsBackground = true };
                    th.Start(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("에러: " + ex.Message);
            }
        }

        static void HandleClient(object obj)
        {
            var client = (TcpClient)obj;
            using (var reader = new StreamReader(client.GetStream(), Encoding.UTF8))
            using (var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true })
            {
                try
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;

                        JObject req;
                        try { req = JObject.Parse(line); }
                        catch { WriteErr(writer, "BAD_REQUEST", "JSON 파싱 실패"); continue; }

                        string op = req["op"] != null ? req["op"].ToString() : null;
                        string sid = req["sid"] != null ? req["sid"].ToString() : null;
                        var data = req["data"] as JObject ?? new JObject();

                        try
                        {
                            switch (op)
                            {
                                case "register": DoRegister(data, writer); break;
                                case "login": DoLogin(data, writer); break;
                                case "create": DoCreate(sid, data, writer); break;
                                case "balance": DoBalance(sid, data, writer); break;
                                case "deposit": DoDeposit(sid, data, writer); break;
                                case "withdraw": DoWithdraw(sid, data, writer); break;
                                default: WriteErr(writer, "BAD_REQUEST", "지원하지 않는 op"); break;
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            WriteErr(writer, "UNAUTHORIZED", "로그인이 필요합니다");
                        }
                        catch (Exception ex)
                        {
                            WriteErr(writer, "INTERNAL_ERROR", ex.Message);
                        }
                    }
                }
                finally
                {
                    client.Close();
                    Console.WriteLine("클라이언트 연결 종료");
                }
            }
        }

        // --------- Handlers ---------

        static void DoRegister(JObject d, StreamWriter w)
        {
            string name = d.Value<string>("name") ?? "";
            string uid = d.Value<string>("userId") ?? "";
            string pw = d.Value<string>("password") ?? "";

            if (name == "" || uid == "" || pw == "") { WriteErr(w, "BAD_REQUEST", "필수값 누락"); return; }

            lock (UsersLock)
            {
                if (Users.ContainsKey(uid)) { WriteErr(w, "DUPLICATE_USER", "이미 존재하는 아이디"); return; }
                Users[uid] = new User
                {
                    UserId = uid,
                    Name = name,
                    PasswordHash = Hash(pw)
                };
            }
            WriteOk(w, new JObject()); // "계정 생성 완료"
        }

        static void DoLogin(JObject d, StreamWriter w)
        {
            string uid = d.Value<string>("userId") ?? "";
            string pw = d.Value<string>("password") ?? "";

            User user;
            lock (UsersLock)
            {
                if (!Users.TryGetValue(uid, out user) || user.PasswordHash != Hash(pw))
                { WriteErr(w, "INVALID_CREDENTIALS", "아이디/비밀번호가 틀렸습니다."); return; }
            }
            string sid = Guid.NewGuid().ToString("N");
            Sessions[sid] = uid;
            var data = new JObject { ["sid"] = sid, ["name"] = user.Name };
            WriteOk(w, data);
        }

        static void DoCreate(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            string type = d.Value<string>("type") ?? "";            // DEMAND | ISA
            string apw = d.Value<string>("accountPw") ?? "";
            decimal initial = d.Value<decimal?>("initial") ?? 0;

            if (type == "" || apw == "") { WriteErr(w, "BAD_REQUEST", "필수값 누락"); return; }

            lock (UsersLock)
            {
                if (user.Accounts.ContainsKey(type))
                { WriteErr(w, "ALREADY_HAS_ACCOUNT", "이미 해당 형식 계좌 보유"); return; }

                var acc = new Account
                {
                    AccountId = "A-" + DateTime.Now.ToString("yyyyMMdd") + "-" + Guid.NewGuid().ToString("N").Substring(0, 6),
                    Type = type,
                    AccountPwHash = Hash(apw),
                    Balance = initial
                };
                user.Accounts[type] = acc;
                var data = new JObject { ["accountId"] = acc.AccountId, ["balance"] = acc.Balance };
                WriteOk(w, data);
            }
        }

        static void DoBalance(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            var data = new JObject { ["name"] = user.Name, ["type"] = acc.Type, ["balance"] = acc.Balance };
            WriteOk(w, data);
        }

        static void DoDeposit(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            decimal amount = d.Value<decimal?>("amount") ?? 0;
            if (amount <= 0) { WriteErr(w, "BAD_REQUEST", "금액 오류"); return; }

            lock (acc.LockObj)
            {
                acc.Balance += amount;
                WriteOk(w, new JObject { ["balance"] = acc.Balance });
            }
        }

        static void DoWithdraw(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            decimal amount = d.Value<decimal?>("amount") ?? 0;
            if (amount <= 0) { WriteErr(w, "BAD_REQUEST", "금액 오류"); return; }

            lock (acc.LockObj)
            {
                if (acc.Balance < amount) { WriteErr(w, "INSUFFICIENT_FUNDS", "잔액 부족"); return; }
                acc.Balance -= amount;
                WriteOk(w, new JObject { ["balance"] = acc.Balance });
            }
        }

        // --------- Helpers ---------

        static User RequireUser(string sid)
        {
            if (string.IsNullOrEmpty(sid) || !Sessions.TryGetValue(sid, out var uid))
                throw new UnauthorizedAccessException();
            lock (UsersLock)
            {
                if (!Users.TryGetValue(uid, out var u)) throw new UnauthorizedAccessException();
                return u;
            }
        }

        static Account GetAccount(User user, JObject d)
        {
            string type = d.Value<string>("type") ?? "";
            string apw = d.Value<string>("accountPw") ?? "";
            if (!user.Accounts.TryGetValue(type, out var acc))
                throw new Exception("ACCOUNT_NOT_FOUND");
            if (acc.AccountPwHash != Hash(apw))
                throw new Exception("ACCOUNT_PASSWORD_MISMATCH");
            return acc;
        }

        static void WriteOk(StreamWriter w, JObject data)
        {
            var obj = new JObject { ["ok"] = true, ["data"] = data ?? new JObject() };
            w.WriteLine(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        static void WriteErr(StreamWriter w, string code, string msg)
        {
            var obj = new JObject { ["ok"] = false, ["err"] = msg, ["code"] = code };
            w.WriteLine(obj.ToString(Newtonsoft.Json.Formatting.None));
        }

        static string Hash(string s)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
                return Convert.ToBase64String(bytes);
            }
        }
    }

    class User
    {
        public string UserId;
        public string Name;
        public string PasswordHash;
        public Dictionary<string, Account> Accounts = new Dictionary<string, Account>(); // key: DEMAND|ISA
    }

    class Account
    {
        public string AccountId;
        public string Type;              // DEMAND | ISA
        public string AccountPwHash;
        public decimal Balance;
        public object LockObj = new object();
    }
}

