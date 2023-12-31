﻿using BusinessLogicLayer.Models;
using BusinessLogicLayer.Services;
using Chess.ViewModels;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace Chess
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Image selectedFigure = null;
        private System.Drawing.Point startPos;
        private System.Drawing.Point endPos;
        private Grid startGrid = null;

        public MainWindow()
        {
            InitializeComponent();
            ChessGame.GameStarting += StartGameCommand;
            RestorePosition();
        }

        private void RestorePosition()
        {
            if (Properties.Settings.Default.Position.Height == 0) return;
            this.Height = Properties.Settings.Default.Position.Height;
            this.Width = Properties.Settings.Default.Position.Width;
            this.Top = Properties.Settings.Default.Position.Y;
            this.Left = Properties.Settings.Default.Position.X;
        }

        private void CleanBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    (gameboardGrid.Children[i * 8 + j] as Grid).Children.Clear();
                    (gameboardGrid.Children[i * 8 + j] as Grid).Background = new SolidColorBrush(((i + j) % 2 == 0 ?
                        Color.FromArgb(255, 128, 128, 128) : Color.FromArgb(255, 255, 255, 255)));
                }
            }
        }

        private void SetBoardForRuls(object sender, EventArgs e)
        {
            var list = (sender as List<IFigure>);
            CleanBoard();

            list.ForEach(f =>
            {
                string path = $"/Images/{ChessConverter.ConvertFigureToSource(f)}.png";
                AddFigure(path, new Point(f.Position.X, f.Position.Y), f.IsWhite);
                
                foreach(var p in ChessGame.GetMoves(f.Position)) {
                }
            });
        }

        private void FillGameboard()
        {
            for (int i = 0; i < 10; i++)
            {
                gameboardGrid.RowDefinitions.Add(new RowDefinition());
                gameboardGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
            gameboardGrid.RowDefinitions[0].Height = new GridLength(0.4, GridUnitType.Star);
            gameboardGrid.ColumnDefinitions[0].Width = new GridLength(0.4, GridUnitType.Star);
            gameboardGrid.RowDefinitions[9].Height = new GridLength(0.4, GridUnitType.Star);
            gameboardGrid.ColumnDefinitions[9].Width = new GridLength(0.4, GridUnitType.Star);

            for (int i = 1; i < 9; i++)
            {
                for (int j = 1; j < 9; j++)
                {
                    var grid = new Grid() {
                        Background = new SolidColorBrush(((i + j) % 2 == 0 ?
                        Color.FromArgb(255, 128, 128, 128) : Color.FromArgb(255, 255, 255, 255))),
                    };

                    grid.MouseEnter += gridMouseEnter;

                    gameboardGrid.Children.Add(grid);
                    Grid.SetColumn(grid, j);
                    Grid.SetRow(grid, i);
                }
            }

            for (int i = 1; i < 9; i++)
            {
                var textNum = new TextBlock() { Text = i.ToString() };
                var textLetter = new TextBlock() { Text = ((char)('a' + (i - 1))).ToString() };
                gameboardGrid.Children.Add(textNum);
                Grid.SetRow(textNum, 9 - i);
                Grid.SetColumn(textNum, 0);
                gameboardGrid.Children.Add(textLetter);
                Grid.SetRow(textLetter, 0);
                Grid.SetColumn(textLetter, i);
            }
        }

        private void gridMouseEnter(object sender, MouseEventArgs e)
        {
            // Проверяем, если выбранная фигура не равна null
            if (selectedFigure != null)
            {
                // Удаляем выбранную фигуру из временного холста
                tempCanvas.Children.Remove(selectedFigure);

                // Получаем конечные координаты
                endPos = new System.Drawing.Point(Grid.GetColumn((sender as Grid)) - 1, Grid.GetRow((sender as Grid)) - 1);

                // Проверяем, можно ли сделать ход
                if (ChessGame.CanMove(startPos, endPos))
                {
                    // Очищаем текущую ячейку и добавляем выбранную фигуру в новую ячейку
                    (sender as Grid).Children.Clear();
                    (sender as Grid).Children.Add(selectedFigure);

                    // Делаем ход в игре
                    ChessGame.Move(startPos, endPos);

                    // Проверяем, является ли выбранная фигура пешкой и находится ли она на конечной позиции (на первом или последнем ряду)
                    if (selectedFigure.Source.ToString().Contains("pawn") && (endPos.Y == 0 || endPos.Y == 7))
                    {
                        // Открываем popup с выбором превращения пешки
                        choicePopup.IsOpen = true;
                    }
                    else if (ChessGame.ToweringPos != null)
                    {
                        // Получаем позицию башни в игре
                        var towerPoint = new Point(ChessGame.ToweringPos.Value.X, ChessGame.ToweringPos.Value.Y);

                        // Проверяем позицию башни и перемещаем ее на соответствующую позицию на игровом поле
                        if (towerPoint.X == 2)
                        {
                            var towerImg = ((gameboardGrid.Children[(int)(towerPoint.Y * 8 + 0)] as Grid).Children[0] as Image);
                            (gameboardGrid.Children[(int)(towerPoint.Y * 8 + 0)] as Grid).Children.Remove(towerImg);
                            (gameboardGrid.Children[(int)(towerPoint.Y * 8 + 2)] as Grid).Children.Add(towerImg);
                            (gameboardGrid.Children[(int)(towerPoint.Y * 8 + 0)] as Grid).Children.Clear();
                        }
                        else if (towerPoint.X == 5)
                        {
                            var towerImg = ((gameboardGrid.Children[(int)(towerPoint.Y * 8 + 7)] as Grid).Children[0] as Image);
                            (gameboardGrid.Children[(int)(towerPoint.Y * 8 + 7)] as Grid).Children.Remove(towerImg);
                            (gameboardGrid.Children[(int)(towerPoint.Y * 8 + 5)] as Grid).Children.Add(towerImg);
                            (gameboardGrid.Children[(int)(towerPoint.Y * 8 + 7)] as Grid).Children.Clear();
                        }
                    }
                }
                else
                {
                    // Если ход недопустим, добавляем выбранную фигуру обратно в исходную ячейку
                    startGrid.Children.Add(selectedFigure);
                }

                // Обновляем игровое поле
                RefreshBoard();

                // Сбрасываем значения переменных
                startGrid = null;
                selectedFigure = null;
            }
        }

        private void RefreshBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    (gameboardGrid.Children[i * 8 + j] as Grid).Background = new SolidColorBrush(((i + j) % 2 == 0 ?
                        Color.FromArgb(255, 128, 128, 128) : Color.FromArgb(255, 255, 255, 255)));
                }
            }

            if (ChessGame.IsCheck(out System.Drawing.Point position))
            {
                (gameboardGrid.Children[position.Y * 8 + position.X] as Grid).Background = Brushes.Red;
            }
        }


        private void AddFigure(string path, Point point, bool isWhite = true)
        {
            Image pawn = new Image()
            {
                Source = new BitmapImage(new Uri(path, UriKind.Relative))
            };
            if(isWhite)
                pawn.MouseLeftButtonDown += WhiteFigureMouseDown;
            else pawn.MouseLeftButtonDown += BlackFigureMouseDown;
            (gameboardGrid.Children[(int)(point.Y * 8 + point.X)] as Grid).Children.Add(pawn);
        }

        private void SetFigures()
        {
            foreach(var item in ChessGame.WhiteFigures)
            {
                string path = ChessConverter.ConvertFigureToSource(item);

                AddFigure($"/Images/{path}.png", new Point(item.Position.X, item.Position.Y));
            }

            foreach (var item in ChessGame.BlackFigures)
            {
                string path = ChessConverter.ConvertFigureToSource(item);

                AddFigure($"/Images/{path}.png", new Point(item.Position.X, item.Position.Y), false);
            }
        }

        private void WhiteFigureMouseDown(object sender, MouseEventArgs e)
        {
            if (!ChessGame.WhiteTurn) return;
            FigureLeftButtonMouseDown(sender, e);
        }

        private void BlackFigureMouseDown(object sender, MouseEventArgs e)
        {
            if (ChessGame.WhiteTurn) return;
            FigureLeftButtonMouseDown(sender, e);
        }

        private void FigureLeftButtonMouseDown(object sender, MouseEventArgs e)
        {
            if (choicePopup.IsOpen) return;
            startGrid = (sender as Image).Parent as Grid;
            startPos = new System.Drawing.Point(Grid.GetColumn((sender as Image).Parent as Grid) - 1,
                Grid.GetRow((sender as Image).Parent as Grid) - 1);

            var attacks = ChessGame.GetAttacks(startPos);
            foreach (var p in attacks)
            {
                (gameboardGrid.Children[(p.Y) * 8 + p.X] as Grid).Background = Brushes.Red;
            }

            selectedFigure = (sender as Image);

            Panel.SetZIndex(tempCanvas, 1);
            startGrid.Children.Clear();
            var grid = new Grid()
            {
                Background = startGrid.Background
            };
            grid.MouseEnter += gridMouseEnter;
            int c = Grid.GetColumn(startGrid);
            int r = Grid.GetRow(startGrid);
            int index = gameboardGrid.Children.IndexOf(startGrid);
            gameboardGrid.Children.RemoveAt(index);
            gameboardGrid.Children.Insert(index, grid);
            Grid.SetColumn(grid, c);
            Grid.SetRow(grid, r);
            startGrid = grid;
            tempCanvas.Children.Add(selectedFigure);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FillGameboard();
        }

        private void tempCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedFigure == null) return;

        }

        private void tempCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (selectedFigure == null) return;
            Panel.SetZIndex(tempCanvas, -1);
        }

        private void StartGameCommand(object sender, EventArgs e)
        {
            winnerPanel.Visibility = Visibility.Hidden;
            CleanBoard();
            SetFigures();
        }

        private void ChangePawn_MouseUp(object sender, MouseButtonEventArgs e)
        {
            choicePopup.IsOpen = false;
            var image = (e.Source as Image).Source.ToString();
            IFigure figure;
            if (image.Contains("queen"))
                figure = new Queen();
            else if (image.Contains("bishop"))
                figure = new Bishop();
            else if (image.Contains("knight"))
                figure = new Knight();
            else figure = new Tower();

            ChessGame.ChangePawn(endPos, figure);
            ((gameboardGrid.Children[endPos.Y * 8 + endPos.X] as Grid).Children[0] as Image).Source =
                new BitmapImage(new Uri($"/Images/{ChessConverter.ConvertFigureToSource(figure)}.png", UriKind.Relative));
        }

        private void winnerPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            winnerPanel.Visibility = Visibility.Hidden;
            CleanBoard();
            (this.DataContext as PageViewModel).GoBack.Execute(null);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Position = this.RestoreBounds;
            Properties.Settings.Default.Save();
        }

        private void mainFrame_Navigated(object sender, NavigationEventArgs e)
        {

        }

        private void mainFrame_Navigated_1(object sender, NavigationEventArgs e)
        {

        }
    }
}
