using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json.Linq;
using Server.Helpers;
using Server.Handlers;

namespace Server.Net
{
    public class ClientWorker
    {
        private readonly TcpClient _client;
        private readonly RequestRouter _router;

        public ClientWorker(TcpClient client, RequestRouter router)
        {
            _client = client;
            _router = router;
        }

        public void Run()
        {
            using (var reader = new StreamReader(_client.GetStream(), Encoding.UTF8))
            using (var writer = new StreamWriter(_client.GetStream(), new UTF8Encoding(false)) { AutoFlush = true })
            {
                try
                {
                    while (true)
                    {
                        string line = reader.ReadLine();
                        if (line == null) break;

                        JObject req;
                        try { req = JObject.Parse(line); }
                        catch { JsonHelper.WriteErr(writer, "BAD_REQUEST", "JSON 파싱 실패"); continue; }

                        string op = req["op"] != null ? req["op"].ToString() : null;
                        string sid = req["sid"] != null ? req["sid"].ToString() : null;
                        var data = req["data"] as JObject ?? new JObject();

                        try
                        {
                            _router.Route(op, sid, data, writer);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            JsonHelper.WriteErr(writer, "UNAUTHORIZED", "로그인이 필요합니다");
                        }
                        catch (Exception ex)
                        {
                            JsonHelper.WriteErr(writer, "INTERNAL_ERROR", ex.Message);
                        }
                    }
                }
                finally
                {
                    _client.Close();
                    Console.WriteLine("클라이언트 연결 종료");
                }
            }
        }
    }
}
