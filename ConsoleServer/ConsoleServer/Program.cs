using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        static readonly int _port = 1026;
        static readonly string _serverIp = "127.0.0.1";
        static readonly string _logPath = "server_log.txt";

        static void Send(string message, Socket handler)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            handler.Send(data);
        }

        static void Log(string data)
        {
            using (StreamWriter writer = File.AppendText(_logPath))
            {
                writer.WriteLine(data);
            }
        }
        static string[] arrstr = new string[] { "First string\0", "Second string\0", "Third string\0",
            "Fourth string\0", "Fifth string\0"};

        static void ChangeString(string Message)
        {
            int index = Convert.ToInt32(Message.Substring(3));
            int nstring = Message[1] - '0';
            string substr = "";
            if (Message[0] == '?')
            {
                for (int i = 0; i < arrstr[nstring - 1].Length; ++i)
                {
                    if (i == index) substr += Message[2];
                    else substr += arrstr[nstring - 1][i];
                }
            }
            else if (Message[0] == '+')
            {

                for (int i = 0; i < arrstr[nstring - 1].Length; ++i)
                {
                    if (i == index) substr += Message[2];
                    substr += arrstr[nstring - 1][i];
                }
            }
            else if (Message[0] == '-')
            {
                
                for (int i = 0; i < arrstr[nstring - 1].Length; ++i)
                {
                    if (i == index) continue;
                    substr += arrstr[nstring - 1][i];
                }
            }
            arrstr[nstring - 1] = substr;
        }

        static void Main(string[] args)
        {
            File.WriteAllText(_logPath, String.Empty);
            string strings = "";
            foreach (string str in arrstr)
            {
                strings += str;
            }
            IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_serverIp), _port);
            Socket listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listenSocket.Bind(ipPoint);
                listenSocket.Listen(10);

                string serverInitInfo = "Server initialized at " + DateTime.Now.ToString();
                Console.WriteLine(serverInitInfo);
                Log(serverInitInfo);
                while (true)
                {
                    Socket handler = listenSocket.Accept();
                    StringBuilder builder = new StringBuilder();

                    int bytes = 0;
                    byte[] data = new byte[256];

                    do
                    {
                        bytes = handler.Receive(data);
                        builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                    }
                    while (handler.Available > 0);

                    string clientMessage = builder.ToString().Substring(1);
                    string logMessage = "Client message at " + DateTime.Now.ToString() + ": " + clientMessage;


                    Console.WriteLine(logMessage);
                    Log(logMessage);

                    if (clientMessage.Equals("who"))
                    {
                        string info = "Lykhopud Oleksandr, K26, v.1 \"Text Editor\"";
                        Send(info, handler);
                        Log(info);
                    }
                    else if (clientMessage.Equals("connect"))
                    {
                        Send(strings, handler);
                        Log("Connection established successfully at " + DateTime.Now.ToString());
                    }
                    else
                    {
                        ChangeString(clientMessage);
                        if (clientMessage[0] == '+')
                        {
                            Send("Symbol was added", handler);
                            Log("Symbol was added at " + DateTime.Now.ToString());
                        }
                        else if (clientMessage[0] == '-')
                        {
                            Send("Symbol was deleted", handler);
                            Log("Symbol was deleted at " + DateTime.Now.ToString());
                        }
                        else if (clientMessage[0] == '?')
                        {
                            Send("Symbol was changed", handler);
                            Log("Symbol was changed at " + DateTime.Now.ToString());
                        }
                        foreach (string x in arrstr)
                        {
                            Console.WriteLine(x);
                        }
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            }
        }
}
