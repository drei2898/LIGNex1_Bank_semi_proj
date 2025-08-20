using System.Collections.Generic;
using Server.Models;

namespace Server.State
{
    public static class ServerState
    {
        public static readonly Dictionary<string, User> Users = new Dictionary<string, User>();       // userId -> User
        public static readonly Dictionary<string, string> Sessions = new Dictionary<string, string>(); // sid -> userId
        public static readonly object UsersLock = new object();
    }
}
