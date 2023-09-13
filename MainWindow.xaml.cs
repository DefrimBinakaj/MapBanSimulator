using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace MapBanSimulator;

public partial class MainWindow : Window
{

    private Dictionary<string, (Rectangle redOverlay, Image cross, Image check)> mapControls;

    public MainWindow()
    {
        InitializeComponent();
        InitBanPhase();
    }

    private void InitBanPhase()
    {
        mapControls = new Dictionary<string, (Rectangle, Image, Image)>
        {
            {"Anubis", (anubisRedOverlay, anubisCross, anubisCheck)},
            {"Ancient", (ancientRedOverlay, ancientCross, ancientCheck)},
            {"Inferno", (infernoRedOverlay, infernoCross, infernoCheck)},
            {"Mirage", (mirageRedOverlay, mirageCross, mirageCheck)},
            {"Vertigo", (vertigoRedOverlay, vertigoCross, vertigoCheck)},
            {"Overpass", (overpassRedOverlay, overpassCross, overpassCheck)},
            {"Nuke", (nukeRedOverlay, nukeCross, nukeCheck)}
        };

    }

    private void MapClicked(string mapName)
    {

        mapControls[mapName].redOverlay.Visibility = Visibility.Visible;
        mapControls[mapName].cross.Visibility = Visibility.Visible;

        int banCount = mapControls.Count(mpCntrl => mpCntrl.Value.redOverlay.Visibility == Visibility.Visible);

        if (banCount == 6)
        {
            var finalMap = mapControls.Last(mapCntrl => mapCntrl.Value.redOverlay.Visibility == Visibility.Hidden).Key;
            mapControls[finalMap].check.Visibility = Visibility.Visible;
        }
    }



    private void anubisClicked(object sender, RoutedEventArgs e) => MapClicked("Anubis");
    private void ancientClicked(object sender, RoutedEventArgs e) => MapClicked("Ancient");
    private void infernoClicked(object sender, RoutedEventArgs e) => MapClicked("Inferno");
    private void nukeClicked(object sender, RoutedEventArgs e) => MapClicked("Nuke");
    private void mirageClicked(object sender, RoutedEventArgs e) => MapClicked("Mirage");
    private void overpassClicked(object sender, RoutedEventArgs e) => MapClicked("Overpass");
    private void vertigoClicked(object sender, RoutedEventArgs e) => MapClicked("Vertigo");

    private void refreshClicked(object sender, RoutedEventArgs e)
    {
        foreach (var control in mapControls)
        {
            control.Value.redOverlay.Visibility = Visibility.Hidden;
            control.Value.cross.Visibility = Visibility.Hidden;
            control.Value.check.Visibility = Visibility.Hidden;
        }
    }
}

