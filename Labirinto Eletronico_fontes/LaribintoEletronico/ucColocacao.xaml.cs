using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LaribintoEletronico
{
    /// <summary>
    /// Interaction logic for ucColocacao.xaml
    /// </summary>
    public partial class ucColocacao : UserControl
    {
        public ucColocacao()
        {
            InitializeComponent();

            txtbToques.Text = "-----------";
            txtbTempo.Text = "--------";
        }

        public int Posicao
        {
            get { return (int)GetValue(PosicaoProperty); }
            set { SetValue(PosicaoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Posicao.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PosicaoProperty =
            DependencyProperty.Register("Posicao", typeof(int), typeof(ucColocacao), new PropertyMetadata(-1, PosicaoChanged_Callback));

        private static void PosicaoChanged_Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucColocacao uc = (ucColocacao)d;
            uc.txtbPosicao.Text = e.NewValue.ToString() + ".";
        }


        public int Toques
        {
            get { return (int)GetValue(ToquesProperty); }
            set { SetValue(ToquesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Toques.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToquesProperty =
            DependencyProperty.Register("Toques", typeof(int), typeof(ucColocacao), new PropertyMetadata(-1, ToquesChanged_Callback));

        private static void ToquesChanged_Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucColocacao uc = (ucColocacao)d;
            int toques = (int)e.NewValue;
            uc.txtbToques.Text = toques.ToString() + ((toques > 1) ? " toques" : " toque");            
        }

        public TimeSpan Tempo
        {
            get { return (TimeSpan)GetValue(TempoProperty); }
            set { SetValue(TempoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Tempo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TempoProperty =
            DependencyProperty.Register("Tempo", typeof(TimeSpan), typeof(ucColocacao), new PropertyMetadata(TimeSpan.Zero, TempoChanged_Callback));

        private static void TempoChanged_Callback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ucColocacao uc = (ucColocacao)d;
            string tempo = string.Format("{0:mm:ss.f}", new DateTime(1, 1, 1, uc.Tempo.Hours, uc.Tempo.Minutes, uc.Tempo.Seconds, uc.Tempo.Milliseconds));
            uc.txtbTempo.Text = tempo;
        }
        
    }
}
