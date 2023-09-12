using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Diagnostics;

namespace MapBanSimulator;

public partial class MainWindow : Window
{

    // dict for checking if maps are banned
    private Dictionary<string, Visibility> mapBanStatus;

    private List<string> de_maps;
    private int step;

    public MainWindow()
    {
        InitializeComponent();
        InitBanPhase();
    }

    private void InitBanPhase()
    {

        mapBanStatus = new Dictionary<string, Visibility>
        {
            {"Ancient", ancientCheck.Visibility },
            {"Anubis", anubisCheck.Visibility },
            {"Inferno", infernoCheck.Visibility },
            {"Mirage", mirageCheck.Visibility },
            {"Vertigo", vertigoCheck.Visibility },
            {"Overpass", overpassCheck.Visibility },
            {"Nuke", nukeCheck.Visibility }
        };

    }


    private void mapSelection(string clickedMap)
    {

        mapBanStatus[clickedMap] = Visibility.Visible;

        int banCount = 0;
        string finalMap = "";

        foreach (KeyValuePair<string, Visibility> keyMap in mapBanStatus)
        {
            // Debug.WriteLine("\n\nHERE--- " +  keyMap.Key.ToString() + " " + keyMap.Value.ToString());
            if (keyMap.Key != clickedMap && keyMap.Value == Visibility.Visible)
            {
                banCount++;
            }
        }

        if (banCount == 5)
        {
            foreach (KeyValuePair<string, Visibility> keyMap in mapBanStatus)
            {
                if (keyMap.Value == Visibility.Hidden)
                {
                    finalMap = keyMap.Key;
                }
            }
            MessageBox.Show($"Team 1 chose {finalMap}", "Game Over");
        }

    }



    private void anubisClicked(object sender, RoutedEventArgs e)
    {
        anubisCheck.Visibility = Visibility.Visible;
        mapSelection("Anubis");
    }

    private void ancientClicked(object sender, RoutedEventArgs e)
    {
        ancientCheck.Visibility = Visibility.Visible;
        mapSelection("Ancient");
    }

    private void infernoClicked(object sender, RoutedEventArgs e)
    {
        infernoCheck.Visibility = Visibility.Visible;
        mapSelection("Inferno");
    }

    private void nukeClicked(object sender, RoutedEventArgs e)
    {
        nukeCheck.Visibility = Visibility.Visible;
        mapSelection("Nuke");
    }

    private void mirageClicked(object sender, RoutedEventArgs e)
    {
        mirageCheck.Visibility = Visibility.Visible;
        mapSelection("Mirage");
    }

    private void overpassClicked(object sender, RoutedEventArgs e)
    {
        overpassCheck.Visibility = Visibility.Visible;
        mapSelection("Overpass");
    }

    private void vertigoClicked(object sender, RoutedEventArgs e)
    {
        vertigoCheck.Visibility = Visibility.Visible;
        mapSelection("Vertigo");
    }


}
