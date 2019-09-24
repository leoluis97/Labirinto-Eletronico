using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Un4seen.Bass;

namespace LaribintoEletronico
{
    /// <summary>
    /// Interaction logic for LabirintoEletronico.xaml
    /// </summary>
    public partial class LabirintoEletronico : Window
    { 
        // Importações nativas.
        [DllImport("Drix_Joystick.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DRIX_InitializeDI();

        [DllImport("Drix_Joystick.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DRIX_GetConnectedJoystickIDs();

        [DllImport("Drix_Joystick.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr DRIX_GetJoystickName(string ID);

        [DllImport("Drix_Joystick.dll", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool DRIX_CreateJoystickDevice(string ID);

        [DllImport("Drix_Joystick.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DRIX_ReadJoystickState(string ID, int element);

        [DllImport("Drix_Joystick.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int DRIX_ReleaseJoystick(string ID);
        

        private Sentido sentido = Sentido.AB;
        private DispatcherTimer dTmr = null;
        private uint toques = 0;                
        private int streamBeep = 0;

        public LabirintoEletronico()
        {
            InitializeComponent();
            
            txtbSentidoDescricao.Visibility = Visibility.Collapsed;
            txtbSentido.Visibility = Visibility.Collapsed;
            recImagemDisp.Visibility = Visibility.Collapsed;
            txtbTempo.Visibility = Visibility.Collapsed;
            txtbToques.Visibility = Visibility.Collapsed;
            txtbPosicao.Visibility = Visibility.Collapsed;

            dTmr = new DispatcherTimer();
            dTmr.Interval = TimeSpan.FromMilliseconds(10);
            dTmr.Tick += dTmr_Tick;

            if (Bass.BASS_Init(-1, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero))
                streamBeep = Bass.BASS_StreamCreateFile("Beep.wav", 0, 0, BASSFlag.BASS_DEFAULT);
            else
                MessageBox.Show(Bass.BASS_ErrorGetCode().ToString());
            
            IniciaJoystick();

            LeArquivoRecords();
        }

        #region Métodos

        private void LeArquivoRecords()
        {
            if(!File.Exists("records"))
                File.Create("records");
            
            using (StreamReader sr = new StreamReader("records", Encoding.Default))
            {
                spRecordesEsq.Children.Clear();
                spRecordesDir.Children.Clear();

                for (int i = 0; i < 2; i++)
                {
                    string linha = sr.ReadLine();

                    if (linha != null)
                    {
                        string[] array = linha.Split(';');

                  
                    }
                }
            }
        }

        DateTime captTempoPS = DateTime.MinValue;
        
        private void PrepararSentido()
        {
            if (DateTime.Now - captTempoPS < TimeSpan.FromMilliseconds(200))
            {
                captTempoPS = DateTime.Now;
                return;
            }

            captTempoPS = DateTime.Now;

            int s = Bass.BASS_StreamCreateFile("botao.wav", 0, 0, BASSFlag.BASS_STREAM_AUTOFREE);
            Bass.BASS_ChannelPlay(s, false);

            Dispatcher.Invoke(new Action(() =>
                {  
                    dTmr.IsEnabled = false;
                    txtbTempo.Visibility = Visibility.Collapsed;
                    txtbToques.Visibility = Visibility.Collapsed;
                    txtbPosicao.Visibility = Visibility.Collapsed;
                    txtbMsgInicial.Visibility = Visibility.Visible;
                    txtbSentidoDescricao.Visibility = Visibility.Visible;
                    txtbSentido.Visibility = Visibility.Visible;
                    recImagemDisp.Visibility = Visibility.Visible;

                    gridCen.Background = new SolidColorBrush(Colors.Transparent);
                    txtbTempo.Foreground = new SolidColorBrush(Colors.Gray);
                    txtbToques.Foreground = txtbTempo.Foreground;
                    txtbSentido.Foreground = txtbTempo.Foreground;
                    txtbSentidoDescricao.Foreground = txtbTempo.Foreground;
                    recImagemDisp.Fill = txtbTempo.Foreground;

                    if (sentido == Sentido.AB)
                    {
                        txtbSentido.RenderTransform = new ScaleTransform(1, 1);
                        txtbMsgInicial.Text = "Toque com a argola em A para iniciar";
                    }
                    else
                    {
                        txtbSentido.RenderTransform = new ScaleTransform(-1, 1);
                        txtbMsgInicial.Text = "Toque com a argola em B para iniciar";
                    }
                }));
        }

        private void IniciaJoystick()
        {
            if (!DRIX_InitializeDI())
            {
                MessageBox.Show("Falha ao iniciar o DirectInput.", "Labirinto Eletrônico", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Environment.Exit(0);
                return;
            }

            Thread thrMOnitoraJoystick = new Thread(MonitoraJoystick);
            thrMOnitoraJoystick.IsBackground = true;
            thrMOnitoraJoystick.Start();
        }

        private DateTime captTime = DateTime.MinValue;
        private string JoyID = null;
       
        private void MonitoraJoystick()
        {
            while (true)
            {
                if (DateTime.Now - captTime > TimeSpan.FromSeconds(3))
                {
                    // Obtemos os IDs dos joysticks conectados (DRIX_GetConnectedJoystickIDs retorna uma string contendo os IDs separados pelo caracter espaço).
                    string strIDs = Marshal.PtrToStringAnsi(DRIX_GetConnectedJoystickIDs());

                    if (string.IsNullOrEmpty(strIDs))
                    {
                        // TODO: Exibir mensagem alertando que não há joystick conectado ao computador.
                        Dispatcher.Invoke(new Action(() =>
                        {
                            dTmr.IsEnabled = false;
                            txtbStatus.Foreground = new SolidColorBrush(Colors.Red);
                            txtbStatus.Text = "Joystick não detectado";
                            txtbSentidoDescricao.Visibility = Visibility.Collapsed;
                            txtbSentido.Visibility = Visibility.Collapsed;
                            recImagemDisp.Visibility = Visibility.Collapsed;
                            txtbTempo.Visibility = Visibility.Collapsed;
                            txtbToques.Visibility = Visibility.Collapsed;
                            txtbPosicao.Visibility = Visibility.Collapsed;
                            txtbMsgInicial.Visibility = Visibility.Collapsed;                                                        
                            gridCen.Background = new SolidColorBrush(Colors.Transparent);                            
                        }));
                        if (JoyID != null) DRIX_ReleaseJoystick(JoyID);
                        JoyID = null;                        
                        continue;
                    }

                    string[] IDs = strIDs.Split(' ');

                    if (IDs.Length > 1)
                    {
                        // TODO: Exibir mensagem alertando que há mais de 1 joystick conectado ao computador.                    
                        Dispatcher.Invoke(new Action(() =>
                        {
                            dTmr.IsEnabled = false;
                            txtbStatus.Foreground = new SolidColorBrush(Colors.Red);
                            txtbStatus.Text = "Conflito: Múltiplos joysticks detectados";
                            txtbSentidoDescricao.Visibility = Visibility.Collapsed;
                            txtbSentido.Visibility = Visibility.Collapsed;
                            recImagemDisp.Visibility = Visibility.Collapsed;
                            txtbTempo.Visibility = Visibility.Collapsed;
                            txtbToques.Visibility = Visibility.Collapsed;
                            txtbPosicao.Visibility = Visibility.Collapsed;
                            txtbMsgInicial.Visibility = Visibility.Collapsed;
                            gridCen.Background = new SolidColorBrush(Colors.Transparent);
                        }));
                        if (JoyID != null) DRIX_ReleaseJoystick(JoyID);
                        JoyID = null;
                        continue;
                    }

                    Dispatcher.Invoke(new Action(() =>
                    {
                        txtbStatus.Foreground = new SolidColorBrush(Colors.Green);
                        txtbStatus.Text = "Joystick detectado";

                        if (txtbTempo.Visibility == Visibility.Collapsed)
                        {
                            txtbMsgInicial.Visibility = Visibility.Visible;
                            //txtbMsgInicial.Text = "Pressione um dos botões para iniciar";
                        }
                    }));

                    if (JoyID == null)
                    {
                        if (DRIX_CreateJoystickDevice(IDs[0]))
                        {

                        }

                        JoyID = IDs[0];
                    }
                    else
                    {
                        if (IDs[0] != JoyID)
                        {
                            DRIX_ReleaseJoystick(JoyID);

                            if (DRIX_CreateJoystickDevice(IDs[0]))
                            {

                            }

                            JoyID = IDs[0];
                        }
                    }

                    captTime = DateTime.Now;
                }

                if (JoyID != null)
                {
                    bool B1 = DRIX_ReadJoystickState(JoyID, 1) > 0 ? true : false;
                    bool B2 = DRIX_ReadJoystickState(JoyID, 2) > 0 ? true : false;
                    bool B3 = DRIX_ReadJoystickState(JoyID, 3) > 0 ? true : false;
                    bool B4 = DRIX_ReadJoystickState(JoyID, 4) > 0 ? true : false;
                    int h = DRIX_ReadJoystickState(JoyID, 101);
                    bool dEsq = h == -1000 ? true : false;

                    if (B2)
                    {
                        sentido = Sentido.AB;
                        PrepararSentido();
                    }
                    else if (B4)
                    {
                        sentido = Sentido.BA;
                        PrepararSentido();
                    }
                    else if (dEsq)
                    {
                        if (sentido == Sentido.AB)
                            IniciarJogo();
                        else
                            FinalizarJogo();
                    }
                    else if (B1)
                    {
                        if (sentido == Sentido.BA)
                            IniciarJogo();
                        else
                            FinalizarJogo();
                    }
                    else if (B3)
                    {
                        Toque();
                    }
                    else
                    {                      
                        Dispatcher.Invoke(new Action(() =>
                        {
                            gridCen.Background = new SolidColorBrush(Colors.Transparent);
                            txtbTempo.Foreground = new SolidColorBrush(Colors.Gray);
                            txtbToques.Foreground = txtbTempo.Foreground;
                            txtbSentido.Foreground = txtbTempo.Foreground;
                            txtbSentidoDescricao.Foreground = txtbTempo.Foreground;
                            recImagemDisp.Fill = txtbTempo.Foreground;
                        }));
                    }
                }

                Thread.Sleep(20);
            }
        }

        private void IniciarJogo()
        {
            if (txtbSentido.Visibility == Visibility.Collapsed)
                return;

            if (!dTmr.IsEnabled)
            {
                int s = Bass.BASS_StreamCreateFile("ja.wav", 0, 0, BASSFlag.BASS_STREAM_AUTOFREE);
                Bass.BASS_ChannelPlay(s, false);
                toques = 0;
                dTmr.Tag = DateTime.Now;
                Dispatcher.Invoke(new Action(() =>
                {
                    txtbMsgInicial.Visibility = Visibility.Collapsed;
                    txtbPosicao.Visibility = Visibility.Collapsed;
                    txtbTempo.Visibility = Visibility.Visible;
                    txtbToques.Visibility = Visibility.Visible;                    
                    txtbToques.Text = "nenhum toque";
                }));
                dTmr.IsEnabled = true;
            }
        }

        private void FinalizarJogo()
        {
            if (!dTmr.IsEnabled) return;

            dTmr.IsEnabled = false;

            string tempo = null;
            Dispatcher.Invoke(new Action(() => tempo = txtbTempo.Text));

            if (toques == 0)
            {
                int s = Bass.BASS_StreamCreateFile("aplausos.wav", 0, 0, BASSFlag.BASS_STREAM_AUTOFREE);
                Bass.BASS_ChannelPlay(s, false);

                Dispatcher.Invoke(new Action(() => txtbToques.Text = "Parabéns! Percurso perfeito!"));
            }
            else
            {
                int s = Bass.BASS_StreamCreateFile("fim.wav", 0, 0, BASSFlag.BASS_STREAM_AUTOFREE);
                Bass.BASS_ChannelPlay(s, false);
            }

            string[,] arrayRecords = new string[2, 2];


            using (StreamReader sr = new StreamReader("records", Encoding.Default))
            {
                for (int i = 0; i < 2; i++)
                {
                    string linha = sr.ReadLine();

                    if (linha != null)
                    {
                        string[] array = linha.Split(';');

                        arrayRecords[i, 0] = array[0];
                        arrayRecords[i, 1] = array[1];
                    }
                }
            }

            int ini = 0;
            int fin = 1;

            if (sentido == Sentido.BA)
            {
                ini = 1;
                fin = 2;
            }

            for (int i = ini; i < fin; i++)
            {
                arrayRecords[i, 0] = toques.ToString();
                arrayRecords[i, 1] = tempo;
              
            }

            using (StreamWriter sw = new StreamWriter("records", false, Encoding.Default))
            {
                for (int i = 0; i < 2; i++)
                    sw.WriteLine("Nº de toques:" + arrayRecords[i, 0] + "; Tempo Total:" + arrayRecords[i, 1]);
            }

            Dispatcher.Invoke(new Action(() => LeArquivoRecords()));
        }

        DateTime dtToque = DateTime.Now;

        private void Toque()
        {
            if (dTmr.IsEnabled)
            {
                if (DateTime.Now - dtToque > TimeSpan.FromMilliseconds(400))
                {
                    toques++;
                    Bass.BASS_ChannelPlay(streamBeep, false);
                    string strToques = toques > 1 ? string.Format("{0} toques", toques) : "1 toque";
                    Dispatcher.Invoke(new Action(() =>
                    {
                        if (dTmr.IsEnabled)
                        {
                            txtbToques.Text = strToques;
                            gridCen.Background = new SolidColorBrush(Colors.Red);
                            txtbTempo.Foreground = new SolidColorBrush(Colors.White);
                            txtbToques.Foreground = txtbTempo.Foreground;
                            txtbSentidoDescricao.Foreground = txtbTempo.Foreground;
                            txtbSentido.Foreground = txtbTempo.Foreground;
                            recImagemDisp.Fill = txtbTempo.Foreground;
                        }
                    }));
                    dtToque = DateTime.Now;
                }
            }
        }

        #endregion

        #region Handlers
               
        void dTmr_Tick(object sender, EventArgs e)
        {
            DateTime dtIni = (DateTime)dTmr.Tag;
            TimeSpan ts = DateTime.Now - dtIni;
            txtbTempo.Text = string.Format("{0:mm:ss.f}", new DateTime(1, 1, 1, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds));
        }

        #endregion
    }

    public enum Sentido
    {
        AB,
        BA
    }
}
