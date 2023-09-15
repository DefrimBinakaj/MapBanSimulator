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

namespace MapBanSimulator;

public partial class MainWindow : Window
{

    enum clickMode
    {
        rankingMode,
        banningMode
    }

    Dictionary<int, Color> rankColourTable = new Dictionary<int, Color>
    {
        {1, Colors.LawnGreen },
        {2, Colors.GreenYellow },
        {3, Colors.GreenYellow },
        {4, Colors.LightGreen },
        {5, Colors.Yellow },
        {6, Colors.Orange },
        {7, Colors.OrangeRed }
    };


    // default mode
    clickMode currMode = clickMode.banningMode;

    private Dictionary<string, (Rectangle redOverlay, Image cross, Image check)> mapBans;

    private ObservableCollection<string> mapRanking;







    // private static readonly HttpClient client = new HttpClient();

    // private Dictionary<string, double> mapPublicBanRate;








    public MainWindow()
    {
        InitializeComponent();
        initData();
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
        // ternary toggle logic
        // currMode = currMode == clickMode.rankingMode ? clickMode.banningMode : clickMode.rankingMode;


        if (currMode == clickMode.rankingMode)
        {
            currMode = clickMode.banningMode;
            mainBackground.Background = new SolidColorBrush(Colors.Maroon);
        }
        else if (currMode == clickMode.banningMode)
        {
            currMode = clickMode.rankingMode;
            mainBackground.Background = new SolidColorBrush(Colors.LightSlateGray);
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
            correspondingTextBlock.Foreground = new SolidColorBrush(rankColourTable[i + 1]);
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
    private void refreshMouseDown(object sender, RoutedEventArgs e) => refreshButton.Opacity = 1.0;
    private void refreshMouseUp(object sender, RoutedEventArgs e) => refreshButton.Opacity = 0.8;
}

