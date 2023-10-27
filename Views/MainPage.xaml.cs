using BusinessLogicLayer.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Chess.Views
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public int GameTime { get; set; }

        public MainPage()
        {
            InitializeComponent();
            InitializeCulture();
            InitializeGameTime();
            InitializeContinueButtonVisibility();
        }

        private void InitializeCulture()
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Properties.Settings.Default.Culture);
        }

        private void InitializeGameTime()
        {
            GameTime = Properties.Settings.Default.Time;
            timeTextBlock.Text = $"{GameTime}";
        }

        private void InitializeContinueButtonVisibility()
        {
            if (ChessGame.IsSavedGame)
            {
                (continueButton.Parent as Border).Visibility = Visibility.Visible;
            }
        }

        private void upNum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GameTime = Math.Min(GameTime + 5, 30);
            ShowTimeAndSave();
        }

        private void downNum_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GameTime = Math.Max(GameTime - 5, 5);
            ShowTimeAndSave();
        }

        private void ShowTimeAndSave()
        {
            timeTextBlock.Text = $"{GameTime}";
            Properties.Settings.Default.Time = (byte)GameTime;
            Properties.Settings.Default.Save();
        }
        // Создание экземпляра Image из файла изображения

        // Присвоение изображения в качестве фонового изображения элементу управления

    }
}