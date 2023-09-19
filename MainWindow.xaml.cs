using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Input;

using MapBanSimulator.UserControls;
using MapBanSimulator.Models;
using MapBanSimulator.ViewModels;

namespace MapBanSimulator;

public partial class MainWindow : Window
{

    enum clickMode
    {
        rankingMode,
        banningMode
    }


    // default mode
    clickMode currMode = clickMode.banningMode;

    private Dictionary<string, (Rectangle redOverlay, Image cross, Image check)> mapBans;

    private ObservableCollection<string> mapRanking;


    public MainWindow()
    {
        InitializeComponent();
        initData();

        mapRankingListView.DataContext = mapRanking;

    }



    private void initData()
    {
        mapBans = new Dictionary<string, (Rectangle, Image, Image)>
        {
            {"Anubis", (anubisRedOverlay, anubisCross, anubisCheck)},
            {"Ancient", (ancientRedOverlay, ancientCross, ancientCheck)},
            {"Inferno", (infernoRedOverlay, infernoCross, infernoCheck)},
            {"Mirage", (mirageRedOverlay, mirageCross, mirageCheck)},
            {"Vertigo", (vertigoRedOverlay, vertigoCross, vertigoCheck)},
            {"Overpass", (overpassRedOverlay, overpassCross, overpassCheck)},
            {"Nuke", (nukeRedOverlay, nukeCross, nukeCheck)}
        };

        // subscribe UI to maprank update for better decoupling (instead of calling it from inside the mapClicked function as a List<string>
        mapRanking = new ObservableCollection<string>();
        mapRanking.CollectionChanged += mapRankCollecUpdate;
    }



    private void clickModeToggle(object sender, RoutedEventArgs e)
    {
        clearBans();
        clearRanks();

        if (currMode == clickMode.rankingMode)
        {
            currMode = clickMode.banningMode;
            // mainBackground.Background = new SolidColorBrush(Colors.RosyBrown);
            blueBackground.Visibility = Visibility.Hidden;
            redBackground.Visibility = Visibility.Visible;
        }
        else if (currMode == clickMode.banningMode)
        {
            currMode = clickMode.rankingMode;
            // mainBackground.Background = new SolidColorBrush(Colors.LightSlateGray);
            redBackground.Visibility = Visibility.Hidden;
            blueBackground.Visibility = Visibility.Visible;
        }

    }





    private void clearBans()
    {
        foreach (var control in mapBans)
        {
            control.Value.redOverlay.Visibility = Visibility.Hidden;
            control.Value.cross.Visibility = Visibility.Hidden;
            control.Value.check.Visibility = Visibility.Hidden;
        }
    }


    private void clearRanks()
    {
        foreach (var mapName in mapRanking)
        {
            TextBlock correspondingTextBlock = getRankText(mapName);
            correspondingTextBlock.Text = string.Empty;
            correspondingTextBlock.Visibility = Visibility.Hidden;
        }
        mapRanking.Clear();

    }

    private void clearAll()
    {
        clearBans();
        clearRanks();
    }

    private void MapClicked(string mapName)
    {
        switch (currMode)
        {
            case clickMode.banningMode:
                banClick(mapName);
                break;

            case clickMode.rankingMode:
                rankClick(mapName);
                break;

            default:
                Debug.WriteLine("clickMode is incorrect");
                break;
        }
        calcFunc();
    }



    private void banClick(string mapName)
    {
        mapBans[mapName].redOverlay.Visibility = Visibility.Visible;
        mapBans[mapName].cross.Visibility = Visibility.Visible;

        int banCount = mapBans.Count(mpCntrl => mpCntrl.Value.redOverlay.Visibility == Visibility.Visible);

        if (banCount == 6)
        {
            string finalMap = mapBans.Last(mapCntrl => mapCntrl.Value.redOverlay.Visibility == Visibility.Hidden).Key;
            mapBans[finalMap].check.Visibility = Visibility.Visible;
        }
    }

