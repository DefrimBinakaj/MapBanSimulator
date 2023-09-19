using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MapBanSimulator.Models;

public static class ConstantSets
{

    public static Dictionary<int, Color> rankColourTable = new Dictionary<int, Color>
    {
        {1, Colors.LawnGreen },
        {2, Colors.GreenYellow },
        {3, Colors.GreenYellow },
        {4, Colors.LightGreen },
        {5, Colors.Yellow },
        {6, Colors.Orange },
        {7, Colors.OrangeRed }
    };


    public static Dictionary<string, double> MapPublicPlayRate = new Dictionary<string, double>
        {
            {"Anubis", 4.07},
            {"Ancient", 8.27},
            {"Inferno", 21.44},
            {"Mirage", 27.12},
            {"Vertigo", 10.73},
            {"Overpass", 15.5},
            {"Nuke", 12.86}
        };


}
