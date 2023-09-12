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

namespace MapBanSimulator;

public partial class MainWindow : Window
{
    private List<string> de_maps;
    private int step;

    public MainWindow()
    {
        InitializeComponent();
        InitBanPhase();
    }

    private void InitBanPhase()
    {
        de_maps = new List<string> { "Ancient", "Anubis", "Inferno", "Mirage", "Vertigo", "Overpass", "Nuke" };
        MapBox.ItemsSource = de_maps;
        step = 1;
        SetInstruction();
    }

    private void SetInstruction()
    {
        switch (step)
        {
            case 1:
                InstructionText.Text = "Team 1: Ban 2 maps";
                MapBox.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;
                break;
            case 2:
                InstructionText.Text = "Team 2: Ban 3 maps";
                MapBox.SelectionMode = System.Windows.Controls.SelectionMode.Multiple;
                break;
            case 3:
                InstructionText.Text = "Team 1: Ban 1 map";
                MapBox.SelectionMode = System.Windows.Controls.SelectionMode.Single;
                break;
        }
    }

    private void ProceedButton_Click(object sender, RoutedEventArgs e)
    {
        List<string> selectedColors = MapBox.SelectedItems.Cast<string>().ToList();

        if (step == 1 && selectedColors.Count == 2)
        {
            de_maps = de_maps.Except(selectedColors).ToList();
            step++;
        }
        else if (step == 2 && selectedColors.Count == 3)
        {
            de_maps = de_maps.Except(selectedColors).ToList();
            step++;
        }
        else if (step == 3 && selectedColors.Count == 1)
        {
            de_maps = de_maps.Except(selectedColors).ToList();
            step++;
            MessageBox.Show($"Team 1 chose the color: {de_maps[0]}", "Game Over");
            // InitBanPhase();
            // return;
        }
        else
        {
            MessageBox.Show("Invalid selection. Please follow the instructions.", "Error");
            return;
        }

        MapBox.ItemsSource = null;
        MapBox.ItemsSource = de_maps;
        SetInstruction();
    }
}
