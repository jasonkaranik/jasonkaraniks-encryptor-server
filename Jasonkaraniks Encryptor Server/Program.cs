using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Jasonkaraniks_Encryptor_Server
{
    static class Program
    {
        public static HttpListener listener;

        public static dynamic conf = null;

        public static async Task HandleIncomingConnections()
        {
            while (true)
            {
                try
                {
                    HttpListenerContext ctx = await listener.GetContextAsync();

                    HttpListenerRequest req = ctx.Request;
                    HttpListenerResponse resp = ctx.Response;

                    byte[] data = null;

                    if ((req.HttpMethod == "GET") && (req.Url.AbsolutePath == "/"))
                    {
                        data = Encoding.UTF8.GetBytes("Hi");
                    }
                    else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/encrypt"))
                    {
                        if (!req.HasEntityBody)
                        {
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = false }));
                        }
                        System.IO.Stream body = req.InputStream;
                        System.Text.Encoding encoding = req.ContentEncoding;
                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                        dynamic s = JsonConvert.DeserializeObject(reader.ReadToEnd());
                        body.Close();
                        reader.Close();
                        String d = (String)s.data;
                        String key = (String)s.key;
                        bool useHashing = (bool)s.useHashing;
                        if (s.data != null && s.key != null && s.useHashing != null)
                        {
                            String enc = Encryptor.Encrypt(d, key, useHashing);
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = true, encrypted = enc }));
                        }
                        else
                        {
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = false }));
                        }
                    }
                    else if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/decrypt"))
                    {
                        if (!req.HasEntityBody)
                        {
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = false }));
                        }
                        System.IO.Stream body = req.InputStream;
                        System.Text.Encoding encoding = req.ContentEncoding;
                        System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
                        dynamic s = JsonConvert.DeserializeObject(reader.ReadToEnd());
                        body.Close();
                        reader.Close();
                        String d = (String)s.data;
                        String key = (String)s.key;
                        bool useHashing = (bool)s.useHashing;
                        if (s.data != null && s.key != null && s.useHashing != null)
                        {
                            String enc = Encryptor.Decrypt(d, key, useHashing);
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = true, decrypted = enc }));
                        }
                        else
                        {
                            data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = false }));
                        }
                    }

                    if (data == null) data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new { success = false }));

                    resp.ContentEncoding = Encoding.UTF8;
                    resp.ContentType = "application/json";
                    resp.ContentLength64 = data.LongLength;
                    resp.StatusCode = 200;

                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
                catch { }
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists(Application.StartupPath + "\\config.json")) {
                File.WriteAllText(Application.StartupPath + @"\config.json", JsonConvert.SerializeObject(new { }));
            }

            conf = JsonConvert.DeserializeObject(File.ReadAllText(Application.StartupPath + @"\config.json"));

            if (conf.URL == null) {
                conf.URL = "http://localhost:45576/";
            }

            File.WriteAllText(Application.StartupPath + "\\config.json", JsonConvert.SerializeObject(conf));

            Console.WriteLine("Server running!");

            listener = new HttpListener();
            listener.Prefixes.Add((String)conf.URL);
            listener.Start();

            Task listenTask = HandleIncomingConnections();
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            listener.Close();
        } 
    }
}
