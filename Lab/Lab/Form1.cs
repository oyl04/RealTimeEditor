using System.Text;
using System.Net;
using System.Net.Sockets;


namespace Sockets
{
    public partial class Client : Form
    {
        private int cnt = 0;
        const int MAX_CHARS = 255;
        const int _port = 1026;
        string[] prev = new string[5];
        const string CAPTION_ERR = "Помилка";
        const string ERR_TOO_MANY = "Довжина рядку перевищує допустиму";
        const string ERR_INVALID = "Можна змінювати лише один символ за операцію";
        const string _serverIp = "127.0.0.1";
        const string _logPath = "client_log.txt";

        public Client()
        {
            InitializeComponent();
            File.WriteAllText(_logPath, String.Empty);
        }

        private void Log(string data)
        {
            using (StreamWriter writer = File.AppendText(_logPath))
            {
                writer.WriteLine(data);
            }
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            Submit("connect", out string reply);
            Log("Connected to server at " + DateTime.Now.ToString());

            textBox1.Text = prev[0] = reply.Split("\0")[0];
            textBox2.Text = prev[1] = reply.Split("\0")[1];
            textBox3.Text = prev[2] = reply.Split("\0")[2];
            textBox4.Text = prev[3] = reply.Split("\0")[3];
            textBox5.Text = prev[4] = reply.Split("\0")[4];
        
            textBox1.Visible = true;
            textBox2.Visible = true;
            textBox3.Visible = true;
            textBox4.Visible = true;
            textBox5.Visible = true;
            resultLabel.Visible = true;
            infoBtn.Visible = true;
            infoLabel.Visible = true;
            hideBtn.Visible = true;
            connectBtn.Visible = false;
        }

        private void Submit(string message, out string result)
        {

            if (message.Length >= MAX_CHARS)
            {
                MessageBox.Show(ERR_TOO_MANY, CAPTION_ERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                result = "";
                return;
            }

            try
            {
                IPEndPoint ipPoint = new IPEndPoint(IPAddress.Parse(_serverIp), _port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(ipPoint);

                byte[] data = Encoding.Unicode.GetBytes(message.Length.ToString() + message);
                socket.Send(data);

                result = ReceiveServerReply(socket);

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();

            }
            catch (Exception ex)
            {
                result = "";
            }
        }

        private string ReceiveServerReply(Socket socket)
        {
            byte[] data = new byte[256];
            StringBuilder builder = new StringBuilder();
            int bytes = 0;

            do
            {
                bytes = socket.Receive(data, data.Length, 0);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (socket.Available > 0);

            return builder.ToString();
        }


        private void infoBtn_Click(object sender, EventArgs e)
        {
            Submit("who", out string reply);
            infoLabel.Text = reply;
            Log("Info retrieved: " + reply);
        }
        private void hideBtn_Click(object sender, EventArgs e)
        {
            infoLabel.Text = "";
        }

        private int checkAddChar(string s, string t)
        {
            int pos = s.Length;
            if (t.Length - s.Length != 1) return -1;
            int j = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (j > 1) return -1;
                if (s[i] != t[i + j])
                {
                    pos = i;
                    ++j;
                    --i;
                }
            }
            return pos;
        }
        private int checkReplaceChar(string s, string t)
        {
            int pos = 0;
            if (t.Length - s.Length != 0) return -1;
            int j = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != t[i])
                {
                    ++j;
                    pos = i;
                }
            }
            if (j > 1) return -1;
            else return pos;
        }

        private int checkRemoveChar(string t, string s)
        {
            int pos = s.Length;
            if (t.Length - s.Length != 1) return -1;
            int j = 0;
            for (int i = 0; i < s.Length; i++)
            {
                if (j > 1) return -1;
                if (s[i] != t[i + j])
                {
                    ++j;
                    --i;
                    pos = i + 1;
                }
            }
            return pos;
        }
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            TextBox t = sender as TextBox;
            int ind = t.Name[7] - '0';
            if (t.Text.Length > MAX_CHARS)
            {
                MessageBox.Show(ERR_TOO_MANY, CAPTION_ERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
                t.Text = prev[ind - 1];
                --cnt;
            }
            else if (checkAddChar(prev[ind - 1], t.Text) != -1 || checkRemoveChar(prev[ind - 1], t.Text) != -1 || checkReplaceChar(prev[ind - 1], t.Text) != -1)
            {
                if (prev[ind - 1] != t.Text)
                {
                    char ch = '0'; int index = -1; char symbol = '!';
                    if (checkAddChar(prev[ind - 1], t.Text) != -1) {ch = '+'; index = checkAddChar(prev[ind - 1], t.Text); symbol = t.Text[index]; }
                    if (checkRemoveChar(prev[ind - 1], t.Text) != -1) { ch = '-'; index = checkRemoveChar(prev[ind - 1], t.Text); }
                    if (checkReplaceChar(prev[ind - 1], t.Text) != -1) { ch = '?'; index = checkReplaceChar(prev[ind - 1], t.Text); symbol = t.Text[index]; }
                    string request = ch.ToString() + ind.ToString() + symbol.ToString() + index.ToString();
                    Submit(request, out string reply);
                    Log("Request change sent to server at " + DateTime.Now.ToString() + ": " + request);
                    cnt++;
                    cntReq.Text = "Counter of changes: " + cnt.ToString();
                    prev[ind - 1] = t.Text;
                }
            }
            else
            {
                t.Text = prev[ind - 1];
                MessageBox.Show(ERR_INVALID, CAPTION_ERR, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}