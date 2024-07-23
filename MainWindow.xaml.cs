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
using System.ComponentModel;


namespace MapBanSimulator;



/**
 * 
 * Mainwindow Class
 * 
 * 
 * 
 */

public partial class MainWindow : Window, INotifyPropertyChanged
{

    // UI mode
    enum clickMode
    {
        rankingMode,
        banningMode
    }
    // set default mode to banning
    clickMode currMode = clickMode.banningMode;

    // dict of maps and bans
    private Dictionary<string, (Rectangle redOverlay, Image cross, Image check)> mapBans;

    // map ranking list
    private ObservableCollection<string> mapRanking;


    // map ranking % (1-7)
    public Dictionary<string, double> team1PrefRankDict = new Dictionary<string, double>()
    {
        {"Anubis", 0.00},
        {"Inferno", 0.00},
        {"Mirage", 0.00},
        {"Vertigo", 0.00},
        {"Overpass", 0.00},
        {"Nuke", 0.00},
        {"Ancient", 0.00}
    };

    // list that assigns a numeric value to the map preference listing. 1 being the 1st map preferred, 0.3 being the 3rd last map preferred
    public List<double> prefRankSequence = new List<double>() { 1, 0.8, 0.6, 0.4, 0.3, 0, 0 };


    // vertical slider drag init
    private Dictionary<string, bool> dragStateDict = new Dictionary<string, bool>()
    {
        { "anubisSliderTag", false },
        { "ancientSliderTag", false },
        { "infernoSliderTag", false },
        { "nukeSliderTag", false},
        { "mirageSliderTag", false },
        { "overpassSliderTag", false },
        { "vertigoSliderTag", false },
        { "team1PrefWeightSliderTag", false },
        { "team1WinRateWeightSliderTag", false },
        { "team2WinRateWeightSliderTag", false }
    };

    // team 1 vertical slider vals (have initial value so that calculation isnt random if the sliders arent touched)
    public Dictionary<string, double?> team1WinRateDict = new Dictionary<string, double?>()
    {
        {"Anubis", 0.00},
        {"Inferno", 0.00},
        {"Mirage", 0.00},
        {"Vertigo", 0.00},
        {"Overpass", 0.00},
        {"Nuke", 0.00},
        {"Ancient", 0.00}
    };



    // team 2 init (red dots)
    public Dictionary<string, double?> team2WinRateDict = new Dictionary<string, double?>()
    {
        {"Anubis", 0.00},
        {"Inferno", 0.00},
        {"Mirage", 0.00},
        {"Vertigo", 0.00},
        {"Overpass", 0.00},
        {"Nuke", 0.00},
        {"Ancient", 0.00}
    };



    // team preference weights
    private double team1PrefWeight = 0.00;
    private double team1WinRateWeight = 0.00;
    private double team2WinRateWeight = 0.00;


    // ********************************************





    public MainWindow()
    {
        InitializeComponent();
        initData();
        this.DataContext = this;

        // instead of creating a new SeptagonDrag instance, you must use the one created in your XAML
        // otherwise you will have issues tying subscriptions and events to the UI
        septagonDragXAMLinstance.PercentagesUpdated += SeptagonDrag_PercentagesUpdated;
    }

