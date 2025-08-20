using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Server
{
    internal class Program
    {
        static TcpListener listener;
        static readonly Dictionary<string, User> Users = new();   // userId -> User
        static readonly Dictionary<string, string> Sessions = new(); // sid -> userId
        static readonly object UsersLock = new();

        static void Main(string[] args)
        {
            try
            {
                int port = 3333;
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                Console.WriteLine($"���� ���� : ��Ʈ {port}");

                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine("Ŭ���̾�Ʈ �����");
                    var th = new Thread(HandleClient) { IsBackground = true };
                    th.Start(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("����: " + ex.Message);
            }
        }

        static void HandleClient(object obj)
        {
            var client = (TcpClient)obj;
            using var reader = new StreamReader(client.GetStream(), Encoding.UTF8);
            using var writer = new StreamWriter(client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true };

            try
            {
                while (true)
                {
                    string line = reader.ReadLine();
                    if (line == null) break;

                    JObject req;
                    try { req = JObject.Parse(line); }
                    catch { WriteErr(writer, "BAD_REQUEST", "JSON �Ľ� ����"); continue; }

                    string op = req["op"]?.ToString();
                    string sid = req["sid"]?.ToString();
                    var data = (JObject?)req["data"] ?? new JObject();

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
                            default: WriteErr(writer, "BAD_REQUEST", "�������� �ʴ� op"); break;
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        WriteErr(writer, "UNAUTHORIZED", "�α����� �ʿ��մϴ�");
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
                Console.WriteLine("Ŭ���̾�Ʈ ���� ����");
            }
        }


        static void DoRegister(JObject d, StreamWriter w)
        {
            string name = d.Value<string>("name") ?? "";
            string uid = d.Value<string>("userId") ?? "";
            string pw = d.Value<string>("password") ?? "";

            if (name=="" || uid=="" || pw=="") { WriteErr(w, "BAD_REQUEST", "�ʼ��� ����"); return; }

            lock (UsersLock)
            {
                if (Users.ContainsKey(uid)) { WriteErr(w, "DUPLICATE_USER", "�̹� �����ϴ� ���̵�"); return; }
                Users[uid] = new User
                {
                    UserId = uid,
                    Name = name,
                    PasswordHash = Hash(pw)
                };
            }
            WriteOk(w, new JObject()); // "���� ���� �Ϸ�"
        }

        static void DoLogin(JObject d, StreamWriter w)
        {
            string uid = d.Value<string>("userId") ?? "";
            string pw = d.Value<string>("password") ?? "";

            User user;
            lock (UsersLock)
            {
                if (!Users.TryGetValue(uid, out user) || user.PasswordHash != Hash(pw))
                { WriteErr(w, "INVALID_CREDENTIALS", "���̵�/��й�ȣ�� Ʋ�Ƚ��ϴ�."); return; }
            }
            string sid = Guid.NewGuid().ToString("N");
            Sessions[sid] = uid;
            WriteOk(w, new JObject { ["sid"] = sid, ["name"] = user.Name });
        }

        static void DoCreate(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            string type = d.Value<string>("type") ?? "";            // DEMAND | ISA
            string apw = d.Value<string>("accountPw") ?? "";
            decimal initial = d.Value<decimal?>("initial") ?? 0;

            if (type=="" || apw=="") { WriteErr(w, "BAD_REQUEST", "�ʼ��� ����"); return; }

            lock (UsersLock)
            {
                if (user.Accounts.ContainsKey(type))
                { WriteErr(w, "ALREADY_HAS_ACCOUNT", "�̹� �ش� ���� ���� ����"); return; }

                var acc = new Account
                {
                    AccountId = $"A-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6]}",
                    Type = type,
                    AccountPwHash = Hash(apw),
                    Balance = initial
                };
                user.Accounts[type] = acc;
                WriteOk(w, new JObject { ["accountId"] = acc.AccountId, ["balance"] = acc.Balance });
            }
        }

        static void DoBalance(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            WriteOk(w, new JObject { ["name"]=user.Name, ["type"]=acc.Type, ["balance"]=acc.Balance });
        }

        static void DoDeposit(string sid, JObject d, StreamWriter w)
        {
            var user = RequireUser(sid);
            var acc = GetAccount(user, d);
            decimal amount = d.Value<decimal?>("amount") ?? 0;
            if (amount <= 0) { WriteErr(w, "BAD_REQUEST", "�ݾ� ����"); return; }

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
            if (amount <= 0) { WriteErr(w, "BAD_REQUEST", "�ݾ� ����"); return; }

            lock (acc.LockObj)
            {
                if (acc.Balance < amount) { WriteErr(w, "INSUFFICIENT_FUNDS", "�ܾ� ����"); return; }
                acc.Balance -= amount;
                WriteOk(w, new JObject { ["balance"] = acc.Balance });
            }
        }


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

        static void WriteOk(StreamWriter w, JObject data) =>
            w.WriteLine(new JObject { ["ok"]=true, ["data"]=data ?? new JObject() }.ToString(Formatting.None));

        static void WriteErr(StreamWriter w, string code, string msg) =>
            w.WriteLine(new JObject { ["ok"]=false, ["err"]=msg, ["code"]=code }.ToString(Formatting.None));

        static string Hash(string s) =>
            Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(s)));
    }

    class User
    {
        public string UserId;
        public string Name;
        public string PasswordHash;
        public Dictionary<string, Account> Accounts = new(); // key: DEMAND|ISA
    }

    class Account
    {
        public string AccountId;
        public string Type;              // DEMAND | ISA
        public string AccountPwHash;
        public decimal Balance;
        public object LockObj = new();
    }
}
