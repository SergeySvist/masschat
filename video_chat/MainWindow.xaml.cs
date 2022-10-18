using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace video_chat
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string myip;
        string mynick;
        string myid;
        string clientip;
        Client c;
        public MainWindow()
        {
            InitializeComponent();

            string hostName = Dns.GetHostName();
            myip = Dns.GetHostByName(hostName).AddressList[1].ToString();
            btn_EndCall.IsEnabled = false;
        }

        private void GetUserList(string message)
        {
            string strres = message.Substring(message.IndexOf("users=") + "users=".Length);
            string[] arrres = strres.Split(";");
            Dispatcher.Invoke(() =>
            {
                list_Users.Items.Clear();
                foreach (string str in arrres)
                {
                    string[] res = str.Split(",");
                    if (res.Length > 1)
                    {
                        if ($"{res[0]},{res[1]}" == $"{myip},{mynick}")
                        {
                            myid = res[2];
                        }
                        else
                        {
                            TextBlock txt = new TextBlock();
                            txt.Text = res[1];
                            txt.Tag = $"{res[0]},{res[2]}";
                            list_Users.Items.Add(txt);
                        }
                    }
                }
            });
        }

        private void ConnectTo(string message)
        {
            string strres = message.Substring(message.IndexOf("connectto=") + "connectto=".Length);
            string[] arrres = strres.Split(",");
            Dispatcher.Invoke(() =>
            {
                var res = MessageBox.Show($"You want to start a conversation with {arrres[1]}", "Confirmation", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Yes)
                {
                    c.SendMessage($"dmto={arrres[2]}|success");
                    txt_UserInfo.Text = arrres[1];
                    txt_UserInfo.Tag = $"{arrres[0]},{arrres[2]}";
                    txt_Chat.Text = "";
                    btn_Connect.IsEnabled = false;
                    btn_EndCall.IsEnabled = true;
                    c.SendMessage("unlogin");
                }
            });
        }

        private void SuccessConnection()
        {
            Dispatcher.Invoke(() =>
            {
                TextBlock txt = list_Users.SelectedItem as TextBlock;
                txt_UserInfo.Text = txt.Text;
                txt_UserInfo.Tag = txt.Tag;
                txt_Chat.Text = "";
                btn_Connect.IsEnabled = false;
                btn_EndCall.IsEnabled = true;
                c.SendMessage("unlogin");
            });
        }

        private void EndCall()
        {
            Dispatcher.Invoke(() =>
            {
                txt_UserInfo.Text = "Chat";
                txt_UserInfo.Tag = "";
                txt_Chat.Text = "";
                btn_Connect.IsEnabled = true;
                btn_EndCall.IsEnabled = false;
                c.SendMessage("login");
            });
        }

        private void btn_Login_Click(object sender, RoutedEventArgs e)
        {
            if (txt_Nickname.Text != "")
            {
                c = new Client(new ConnectionSettings("127.0.0.1", 8080, txt_Nickname.Text));
                c.Connect();
                mynick = txt_Nickname.Text;
                Title = mynick;
                Task.Run(() =>
                {
                    while (true)
                    {
                        string message = "";

                        message = c.ReceiveMessage();
                        string strres = "";
                        string[] arrres;

                        if (message.IndexOf("users=") != -1)
                        {
                            GetUserList(message);
                        }
                        else if (message.IndexOf("connectto=") != -1)
                        {
                            ConnectTo(message);
                        }
                        else if (message == "success")
                        {
                            SuccessConnection();
                        }
                        else if (message == "endcall")
                        {
                            EndCall();
                        }
                        else if (message != "unlogin" && message != "exit" && message != "endcall")
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (txt_UserInfo.Text == "Chat")
                                {
                                    txt_Chat.Text += $"{message}\n";
                                }
                                else if(message.IndexOf("=>") == -1)
                                {
                                    txt_Chat.Text += $"{txt_UserInfo.Text} => {message}\n";
                                }
                            });
                        }
                    }
                });

                tabs_Main.SelectedIndex = 1;
            }
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            TextBlock txt = list_Users.SelectedItem as TextBlock;
            string[] res = txt.Tag.ToString().Split(",");

            c.SendMessage($"dmto={res[1]}|connectto={myip},{mynick},{myid}");
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if(txt_Message.Text != "")
            {
                if (txt_UserInfo.Text == "Chat")
                {
                    c.SendMessage($"mass={txt_Message.Text}");
                }
                else
                {
                    string[] res = txt_UserInfo.Tag.ToString().Split(",");
                    c.SendMessage($"dmto={res[1]}|{txt_Message.Text}");
                    txt_Chat.Text += $"{mynick} => {txt_Message.Text}\n";
                }
                txt_Message.Text = "";
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            c.SendMessage("exit");
        }

        
        private void btn_EndCall_Click(object sender, RoutedEventArgs e)
        {
            string[] res = txt_UserInfo.Tag.ToString().Split(",");
            c.SendMessage($"dmto={res[1]}|endcall");
            EndCall();
        }
    }
}
