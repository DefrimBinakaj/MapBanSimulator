using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace MapBanSimulator.UserControls;




public partial class SeptagonDrag : UserControl
{

    private List<Ellipse> dots;
    private List<Point> vertices;
    private bool isDragging = false;


    public Point SeptagonCenter { get; set; }
    public double SeptagonRadius { get; set; } = 200;
    public double MinimumDistance { get; set; } = 30;
    public double StepDistance { get; set; } = 16;


    // ** order of dict init matters; must go clockwise starting at top
    public Dictionary<string, double?> dotDistances = new Dictionary<string, double?>()
    {
        {"Anubis", null},
        {"Inferno", null},
        {"Mirage", null},
        {"Vertigo", null},
        {"Overpass", null},
        {"Nuke", null},
        {"Ancient", null}
    };

    // ** order of dict init matters; must go clockwise starting at top
    public Dictionary<string, double?> dotPercentages = new Dictionary<string, double?>()
    {
        {"Anubis", 0},
        {"Inferno", 0},
        {"Mirage", 0},
        {"Vertigo", 0},
        {"Overpass", 0},
        {"Nuke", 0},
        {"Ancient", 0}
    };


    public event EventHandler PercentagesUpdated;

    protected virtual void OnPercentagesUpdated()
    {
        PercentagesUpdated?.Invoke(this, EventArgs.Empty);
    }

    public SeptagonDrag()
    {

        InitializeComponent();

        this.SizeChanged += OnSizeChanged;

    }





