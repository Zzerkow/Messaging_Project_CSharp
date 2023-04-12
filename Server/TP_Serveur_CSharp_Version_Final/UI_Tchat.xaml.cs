using System;
using System.Collections.Generic;
using System.Linq;
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
//Using indispensable pour tcplistner
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;
//Using pour le fichier config
using System.Configuration;
using System.Collections.Specialized;
using System.Xml;

namespace TP_Serveur_CSharp_Version_Final
{
    /// <summary>
    /// Logique d'interaction pour UI_Tchat.xaml
    /// </summary>
    public partial class UI_Tchat : UserControl
    {
        public TcpClient monclient;
        Stream monstream;
        Thread THR_Recevoir_MSG;
        Thread THR_Envoyer_MSG;
        Thread THR_Fin_Connexion;

        Grid MonContainer;
        StackPanel Mon_LB_BTN;
        Button Mon_BTN;
        string ma_couleur;
        public StreamWriter sw;

        String data = null;
        public string Pseudo = null;

        public UI_Tchat(TcpClient Client, Grid Container, StackPanel LB_BTN, Button BTN, string couleur)
        {

            InitializeComponent();

            monclient = Client;
            MonContainer = Container;
            Mon_LB_BTN = LB_BTN;
            Mon_BTN = BTN;
            ma_couleur = couleur;
            monstream = monclient.GetStream();

            THR_Recevoir_MSG = new Thread(Recevoir_Message);
            THR_Recevoir_MSG.Start();

            THR_Fin_Connexion = new Thread(FermerConnexion);
            THR_Fin_Connexion.Start();
        }

        private void BTN_Send_Click(object sender, RoutedEventArgs e)
        {
            THR_Envoyer_MSG = new Thread(Envoyer_Message);
            THR_Envoyer_MSG.Start();
        }

        public void Recevoir_Message()
        {
            Byte[] bytes = new Byte[256];
            int i;
            try
            {
                int msg = 0;
                while ((i = monstream.Read(bytes, 0, bytes.Length)) != 0)
                {

                    //Transtype les données de byte à UTF8 string.
                    data = System.Text.Encoding.UTF8.GetString(bytes, 0, i);

                    if (msg == 0)
                    {
                        this.Dispatcher.Invoke(() =>
                        {                            
                            Mon_BTN.Content = data.ToString();
                            LB_Pseudo.Content = data.ToString();
                            Pseudo = data;
                        });
                        sw = new StreamWriter("Conversation_" + Pseudo + ".txt");
                        sw.WriteLine("Conversation du " + DateTime.Now.ToString() + "\n");
                    }
                    else
                    {
                        //Affiche le message dans le tchat
                        this.Dispatcher.Invoke(() => {
                            add_new_message(data, false);
                        });
                        sw.WriteLine(DateTime.Now.ToString() + " : " + Pseudo + " : " + data);
                    }
                    msg++;
                }
            }
            catch (Exception Error)
            {
                MessageBox.Show(Error.ToString());
            }
        }

        public void Envoyer_Message()
        {
            string msg_log = null;
           String MSG_send = this.Dispatcher.Invoke(() => TB_Send.Text);
   
            if(MSG_send == "")
            {
                msg_log = "/-/-/No Message/-/-/";
                this.Dispatcher.Invoke(() => { add_new_message(msg_log, true); });
            }
            else
            {
                msg_log = MSG_send;
                byte[] msg = System.Text.Encoding.UTF8.GetBytes(MSG_send);
                monstream.Write(msg, 0, msg.Length);
                this.Dispatcher.Invoke(() => { add_new_message(MSG_send, true); TB_Send.Text = ""; });
            }            
            sw.WriteLine(DateTime.Now.ToString() + " : Serveur : " + msg_log);
        }

        public void add_new_message(string message, bool msg_envoy)
        {
            string color_moi = "#857BFF";
            string color_you = ma_couleur;
            BrushConverter bc = new BrushConverter();

            TextBlock mytextBlock = new TextBlock();
            mytextBlock.TextWrapping = TextWrapping.WrapWithOverflow;

            Border myBorder = new Border();

            myBorder.CornerRadius = new CornerRadius(10);
            myBorder.MaxWidth = 500;
            myBorder.Margin = new Thickness(10);
            myBorder.Padding = new Thickness(10);
            myBorder.Child = mytextBlock;

            ListBoxItem listBoxItem = new ListBoxItem();

            mytextBlock.Text = message;


            if (msg_envoy)
            {
                myBorder.Background = (Brush)bc.ConvertFrom(color_moi);
                listBoxItem.Content = myBorder;
                listBoxItem.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                myBorder.Background = (Brush)bc.ConvertFrom(color_you);
                listBoxItem.Content = myBorder;
                listBoxItem.HorizontalAlignment = HorizontalAlignment.Left;
            }
            LB_Received.Items.Add(listBoxItem);
        }

        private void FermerConnexion()
        {
            while (true)
            {
                if (monclient.Connected == false)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        //N'affiche plus le bouton et le tchat quand on perd la connexion
                        MonContainer.Children.Clear();
                        Mon_LB_BTN.Children.Remove(Mon_BTN);
                        sw.Close();
                    });
                    THR_Fin_Connexion.Abort();
                }
            }

        }

        private void TB_Send_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Return)
            {
                THR_Envoyer_MSG = new Thread(Envoyer_Message);
                THR_Envoyer_MSG.Start();
            }            
        }

        private void TB_Send_GotFocus(object sender, RoutedEventArgs e)
        {
            if(TB_Send.Text =="Ecrire un message...")
            {
                TB_Send.Text = "";
            }

        }

        private void TB_Send_LostFocus(object sender, RoutedEventArgs e)
        {
            TB_Send.Text = "Ecrire un message...";
        }
    }
}