    private void rankClick(string mapName)
    {
        if (!mapRanking.Contains(mapName))
        {
            mapRanking.Add(mapName);
        }
        else if (mapRanking.Contains(mapName))
        {
            mapRanking.Remove(mapName);
        }

    }


    private TextBlock getRankText(string mapName)
    {
        return (TextBlock)this.FindName(mapName.ToLower() + "Rank");
    }




    private void mapRankCollecUpdate(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (var mapName in mapBans.Keys)
        {
            getRankText(mapName).Visibility = Visibility.Hidden;
        }

        for (int i = 0; i < mapRanking.Count; i++)
        {
            string mapName = mapRanking[i];
            TextBlock correspondingTextBlock = getRankText(mapName);
            correspondingTextBlock.Text = (i + 1).ToString();
            correspondingTextBlock.Visibility = Visibility.Visible;
            correspondingTextBlock.Foreground = new SolidColorBrush(ConstantSets.rankColourTable[i + 1]);
        }
    }





    private void calcFunc()
    {

        Dictionary<string, double> mapPubPlayRate = ConstantSets.MapPublicPlayRate;



        if (mapRanking.Count == 7)
        {
            // maps likely to be banned by enemy due to lowest play rates.
            var potentialEnemyBans = mapPubPlayRate.OrderBy(m => m.Value).Take(3).Select(m => m.Key).ToList();

            // ban your least favorite maps that aren't likely to be banned by the enemy.
            var myBans = mapRanking.TakeLast(2)
                                          .Where(m => !potentialEnemyBans.Contains(m))
                                          .ToList();

            // If you didn't ban 2 maps in the initial phase because potential enemy bans overlap with your least favorite maps, 
            // ban your next least favorite map that isn't likely to be banned by the enemy.
            while (myBans.Count < 2)
            {
                var nextBan = mapRanking.Except(myBans).Except(potentialEnemyBans).Last();
                myBans.Add(nextBan);
            }

            // if the enemy does not ban your least favorite map (last in mapRanking), you should ban it in your final ban.
            var finalBan = potentialEnemyBans.Contains(mapRanking.Last())
                           ? mapRanking.Except(myBans).Except(potentialEnemyBans).Last()
                           : mapRanking.Last();

            myBans.Add(finalBan);

            Debug.WriteLine("First two bans:");
            foreach (var map in myBans.Take(2))
            {
                Debug.WriteLine("You should ban: " + map);
            }

            Debug.WriteLine("Expected enemy bans:");
            foreach (var map in potentialEnemyBans)
            {
                Debug.WriteLine("Enemy will likely ban: " + map);
            }

            Debug.WriteLine("Final ban:");
            Debug.WriteLine("You should ban: " + finalBan);
            Debug.WriteLine("Map to be played: " + mapRanking.Last().ToString());

            Debug.WriteLine("\n//");
        }
        else
        {
            Debug.WriteLine("Not enough maps yet");
        }
        

    }






    private void anubisClicked(object sender, RoutedEventArgs e) => MapClicked("Anubis");
    private void ancientClicked(object sender, RoutedEventArgs e) => MapClicked("Ancient");
    private void infernoClicked(object sender, RoutedEventArgs e) => MapClicked("Inferno");
    private void nukeClicked(object sender, RoutedEventArgs e) => MapClicked("Nuke");
    private void mirageClicked(object sender, RoutedEventArgs e) => MapClicked("Mirage");
    private void overpassClicked(object sender, RoutedEventArgs e) => MapClicked("Overpass");
    private void vertigoClicked(object sender, RoutedEventArgs e) => MapClicked("Vertigo");
    
    private void refreshClicked(object sender, RoutedEventArgs e) => clearAll();
    
    private void refreshHover(object sender, RoutedEventArgs e) => refreshButton.Opacity = 0.6;
    private void refreshUnhover(object sender, RoutedEventArgs e) => refreshButton.Opacity = 0.8;
    private void refreshMouseDown(object sender, RoutedEventArgs e) => refreshButton.Opacity = 0.3;
    private void refreshMouseUp(object sender, RoutedEventArgs e) => refreshButton.Opacity = 0.8;
}

