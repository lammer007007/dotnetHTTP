using System;
using System.IO;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace dotnetHTTP
{
    class Program
    {
        static Message[] msgs = new Message[5];
        static int count = 0;
        static void Main(string[] args)
        {
            HttpListener server = new HttpListener();
            server.Prefixes.Add("http://localhost:9999/");
            server.Prefixes.Add("http://127.0.0.1:9999/");
            server.Start();
            Console.WriteLine("Сервер запущен");
            HttpListenerContext ctx;
            while(true)
            {
                ctx = server.GetContext();
                Task.Run(() => HttpHandle(ctx.Request, ctx.Response));
            }
        }

        static void HttpHandle(HttpListenerRequest req, HttpListenerResponse res)
        {
            switch(req.RawUrl)
            {
                case "/":
                    BinaryReader reader = new BinaryReader(new FileStream("index.html", 
                    FileMode.Open), Encoding.UTF8);
                    byte[] htmlBuffer = new byte[reader.BaseStream.Length];
                    reader.Read(htmlBuffer, 0, (int)reader.BaseStream.Length);
                    reader.Close();
                    res.ContentType = "text/html";
                    res.AddHeader("Charset", "UTF-8");
                    res.OutputStream.Write(htmlBuffer);
                    res.Close();
                    break;
                case "/messages":
                    if(req.HttpMethod == "POST")
                    {
                        byte[] reqBuffer = new byte[req.ContentLength64];
                        req.InputStream.Read(reqBuffer, 0, reqBuffer.Length);
                        Message msg = JsonConvert.DeserializeObject<Message>
                        (Encoding.UTF8.GetString(reqBuffer));
                        for (int i = msgs.Length - 1; i > 0 ; i--)
                            msgs[i] = msgs[i - 1];
                        msgs[0] = msg;
                        if(count < 5)
                            count++;
                        res.OutputStream.Write(Encoding.UTF8.
                        GetBytes(JsonConvert.SerializeObject(msg)));
                        res.Close();
                        break;
                    }
                    Message[] temp = new Message[count];
                    Array.Copy(msgs, temp, count);
                    res.ContentType = "application/json";
                    res.AddHeader("Charset", "UTF-8");
                    res.OutputStream.Write(Encoding.UTF8.
                    GetBytes(JsonConvert.SerializeObject(temp)));
                    res.Close();
                    break;
                default:
                    res.StatusCode = 404;
                    res.Close();
                    break;
            }
        }

        class Message
        {
            public string message = "";
        }
    }
}
