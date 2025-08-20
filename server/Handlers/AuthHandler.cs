using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Server.Helpers;
using Server.Models;
using Server.State;

namespace Server.Handlers
{
    public class AuthHandler
    {
        public void Register(JObject d, StreamWriter w)
        {
            string name = d.Value<string>("name") ?? "";
            string uid = d.Value<string>("userId") ?? "";
            string pw = d.Value<string>("password") ?? "";

            if (name == "" || uid == "" || pw == "")
            { JsonHelper.WriteErr(w, "BAD_REQUEST", "필수값 누락"); return; }

            lock (ServerState.UsersLock)
            {
                if (ServerState.Users.ContainsKey(uid))
                { JsonHelper.WriteErr(w, "DUPLICATE_USER", "이미 존재하는 아이디"); return; }

                ServerState.Users[uid] = new User
                {
                    UserId = uid,
                    Name = name,
                    PasswordHash = HashHelper.Sha256(pw)
                };
            }
            JsonHelper.WriteOk(w, new JObject());
        }

        public void Login(JObject d, StreamWriter w)
        {
            string uid = d.Value<string>("userId") ?? "";
            string pw = d.Value<string>("password") ?? "";

            User user;
            lock (ServerState.UsersLock)
            {
                if (!ServerState.Users.TryGetValue(uid, out user) ||
                    user.PasswordHash != HashHelper.Sha256(pw))
                {
                    JsonHelper.WriteErr(w, "INVALID_CREDENTIALS", "아이디/비밀번호가 틀렸습니다.");
                    return;
                }
            }

            string sid = Guid.NewGuid().ToString("N");
            ServerState.Sessions[sid] = uid;

            var data = new JObject { ["sid"] = sid, ["name"] = user.Name };
            JsonHelper.WriteOk(w, data);
        }
    }
}