    // if the window size is changed, adapts the grid design to the current size
    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        CreateConcentricSeptagons();
        RelocateDots();
    }









    // creates the septagon grid pattern
    private void CreateConcentricSeptagons()
    {
        // Remove existing septagons and dots from the canvas
        canvas.Children.Clear();

        // Dynamically derive the center from UserControl's size
        SeptagonCenter = new Point(this.ActualWidth / 2, this.ActualHeight / 2);

        // Add concentric septagons to the canvas
        for (int j = 0; j < 11; j++)
        {
            var currentRadius = SeptagonRadius - j * StepDistance;
            var currentSeptagon = CreateSeptagon(SeptagonCenter.X, SeptagonCenter.Y, currentRadius);
            if (j == 5)
            {
                currentSeptagon.Stroke = new SolidColorBrush(Colors.Black);
            }
            else if (j == 0)
            { 
                currentSeptagon.Stroke = new SolidColorBrush(Colors.Black);
            }
            else if (j == 10)
            {
                currentSeptagon.Stroke = new SolidColorBrush(Colors.Black);
                currentSeptagon.Fill = new SolidColorBrush(Colors.Black);
            }
            else if (j % 2 == 0)
            {
                currentSeptagon.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#948b9d"));
            }
            else
            {
                currentSeptagon.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#b0b0b0"));
            }
            canvas.Children.Add(currentSeptagon);
        }

        // Create dots for the outermost septagon only
        vertices = CreateSeptagon(SeptagonCenter.X, SeptagonCenter.Y, SeptagonRadius).Points.ToList();

    }



    // creates a thin line in the shape of a septagon
    private Polygon CreateSeptagon(double centerX, double centerY, double radius)
    {
        Polygon septagon = new Polygon();
        septagon.Stroke = Brushes.Black;
        septagon.Fill = Brushes.Transparent;
        septagon.StrokeThickness = 2;

        for (int i = 0; i < 7; i++)
        {
            // Start from -90 degrees to have the septagon upright
            double angle = (-90 + i * 51.43) * (Math.PI / 180);  // Convert to radians
            double x = centerX + radius * Math.Cos(angle);
            double y = centerY + radius * Math.Sin(angle);
            septagon.Points.Add(new Point(x, y));
        }

        return septagon;
    }




    private void RelocateDots()
    {
        // create new dots list
        dots = new List<Ellipse>();

        int i = 0;
        foreach (var mapName in dotDistances.Keys)
        {
            var vertex = vertices[i];
            var dot = new Ellipse() { Width = 15, Height = 15, Fill = Brushes.IndianRed };

            if (dotDistances[mapName].HasValue)
            {
                double storedDistance = dotDistances[mapName].Value;
                double theta = (-90 + i * 360.0 / 7) * (Math.PI / 180); // Convert to radians
                Vector direction = new Vector(Math.Cos(theta), Math.Sin(theta));
                var newPos = SeptagonCenter + direction * storedDistance;

                Canvas.SetLeft(dot, newPos.X - dot.Width / 2);
                Canvas.SetTop(dot, newPos.Y - dot.Height / 2);
            }
            else
            {
                PositionDots(dot, i);
            }

            dot.MouseDown += OnMouseDown;
            dot.MouseMove += OnMouseMove;
            dot.MouseUp += OnMouseUp;
            canvas.Children.Add(dot);
            dots.Add(dot);

            i++;
        }
    }



    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        isDragging = true;
        var ellipse = sender as Ellipse;
        ellipse.CaptureMouse();
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        isDragging = false;
        var ellipse = sender as Ellipse;
        ellipse.ReleaseMouseCapture();

        Debug.WriteLine("----\n");
        foreach (var prctg in dotPercentages)
        {
            Debug.WriteLine(prctg.Key + " - " + prctg.Value);
        }
        Debug.WriteLine("trigger!");
        OnPercentagesUpdated();
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            var ellipse = sender as Ellipse;
            var currentPos = e.GetPosition(canvas);
            var index = dots.IndexOf(ellipse);

            // Translate the index to its corresponding map name
            string mapName = dotDistances.Keys.ElementAt(index);

            UpdateDotPosition(ellipse, currentPos, index, mapName);
        }
    }







    private void UpdateDotPosition(Ellipse ellps, Point currentPos, int index, string mapName)
    {
        // Calculate the angle for the current vertex.
        double theta = (-90 + index * 360.0 / 7) * (Math.PI / 180); // Convert to radians

        // Calculate the direction vector using the angle theta
        Vector direction = new Vector(Math.Cos(theta), Math.Sin(theta));

        // Calculate the vector from center to current mouse position
        var toMouse = currentPos - SeptagonCenter;

        // Dot product gives the projection length of 'toMouse' onto 'direction'
        var distance = Vector.Multiply(toMouse, direction);

        // Clamp the distance between MinimumDistance and SeptagonRadius
        distance = Math.Max(MinimumDistance, Math.Min(distance, SeptagonRadius));

        // Save the distance to our dictionary using the map name
        dotDistances[mapName] = distance;

        // Calculate percentage
        double percentage = ((distance - MinimumDistance) / (SeptagonRadius - MinimumDistance));
        dotPercentages[mapName] = Math.Round(percentage, 3); // Round to three decimal places.

        // Calculate new position
        var newPos = SeptagonCenter + direction * distance;

        Canvas.SetLeft(ellps, newPos.X - ellps.Width / 2);
        Canvas.SetTop(ellps, newPos.Y - ellps.Height / 2);

    }



    // position the dots at start/reset
    private void PositionDots(Ellipse dot, int index)
    {
        double angle = (-90 + index * 360.0 / 7) * (Math.PI / 180);
        Vector direction = new Vector(Math.Cos(angle), Math.Sin(angle));
        var startPos = SeptagonCenter + direction * MinimumDistance;

        Canvas.SetLeft(dot, startPos.X - dot.Width / 2);
        Canvas.SetTop(dot, startPos.Y - dot.Height / 2);
    }





    public void refreshDotsButton(object sender, RoutedEventArgs e)
    {

        // Set all distances to MinimumDistance (i.e., the start of the trajectory)
        foreach (var key in dotDistances.Keys.ToList())
        {
            dotDistances[key] = MinimumDistance;
            dotPercentages[key] = 0; // 0% since it's at the start of the trajectory
        }

        // Update positions based on the modified DotDistances
        for (int i = 0; i < dots.Count; i++)
        {
            PositionDots(dots[i], i);
        }
    }







}

