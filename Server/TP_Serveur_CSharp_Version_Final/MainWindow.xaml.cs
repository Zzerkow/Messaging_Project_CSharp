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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections.Specialized;
using System.Xml;
using System.Configuration;

namespace TP_Serveur_CSharp_Version_Final
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        TcpListener Serveur = null;
        IPAddress localAddr = IPAddress.Any;
        Int32 port;
        Byte[] bytes = new Byte[256];
        String data = null;
        int nbre_client = 0;
        int i = 0;
        int numero_page = 0;
        int nbr_button = 0;

        TcpClient client;
        List<TcpClient> List_Client = new List<TcpClient>();
        List<UI_Tchat> List_Tchat = new List<UI_Tchat>();
        List<Button> List_Button = new List<Button>();
        UI_Tchat tchat;

        Thread THR_Connect;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void BTN_Start_Click(object sender, RoutedEventArgs e)
        {
            THR_Connect = new Thread(Connecter_Clients);
            THR_Connect.Start();
        }

        public void Connecter_Clients()
        {
            //Définition du port et de l'adresse IP
            Serveur = new TcpListener(localAddr, port);

            //Mise en marche de l'écoute pour les clients
            Serveur.Start();


            //Boucle pour écouter les connexions des clients
            while (true)
            {
                this.Dispatcher.Invoke(() => { LB_Console.Items.Add("En attente d'une Connexion..."); });
                //Effectue un appel bloquant pour accepter les demandes

                    client = Serveur.AcceptTcpClient();
                    List_Client.Add(client);                
               
                Creer_BTN_LIST();

                nbre_client = List_Client.Count;
                this.Dispatcher.Invoke(() => { LB_Console.Items.Add("Connecté !"); });
            }
        }

        public void Creer_BTN_LIST()
        {
            this.Dispatcher.Invoke(() => {
                            
                BrushConverter BC = new BrushConverter();
                List<string> List_Color = new List<string>();
                Random RDM = new Random();
                string Color1 = "#7D4FFE";
                string Color2 = "#C49FFF";
                string Color3 = "#FF0080";
                string Color4 = "#00FFC2";
                string Color5 = "#317AC1";
                string Color6 = "#00A0B0";
                string Color7 = "#55D5E0";
                string Color8 = "#0594D0";
                List_Color.Add(Color1);
                List_Color.Add(Color2);
                List_Color.Add(Color3);
                List_Color.Add(Color4);
                List_Color.Add(Color5);
                List_Color.Add(Color6);
                List_Color.Add(Color7);
                List_Color.Add(Color8);

                int i  = RDM.Next(1, 8);

                string Couleur = List_Color[i];

                var button = new Button();                               
                button.Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                button.Background = (Brush)BC.ConvertFrom(Couleur);
                button.FontSize = 15;
                button.FontFamily = new FontFamily("font/CocogooseClassic-Bold.OTF");
                button.Content = "Client " + nbre_client;
                button.Name = "BTN_Client_" + nbre_client + "";
                button.Click += BTN_Tchat_Click;

                //Création du bouton dynamiquement                   
                BTN_Client.Children.Add(button);
               
                //Création du tchat dynamiquement
                Windows_Container.Children.Clear();
                tchat = new UI_Tchat(List_Client[nbre_client], Windows_Container, BTN_Client, button, Couleur);
                Windows_Container.Children.Add(tchat);
                List_Tchat.Add(tchat);
                numero_page = List_Tchat.Count;
            });
        }

        private void BTN_Tchat_Click(object sender, RoutedEventArgs e)
        {           
            Button BTN = (Button)e.Source;

            string Recup = BTN.Name;
            string Recup_2 = Recup.Substring(11);

            nbr_button = Convert.ToInt32(Recup_2);

            Windows_Container.Children.Clear();
            Windows_Container.Children.Add(List_Tchat[nbr_button]);
        }


        private void BTN_Stop_Click(object sender, RoutedEventArgs e)
        {
            Serveur.Stop();
            Environment.Exit(0);
            Windows_Container.Children.Clear();
            BTN_Client.Children.Clear();
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
             try
             {
                 Configuration configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);                             
                 port = Int32.Parse(TB_Port.Text);

                 configFile.AppSettings.Settings.Remove("Port");                
                 configFile.AppSettings.Settings.Add("Port", port.ToString());

                 configFile.Save(ConfigurationSaveMode.Modified);
                 ConfigurationManager.RefreshSection("AppSettings");
             }
             catch (Exception sock_excp)
             {
                 LB_Console.Items.Add("[SAVE IP] Exception : " + sock_excp);
             }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            port = Int32.Parse(ConfigurationManager.AppSettings["Port"].ToString());
            TB_Port.Text = port.ToString();
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = host.AddressList.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            this.Dispatcher.Invoke(() => { LB_IP.Content = ipAddress.ToString(); });           

        }

        private void BTN_Send_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)e.Source;

            string Recup = btn.Name;
            string Recup_2 = Recup.Substring(11);

            nbr_button = Convert.ToInt32(Recup_2);

            Windows_Container.Children.Clear();
            Windows_Container.Children.Add(List_Tchat[nbr_button]);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            tchat.sw.Close();
            Environment.Exit(0);
        }
    }
}
