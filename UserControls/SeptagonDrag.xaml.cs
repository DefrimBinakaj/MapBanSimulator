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

    private Polygon septagon;

    public Point SeptagonCenter { get; set; }
    public double SeptagonRadius { get; set; } = 200;
    public double MinimumDistance { get; set; } = 30;

    private List<Ellipse> dots;
    private List<Point> vertices;
    private bool isDragging = false;

    public Dictionary<string, double?> dotDistances;

    public Dictionary<string, double?> dotPercentages;

    public double StepDistance { get; set; } = 16;

    public SeptagonDrag()
    {

        InitializeComponent();

        // ** order of dict init matters; must go clockwise starting at top
        dotDistances = new Dictionary<string, double?>()
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
        dotPercentages = new Dictionary<string, double?>()
        {
            {"Anubis", 0},
            {"Inferno", 0},
            {"Mirage", 0},
            {"Vertigo", 0},
            {"Overpass", 0},
            {"Nuke", 0},
            {"Ancient", 0}
        };


        this.SizeChanged += OnSizeChanged;

    }


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





    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {

        // Remove all old dots
        if (dots != null)
        {
            foreach (var oldDot in dots)
            {
                canvas.Children.Remove(oldDot);
            }
            dots.Clear();
        }
        else
        {
            dots = new List<Ellipse>();
        }

        CreateConcentricSeptagons();

        // Dynamically derive the center from UserControl's size
        SeptagonCenter = new Point(this.ActualWidth / 2, this.ActualHeight / 2);

        // Add septagon to canvas
        septagon = CreateSeptagon(SeptagonCenter.X, SeptagonCenter.Y, SeptagonRadius);
        canvas.Children.Add(septagon);

        vertices = septagon.Points.ToList();

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
                Canvas.SetLeft(dot, vertex.X - dot.Width / 2);
                Canvas.SetTop(dot, vertex.Y - dot.Height / 2);
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

    }

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
    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging)
        {
            var ellipse = sender as Ellipse;
            var currentPos = e.GetPosition(canvas);
            var index = dots.IndexOf(ellipse);

            // Translate the index to its corresponding map name
            string mapName = dotDistances.Keys.ElementAt(index);

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
            double percentage = ((distance - MinimumDistance) / (SeptagonRadius - MinimumDistance)) * 100;
            dotPercentages[mapName] = Math.Round(percentage, 2); // Round to two decimal places.

            // Calculate new position
            var newPos = SeptagonCenter + direction * distance;

            Canvas.SetLeft(ellipse, newPos.X - ellipse.Width / 2);
            Canvas.SetTop(ellipse, newPos.Y - ellipse.Height / 2);

        }
    }


    public void checkDotVals(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("checkDotVals:");
        Debug.WriteLine(string.Join(" ", dotPercentages));
    }


    public void refreshDotsButton(object sender, RoutedEventArgs e)
    {

        // Set all distances to SeptagonRadius (i.e., the outside)
        foreach (var key in dotDistances.Keys.ToList())
        {
            dotDistances[key] = MinimumDistance;
            dotPercentages[key] = 0;
        }

        // Ensure the dots list is initialized
        if (dots == null)
            return;

        // Update positions based on the modified dotDistances
        for (int i = 0; i < dots.Count; i++)
        {
            var dot = dots[i];
            var mapName = dotDistances.Keys.ElementAt(i);
            var vertex = vertices[i];

            if (dotDistances[mapName].HasValue)
            {
                double storedDistance = dotDistances[mapName].Value;
                double theta = (-90 + i * 360.0 / 7) * (Math.PI / 180);
                Vector direction = new Vector(Math.Cos(theta), Math.Sin(theta));
                var newPos = SeptagonCenter + direction * storedDistance;

                Canvas.SetLeft(dot, newPos.X - dot.Width / 2);
                Canvas.SetTop(dot, newPos.Y - dot.Height / 2);
            }
            else
            {
                Canvas.SetLeft(dot, vertex.X - dot.Width / 2);
                Canvas.SetTop(dot, vertex.Y - dot.Height / 2);
            }

            // Ensure the event handlers are attached
            dot.MouseDown -= OnMouseDown;  // Remove first to avoid double subscription
            dot.MouseDown += OnMouseDown;
            dot.MouseMove -= OnMouseMove;
            dot.MouseMove += OnMouseMove;
            dot.MouseUp -= OnMouseUp;
            dot.MouseUp += OnMouseUp;
        }
    }


    public Dictionary<string, double?> DotPercentages
    {
        get => dotPercentages;
        set
        {
            dotPercentages = value;
            OnPropertyChanged(nameof(DotPercentages));
        }
    }



    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }




}

