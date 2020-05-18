﻿using AlbionMarshaller.MemoryStorage;
using AlbionMarshaller.Model;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace LootUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Boolean AutoScroll = true;
        private AlbionProcessor.AlbionProcessor _processor = null;
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ObservableCollection<PlayerView> players = new ObservableCollection<PlayerView>();

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                _processor = new AlbionProcessor.AlbionProcessor();
                
                LootDB.Instance.LootAddedToPlayer += HandleLootAddedToPlayer;

                trvPlayerLoot.ItemsSource = players;
            }
            catch (Exception ex)
            {
                log.Error(ex.StackTrace);
            }
        }

        private void HandleLootAddedToPlayer(object sender, PlayerLootEventArgs plea)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                Loot loot = plea.Loot;
                Player player = plea.Player;
                Run run = new Run();
                run.Text = $"{player.Name} looted {loot.Quantity}x{loot.LongName} from {loot.BodyName} at {loot.PickupTime}\r\n";
                LootLog.Inlines.Add(run);
                
                PlayerView pv = players.FirstOrDefault(p => p.Name == player.Name);
                if (pv == null && player.Id == -1000)
                {
                    pv = players.FirstOrDefault(p => p.Id == player.Id);
                }
                if(pv == null)
                {
                    pv = new PlayerView() { Name = player.Name, Alliance = player.Alliance, Guild = player.Guild, Id = player.Id };
                    players.Add(pv);
                }
                else
                {
                    pv.Name = player.Name;
                    pv.Guild = player.Guild;
                    pv.Alliance = player.Alliance;
                }
                pv.Loot.Add(loot);
            }));
        }

        private void ScrollViewer_ScrollChanged(Object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer scroller = sender as ScrollViewer;
            // User scroll event : set or unset auto-scroll mode
            if (e.ExtentHeightChange == 0)
            {   // Content unchanged : user scroll event
                if (scroller.VerticalOffset == scroller.ScrollableHeight)
                {   // Scroll bar is in bottom
                    // Set auto-scroll mode
                    AutoScroll = true;
                }
                else
                {   // Scroll bar isn't in bottom
                    // Unset auto-scroll mode
                    AutoScroll = false;
                }
            }

            // Content scroll event : auto-scroll eventually
            if (AutoScroll && e.ExtentHeightChange != 0)
            {   // Content changed and auto-scroll mode set
                // Autoscroll
                scroller.ScrollToVerticalOffset(scroller.ExtentHeight);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //_logger.SaveLootsToFile();

            // Don't want to deal with the fucking open threads this guy made
            Environment.Exit(0);
        }

        private void ClearLoot_Click(object sender, RoutedEventArgs e)
        {
            LootLog.Inlines.Clear();
            players.Clear();
        }

        private void SaveFilter_Click(object sender, RoutedEventArgs e)
        {
            //_service.FilterString = this.Filter.Text;
        }

        private void Filter_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = (e.Source as TextBox).Text;
            foreach(PlayerView player in players)
            {
                if(player.Text.ToLower().Contains(text.ToLower()))
                {
                    player.IsHidden = Visibility.Visible;
                }
                else
                {
                    player.IsHidden = Visibility.Hidden;
                }
            }
        }
    }
}
