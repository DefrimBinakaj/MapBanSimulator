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
using System.ComponentModel;


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



    // init usercontrol / viewmodel instance
    private readonly SeptagonDrag septagonDrag;





    // general map list
    public List<string> mapList = new List<string> { "Anubis", "Inferno", "Mirage", "Vertigo", "Overpass", "Nuke", "Ancient" };

    // team 1
    public List<double> team1WinRateList = new List<double> { 0.69, 0.699, 0.6999, 0, 0, 0.69999, 0.699999 };
    public Dictionary<string, double> team1WinRateDict = new Dictionary<string, double>();

    // arbitrary values for preference ranking
    public List<double> team1PrefWeightList = new List<double> { 0.95, 0.9, 0.7, 0.4, 0.1, 0, 0 };
    Dictionary<string, double> team1PrefDict = new Dictionary<string, double>();


    // team 2
    Dictionary<string, double?> team2WinRateDict;





    public MainWindow()
    {
        InitializeComponent();
        initData();

        septagonDrag = new SeptagonDrag();

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










    private void calcFunc()
    {


        if (mapRanking.Count == 7)
        {
            // get prefDict
            team1PrefDict = getTeam1PrefList(mapRanking.ToList());
            Debug.WriteLine("team1prefDict --- " + string.Join(" ", team1PrefDict));

            // get winratedicts of both teams
            team1WinRateDict = getTeamWinrateDict(team1WinRateList);
            Debug.WriteLine("team1WinRateDict --- " + string.Join(" ", team1WinRateDict));

            // set winrateDict to current dragged percentages
            team2WinRateDict = new Dictionary<string, double?>(septagonDrag.dotPercentages);
            Debug.WriteLine("team2WinRateDict --- " + string.Join(" ", team2WinRateDict));

            // list of remaining maps not banned
            var remainingMaps = new List<string>(mapList);

            // list of the ban order just for printing
            List<string> banOrder = new List<string>();

            // ********************************************
            // team preference weights
            double team1PrefWeight = 0.5;
            double team1WinRateWeight = 1;
            double team2WinRateWeight = 1;
            // ********************************************

            // Team 1's first two bans
            for (int i = 0; i < 2; i++)
            {
                string ban = CalculateBestBanForTeam1(remainingMaps, team1PrefWeight, team1WinRateWeight, team2WinRateWeight);
                banOrder.Add(ban);
                remainingMaps.Remove(ban);
                Debug.WriteLine("t1 bans - " + ban);
            }

            // Team 2's three bans
            for (int i = 0; i < 3; i++)
            {
                string ban = CalculateBestBanForTeam2(remainingMaps, 0.5, 0.5);
                banOrder.Add(ban);
                remainingMaps.Remove(ban);
                Debug.WriteLine("t2 bans * " + ban);
            }

            // Team 1's final ban
            string finalBan = CalculateBestBanForTeam1(remainingMaps, team1PrefWeight, team1WinRateWeight, team2WinRateWeight);
            banOrder.Add(finalBan);
            remainingMaps.Remove(finalBan);
            Debug.WriteLine("t1 bans - " + finalBan);

            // The remaining map is the one to be played
            Debug.WriteLine("remaining map = " + remainingMaps.First());


        }

    }



    public Dictionary<string, double> getTeam1PrefList(List<string> prefList)
    {

        Dictionary<string, double> prefDict = new Dictionary<string, double>();

        for (int i = 0; i < prefList.Count; i++)
        {
            prefDict.Add(mapRanking[i], team1PrefWeightList[i]);
        }

        return prefDict;

    }





    public Dictionary<string, double> getTeamWinrateDict(List<double> winrateList)
    {

        Dictionary<string, double> winrateDict = new Dictionary<string, double>();

        for (int i = 0; i < winrateList.Count; i++)
        {
            winrateDict.Add(mapList[i], winrateList[i]);
        }

        return winrateDict;

    }


    // team1 ban calc
    private string CalculateBestBanForTeam1(List<string> remainingMaps, double prefWeight, double winRateWeight, double oppWinRateWeight)
    {
        Dictionary<string, double?> mapScores = remainingMaps.ToDictionary(
            map => map,
            map => (team1PrefDict[map] * prefWeight) +
                   (team1WinRateDict[map] * winRateWeight) -
                   (team2WinRateDict[map] * oppWinRateWeight)
        );
        Debug.WriteLine("team1banfn** " + string.Join(" ", mapScores));


        // create dict of entries that are the minimum
        var sameValEntries = mapScores.Where(entry => entry.Value == mapScores.Min(minVal => minVal.Value)).ToList();

        // if weight calc is 0, randomize entries to ban an arbitrary map instead of first in dict        
        if (sameValEntries.Count() > 1)
        {
            Debug.WriteLine("samevalentries1-------- " + string.Join(" ", sameValEntries));
            Random rand = new Random();
            // get rand entry
            return sameValEntries.ElementAt(rand.Next(0, sameValEntries.Count)).Key;
        }
        else
        {
            // get lowest weight
            return mapScores.OrderBy(kvp => kvp.Value).First().Key;
        }




    }


    // team2 ban calc
    private string CalculateBestBanForTeam2(List<string> remainingMaps, double team1WinRateWeight, double team2WinRateWeight)
    {
        // Team 2 aims to ban maps where Team 1 is strong and they are weak
        Dictionary<string, double?> mapScores = remainingMaps.ToDictionary(
        map => map,
        map => (team2WinRateDict[map] * team2WinRateWeight) -
               (team1WinRateDict[map] * team1WinRateWeight)
        );
        Debug.WriteLine("team2banfn# " + string.Join(" ", mapScores));


        // create dict of entries that are the minimum
        var sameValEntries = mapScores.Where(entry => entry.Value == mapScores.Min(minVal => minVal.Value)).ToList();

        // if weight calc is 0, randomize entries to ban an arbitrary map instead of first in dict        
        if (sameValEntries.Count() > 1)
        {
            Debug.WriteLine("samevalentries2-------- " + string.Join(" ", sameValEntries));
            Random rand = new Random();
            // get rand entry
            return sameValEntries.ElementAt(rand.Next(0, sameValEntries.Count)).Key;
        }
        else
        {
            // get lowest weight
            return mapScores.OrderBy(kvp => kvp.Value).First().Key;
        }



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


    private void toggleHover(object sender, RoutedEventArgs e) => toggleButton.Opacity = 0.6;
    private void toggleUnhover(object sender, RoutedEventArgs e) => toggleButton.Opacity = 0.8;
    private void toggleMouseDown(object sender, RoutedEventArgs e) => toggleButton.Opacity = 0.3;
    private void toggleMouseUp(object sender, RoutedEventArgs e) => toggleButton.Opacity = 0.8;
}