    // UI dot to percentage update
    private void SeptagonDrag_PercentagesUpdated(object sender, EventArgs e)
    {
        // read updated %
        team2WinRateDict = septagonDragXAMLinstance.dotPercentages;
        // update UI to new %
        Debug.WriteLine(string.Join(" ", team2WinRateDict));
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




    // ********************************************





    // winrate calc
    private void calcFunc()
    {
        // if all maps are ranked/clicked, then continue
        if (mapRanking.Count == 7)
        {
            Debug.WriteLine("team1prefweight= " + team1PrefWeight + " -- team1WinRateWeight= " + team1WinRateWeight + " -- team2WinRateWeight= " + team2WinRateWeight);

            Debug.WriteLine("team1prefDict --- " + string.Join(" ", team1PrefRankDict));

            Debug.WriteLine("team1WinRateDict --- " + string.Join(" ", team1WinRateDict));

            Debug.WriteLine("team2WinRateDict --- " + string.Join(" ", team2WinRateDict));

            // list of remaining maps not banned
            List<string> remainingMapsBanFirst = new List<string> { "Anubis", "Inferno", "Mirage", "Vertigo", "Overpass", "Nuke", "Ancient" };
            List<string> remainingMapsBanSecond = new List<string> { "Anubis", "Inferno", "Mirage", "Vertigo", "Overpass", "Nuke", "Ancient" };

            // list of the ban order just for printing
            List<string> banOrderBanFirst = new List<string>();
            List<string> banOrderBanSecond = new List<string>();

            // =====================
            // 1st pick

            // team 1's first two bans
            for (int i = 0; i < 2; i++)
            {
                string ban = CalculateBestBanForTeam1(remainingMapsBanFirst);
                banOrderBanFirst.Add(ban);
                remainingMapsBanFirst.Remove(ban);
                Debug.WriteLine("t1 bans - " + ban);
                if (i == 0) { firstBan1 = ban; }
                if (i == 1) { firstBan2 = ban; }
            }

            // team 2's three bans
            for (int i = 0; i < 3; i++)
            {
                string ban = CalculateBestBanForTeam2(remainingMapsBanFirst, 0.5, 0.5);
                banOrderBanFirst.Add(ban);
                remainingMapsBanFirst.Remove(ban);
                Debug.WriteLine("t2 bans * " + ban);
                if (i == 0) { firstBan3 = ban; }
                if (i == 1) { firstBan4 = ban; }
                if (i == 2) { firstBan5 = ban; }
            }

            // team 1's final ban
            string finalBanFirst = CalculateBestBanForTeam1(remainingMapsBanFirst);
            banOrderBanFirst.Add(finalBanFirst);
            remainingMapsBanFirst.Remove(finalBanFirst);
            Debug.WriteLine("t1 bans - " + finalBanFirst);
            firstBan6 = finalBanFirst;

            // remaining map to be played
            Debug.WriteLine("remaining map = " + remainingMapsBanFirst.First());
            firstChoice = remainingMapsBanFirst.First();
            remainingMapsBanFirst.Remove(remainingMapsBanFirst.First());



            // =====================
            // 2nd pick

            // team 2's first two bans
            for (int i = 0; i < 2; i++)
            {
                string ban = CalculateBestBanForTeam2(remainingMapsBanSecond, 0.5, 0.5);
                banOrderBanSecond.Add(ban);
                remainingMapsBanSecond.Remove(ban);
                Debug.WriteLine("t2 bans * " + ban);
                if (i == 0) { secondBan1 = ban; }
                if (i == 1) { secondBan2 = ban; }
            }

            // team 1's three bans
            for (int i = 0; i < 3; i++)
            {
                string ban = CalculateBestBanForTeam1(remainingMapsBanSecond);
                banOrderBanSecond.Add(ban);
                remainingMapsBanSecond.Remove(ban);
                Debug.WriteLine("t1 bans - " + ban);
                if (i == 0) { secondBan3 = ban; }
                if (i == 1) { secondBan4 = ban; }
                if (i == 2) { secondBan5 = ban; }
            }

            // team 2's final ban
            string finalBanSecond = CalculateBestBanForTeam2(remainingMapsBanSecond, 0.5, 0.5);
            banOrderBanSecond.Add(finalBanSecond);
            remainingMapsBanSecond.Remove(finalBanSecond);
            Debug.WriteLine("t2 bans - " + finalBanSecond);
            secondBan6 = finalBanSecond;

            // remaining map to be played
            Debug.WriteLine("remaining map = " + remainingMapsBanSecond.First());
            secondChoice = remainingMapsBanSecond.First();
            remainingMapsBanSecond.Remove(remainingMapsBanSecond.First());


            OnPropertyChanged(nameof(FirstBan1));
            OnPropertyChanged(nameof(FirstBan2));
            OnPropertyChanged(nameof(FirstBan3));
            OnPropertyChanged(nameof(FirstBan4));
            OnPropertyChanged(nameof(FirstBan5));
            OnPropertyChanged(nameof(FirstBan6));
            OnPropertyChanged(nameof(FirstChoice));
            OnPropertyChanged(nameof(SecondBan1));
            OnPropertyChanged(nameof(SecondBan2));
            OnPropertyChanged(nameof(SecondBan3));
            OnPropertyChanged(nameof(SecondBan4));
            OnPropertyChanged(nameof(SecondBan5));
            OnPropertyChanged(nameof(SecondBan6));
            OnPropertyChanged(nameof(SecondChoice));
        }

    }




    // team1 ban calc
    private string CalculateBestBanForTeam1(List<string> remainingMaps)
    {
        Dictionary<string, double?> mapScores = remainingMaps.ToDictionary(
            map => map,
            map => (team1PrefRankDict[map] * team1PrefWeight) +
                   (team1WinRateDict[map] * team1WinRateWeight) -
                   (team2WinRateDict[map] * team2WinRateWeight)
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



    // change UI mode
    private void clickModeToggle(object sender, RoutedEventArgs e)
    {
        clearBans();
        clearRanks();

        if (currMode == clickMode.rankingMode)
        {
            currMode = clickMode.banningMode;
            blueBackground.Visibility = Visibility.Hidden;
            redBackground.Visibility = Visibility.Visible;
        }
        else if (currMode == clickMode.banningMode)
        {
            currMode = clickMode.rankingMode;
            redBackground.Visibility = Visibility.Hidden;
            blueBackground.Visibility = Visibility.Visible;
        }

    }




    // clear all banned maps
    private void clearBans()
    {
        foreach (var control in mapBans)
        {
            control.Value.redOverlay.Visibility = Visibility.Hidden;
            control.Value.cross.Visibility = Visibility.Hidden;
            control.Value.check.Visibility = Visibility.Hidden;
        }
    }


    // clear all map ranks
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


    // clear both bans and ranks
    private void clearAll()
    {
        clearBans();
        clearRanks();
    }

    // click a map and decide logic based on UI mode
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

    // map clicked in ban mode
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

    // map clicked in rank mode
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


    // get rank of map
    private TextBlock getRankText(string mapName)
    {
        return (TextBlock)this.FindName(mapName.ToLower() + "Rank");
    }



    // func used for reorganizing the rank once a map is clicked - if 2nd is clicked while 5 maps are ranked, all of them shuffle to account for lost rank
    private void mapRankCollecUpdate(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (var mapName in mapBans.Keys)
        {
            TextBlock correspondingTextBlock = getRankText(mapName);
            if (correspondingTextBlock != null)
            {
                correspondingTextBlock.Visibility = Visibility.Hidden;
                correspondingTextBlock.Text = "";
            }
        }

        // Update preference weight for each ranked map and update UI
        for (int i = 0; i < mapRanking.Count; i++)
        {
            string mapName = mapRanking[i];
            double currentWeight = i < prefRankSequence.Count ? prefRankSequence[i] : 0; // Fallback to 0 if index exceeds list

            // Update the preference weight
            team1PrefRankDict[mapName] = currentWeight;

            // Update the UI for each map's rank
            TextBlock correspondingTextBlock = getRankText(mapName);
            if (correspondingTextBlock != null)
            {
                correspondingTextBlock.Text = (i + 1).ToString();
                correspondingTextBlock.Visibility = Visibility.Visible;
                correspondingTextBlock.Foreground = new SolidColorBrush(ConstantSets.rankColourTable[i + 1]);
            }
        }
    }



    // canvas mouse pressed down
    private void canvasSlider_mouseDown(object sender, MouseButtonEventArgs e)
    {

        if (sender is Canvas)
        {
            Canvas? canvas = (Canvas)sender;
            dragStateDict[canvas.Tag.ToString()] = true;
            UpdateCanvasSlider(e, canvas);
        }

    }
    // canvas mouse moved
    private void canvasSlider_mouseMove(object sender, MouseEventArgs e)
    {
        if (sender is Canvas)
        {
            Canvas? canvas = (Canvas)sender;
            if (dragStateDict[canvas.Tag.ToString()] == true)
            {
                UpdateCanvasSlider(e, canvas);
            }
            
        }
    }
    // canvas mouse pressed up
    private void canvasSlider_mouseUp(object sender, MouseButtonEventArgs e)
    {
        if (sender is Canvas)
        {
            Canvas? canvas = (Canvas)sender;
            dragStateDict[canvas.Tag.ToString()] = false;
        }
    }
    // canvas mouse moved off of the canvas
    private void canvasSlider_mouseLeave(object sender, MouseEventArgs e)
    {
        if (sender is Canvas)
        {
            Canvas? canvas = (Canvas)sender;
            dragStateDict[canvas.Tag.ToString()] = false;
        }
    }
    // updates the var related to the specific canvas
    private void UpdateCanvasSlider(MouseEventArgs mouseEvent, Canvas sliderCanvas)
    {
        // init children of the canvas in order to manipulate them
        var sliderBackground = sliderCanvas.Children[0] as Rectangle;
        var sliderIndicator = sliderCanvas.Children[1] as Rectangle;

        if (sliderBackground == null || sliderIndicator == null) return;


        double percentage = 0.0;


        if (sliderCanvas.Tag.ToString().Contains("WeightSliderTag"))
        {

            double sliderLeft = Canvas.GetLeft(sliderBackground);
            double sliderWidth = sliderBackground.Width;

            double relativeXPos = mouseEvent.GetPosition(sliderCanvas).X - sliderLeft;
            relativeXPos = Math.Max(0, Math.Min(relativeXPos, sliderWidth));

            percentage = relativeXPos / sliderWidth;


            sliderIndicator.Width = sliderWidth * percentage;
            Canvas.SetLeft(sliderIndicator, sliderLeft + (relativeXPos - sliderIndicator.Width));

        }

        else
        {
            double sliderTop = Canvas.GetTop(sliderBackground);
            double sliderHeight = sliderBackground.Height;

            // adjust mouseY by the canvas's top position in its parent to get the relative Y position within the slider
            double relativeYPos = mouseEvent.GetPosition(sliderCanvas).Y - sliderTop;

            // clamp the value to ensure it stays within the slider's height range
            relativeYPos = Math.Max(0, Math.Min(relativeYPos, sliderHeight));

            // calculate the percentage position of the indicator within the slider
            percentage = 1 - (relativeYPos / sliderHeight);

            // adjust the height and position of the indicator based on the calculated percentage
            sliderIndicator.Height = percentage * sliderHeight;
            Canvas.SetTop(sliderIndicator, sliderTop + (sliderHeight - sliderIndicator.Height));
        }


        // use the Tag property to determine which weight to update
        switch (sliderCanvas.Tag.ToString())
        {
            case "team1PrefWeightSliderTag":
                team1PrefWeight = percentage;
                break;
            case "team1WinRateWeightSliderTag":
                team1WinRateWeight = percentage;
                break;
            case "team2WinRateWeightSliderTag":
                team2WinRateWeight = percentage;
                break;
            case "anubisSliderTag":
                team1WinRateDict["Anubis"] = percentage;
                break;
            case "ancientSliderTag":
                team1WinRateDict["Ancient"] = percentage;
                break;
            case "infernoSliderTag":
                team1WinRateDict["Inferno"] = percentage;
                break;
            case "nukeSliderTag":
                team1WinRateDict["Nuke"] = percentage;
                break;
            case "mirageSliderTag":
                team1WinRateDict["Mirage"] = percentage;
                break;
            case "overpassSliderTag":
                team1WinRateDict["Overpass"] = percentage;
                break;
            case "vertigoSliderTag":
                team1WinRateDict["Vertigo"] = percentage;
                break;
        }

    }



    // display 1st pick UI
    private void chooseFirstBanSequence(object sender, RoutedEventArgs e)
    {
        gridFirstBanResults.Visibility = Visibility.Visible;
        gridSecondBanResults.Visibility = Visibility.Hidden;

        firstBanChoice.Foreground = new SolidColorBrush(Colors.Aqua);
        secondBanChoice.Foreground = new SolidColorBrush(Colors.DarkGray);
    }

    // display 2nd pick UI
    private void chooseSecondBanSequence(object sender, RoutedEventArgs e)
    {
        gridFirstBanResults.Visibility = Visibility.Hidden;
        gridSecondBanResults.Visibility = Visibility.Visible;

        secondBanChoice.Foreground = new SolidColorBrush(Colors.Aqua);
        firstBanChoice.Foreground = new SolidColorBrush(Colors.DarkGray);
    }




    // -----------------------------------------------------------------------



    private string firstBan1;
    public string FirstBan1
    {
        get => firstBan1;
        set
        {
            if (firstBan1 != value)
            {
                firstBan1 = value;
                OnPropertyChanged(nameof(firstBan1));
            }
        }
    }

    private string firstBan2;
    public string FirstBan2
    {
        get => firstBan2;
        set
        {
            if (firstBan2 != value)
            {
                firstBan2 = value;
                OnPropertyChanged(nameof(firstBan2));
            }
        }
    }

    private string firstBan3;
    public string FirstBan3
    {
        get => firstBan3;
        set
        {
            if (firstBan3 != value)
            {
                firstBan3 = value;
                OnPropertyChanged(nameof(firstBan3));
            }
        }
    }

    private string firstBan4;
    public string FirstBan4
    {
        get => firstBan4;
        set
        {
            if (firstBan4 != value)
            {
                firstBan4 = value;
                OnPropertyChanged(nameof(firstBan4));
            }
        }
    }

    private string firstBan5;
    public string FirstBan5
    {
        get => firstBan5;
        set
        {
            if (firstBan5 != value)
            {
                firstBan5 = value;
                OnPropertyChanged(nameof(firstBan5));
            }
        }
    }

    private string firstBan6;
    public string FirstBan6
    {
        get => firstBan6;
        set
        {
            if (firstBan6 != value)
            {
                firstBan6 = value;
                OnPropertyChanged(nameof(firstBan6));
            }
        }
    }

    private string firstChoice;
    public string FirstChoice
    {
        get => firstChoice;
        set
        {
            if (firstChoice != value)
            {
                firstChoice = value;
                OnPropertyChanged(nameof(firstChoice));
            }
        }
    }

    private string secondBan1;
    public string SecondBan1
    {
        get => secondBan1;
        set
        {
            if (secondBan1 != value)
            {
                secondBan1 = value;
                OnPropertyChanged(nameof(secondBan1));
            }
        }
    }
    private string secondBan2;
    public string SecondBan2
    {
        get => secondBan2;
        set
        {
            if (secondBan2 != value)
            {
                secondBan2 = value;
                OnPropertyChanged(nameof(secondBan2));
            }
        }
    }

    private string secondBan3;
    public string SecondBan3
    {
        get => secondBan3;
        set
        {
            if (secondBan3 != value)
            {
                secondBan3 = value;
                OnPropertyChanged(nameof(secondBan3));
            }
        }
    }

    private string secondBan4;
    public string SecondBan4
    {
        get => secondBan4;
        set
        {
            if (secondBan4 != value)
            {
                secondBan4 = value;
                OnPropertyChanged(nameof(secondBan4));
            }
        }
    }

    private string secondBan5;
    public string SecondBan5
    {
        get => secondBan5;
        set
        {
            if (secondBan5 != value)
            {
                secondBan5 = value;
                OnPropertyChanged(nameof(secondBan5));
            }
        }
    }

    private string secondBan6;
    public string SecondBan6
    {
        get => secondBan6;
        set
        {
            if (secondBan6 != value)
            {
                secondBan6 = value;
                OnPropertyChanged(nameof(secondBan6));
            }
        }
    }

    private string secondChoice;
    public string SecondChoice
    {
        get => secondChoice;
        set
        {
            if (secondChoice != value)
            {
                secondChoice = value;
                OnPropertyChanged(nameof(secondChoice));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

