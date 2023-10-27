using BusinessLogicLayer.Services;
using Chess.Views;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chess.ViewModels
{
    public class PageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MainPage mainPage;
        private GamePage gamePage;
        private Page currentPage;

        public Page CurrentPage
        {
            get => currentPage;
            set
            {
                currentPage = value;
                OnPropertyChanged();
            }
        }

        public ICommand StartGame
        {
            get => new RelayCommand(() => {
                CurrentPage = gamePage;
                gamePage.GameTime = mainPage.GameTime;
                ChessGame.StartGame();
            });
        }

        public ICommand ContinueGame
        {
            get => new RelayCommand(() =>
            {
                CurrentPage = gamePage;
                (mainPage.continueButton.Parent as Border).Visibility = Visibility.Hidden;
                ChessGame.ContinueGame();
            });
        }

        public ICommand OpenRules
        {
            get => new RelayCommand(() => {
            });
        }

        public ICommand GoBack
        {
            get => new RelayCommand(() => {
                CurrentPage = mainPage;
                if (ChessGame.IsSavedGame)
                {
                    (mainPage.continueButton.Parent as Border).Visibility = Visibility.Visible;
                }
                else (mainPage.continueButton.Parent as Border).Visibility = Visibility.Hidden;
            });
        }

        public PageViewModel()
        {
            mainPage = new MainPage();
            gamePage = new GamePage();

            mainPage.playButton.Command = StartGame;
            mainPage.continueButton.Command = ContinueGame;

            CurrentPage = mainPage;
        }

        public void UpdateLocalization(string local)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(local);
            Properties.Settings.Default.Culture = local;
            Properties.Settings.Default.Save();
        }

        public void OnPropertyChanged([CallerMemberName] string property = null)
        {
            if(PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }
    }
}
