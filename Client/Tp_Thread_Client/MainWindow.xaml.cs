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
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Configuration;
namespace Tp_Thread_Client
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Int32 port;
        string Ip_Serveur;
        string name;
        TcpClient client;
        NetworkStream stream;
        Thread thread_start;
        public MainWindow()
        {

            InitializeComponent();

        }


        public void BTN_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                client = new TcpClient(Ip_Serveur, port);
                stream = client.GetStream();

                thread_start = new Thread(new ThreadStart(Received_Msg));

                thread_start.Start();

                LB_Console.Items.Add("Connected to Serveur : " + Ip_Serveur + ":" + port);

            }
            catch (Exception sock_excp)
            {
                LB_Console.Items.Add("[START] Exception : " + sock_excp);
            }

            string message = TB_Name.Text;

            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                LB_Console.Items.Add("[NAME] Sent : OK !");
            }
            catch (Exception sock_excp)
            {
                LB_Console.Items.Add("[NAME SEND] Exception : " + sock_excp);
            }

        }

        public void BTN_Send_Click(object sender, RoutedEventArgs e)
        {
            string message = TB_Send.Text;



            add_new_message(message, true);


            try
            {
                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);
                

                

                //LB_Received.Items.Add("Moi : " + message);


                LB_Console.Items.Add("Sent : OK !");
            }
            catch (Exception sock_excp)
            {
                LB_Console.Items.Add("[SEND] Exception : " + sock_excp);
            }



        }

        public void Received_Msg()
        {
            try
            {
                while (true)
                {


                    // Receive the TcpServer.response.

                    // Buffer to store the response bytes.
                    Byte[] data = new Byte[256];

                    // String to store the response ASCII representation.
                    String responseData = String.Empty;

                    // Read the first batch of the TcpServer response bytes.

                    Int32 bytes = stream.Read(data, 0, data.Length);
                    responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

                    this.Dispatcher.Invoke(() => //Chaque fois que l'on met à jour des éléments d'interface utilisateur à partir d'un thread autre que le thread principal ceci est necessaire
                    {
                        add_new_message(responseData, false);

                        LB_Console.Items.Add("Received: OK !");
                    });
                }
            }
            catch (Exception sock_excp)
            {
                this.Dispatcher.Invoke(() => //Chaque fois que l'on met à jour des éléments d'interface utilisateur à partir d'un thread autre que le thread principal ceci est necessaire
                {
                    LB_Console.Items.Add("[RECEIVED MSG] Exception : " + sock_excp);
                });
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Ip_Serveur = ConfigurationManager.AppSettings["Ip_Serveur"].ToString();
            port = Int32.Parse(ConfigurationManager.AppSettings["Port"].ToString());
            name = ConfigurationManager.AppSettings["Usr_Name"].ToString();

            TB_Ip_Serveur.Text = Ip_Serveur;
            TB_Port.Text = port.ToString();
            TB_Name.Text = name;
        }

        private void BTN_Save_Ip_Port_Serveur_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


                Ip_Serveur = TB_Ip_Serveur.Text;
                port = Int32.Parse(TB_Port.Text);

                configFile.AppSettings.Settings.Remove("Ip_Serveur");
                configFile.AppSettings.Settings.Remove("Port");
                configFile.AppSettings.Settings.Add("Ip_Serveur", Ip_Serveur);
                configFile.AppSettings.Settings.Add("Port", TB_Port.Text);

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("AppSettings");
            }
            catch (Exception sock_excp)
            {
                LB_Console.Items.Add("[SAVE IP : PORT] Exception : " + sock_excp);
            }




        }

        private void BTN_Save_Name_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                name = TB_Name.Text;

                configFile.AppSettings.Settings.Remove("Usr_Name");
                configFile.AppSettings.Settings.Add("Usr_Name", name);

                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("AppSettings");
            }
            catch (Exception sock_excp)
            {
                LB_Console.Items.Add("[SAVE NAME] Exception : " + sock_excp);
            }

        }




        public void add_new_message(string message, bool msg_envoy)
        {
           
            string color_moi = "#77AFD1";
            string color_you = "#D177AA";
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

        private void Window_Closed(object sender, EventArgs e)
        {           
            Environment.Exit(0);     
        }

        private void BTN_Stop_Click(object sender, RoutedEventArgs e)
        {
            string message = "/#12a35ze4as21a";

            Byte[] data = System.Text.Encoding.UTF8.GetBytes(message);
            // Send the message to the connected TcpServer.
            stream.Write(data, 0, data.Length);

            thread_start.Abort();
        }
    }
}








/*
 
this.Dispatcher.Invoke(() => //Chaque fois que l'on met à jour des éléments d'interface utilisateur à partir d'un thread autre que le thread principal ceci est necessaire
            {
                LB_Received.Items.Add("Serveur : " + responseData);
                LB_Console.Items.Add("Received: OK !");
            }); 


 */