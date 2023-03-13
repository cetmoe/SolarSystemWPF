using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SpaceSim
{
    public class SpaceObject
    {
        public string Name { get; set; }

        public int OrbitalRadius { get; set; }
        public double OrbitalPeriod { get; set; }
        public double Radius { get; set; }
        public Ellipse? Visual { get; set; }
        public TextBlock? Text { get; set; }
        public Boolean ShowText = true;
        public double Scalar = 15;

        public SpaceObject(string name)
        {
            this.Name = name;
        }

        public virtual string GetInformation()
        {
            object[] args = { Name, OrbitalRadius, OrbitalPeriod };
            string output = String.Format("{0, 10}, orbital radius: {1,8}, orbital period: {2,8},", args);
            return output;
        }

        public virtual void Draw(double time)
        {
            Point pos = GetPosition(time);
            object[] args = { pos.X, pos.Y };
            Console.WriteLine(GetInformation() + String.Format(" x: {0,10:.00} y: {1,10:.00} ", args));
        }

        public virtual Point GetPosition(double time)
        {
            double angle = 2 * Math.PI * time / OrbitalPeriod;
            double x = OrbitalRadius * Math.Cos(angle);
            double y = OrbitalRadius * Math.Sin(angle);
            return new(x, y);
        }

        public virtual Point GetScaledPosition(double time)
        {
            double angle = 2 * Math.PI * time / OrbitalPeriod;
            double x = Math.Pow(Math.Log10(OrbitalRadius), 2) * Math.Cos(angle) * Scalar;
            double y = Math.Pow(Math.Log10(OrbitalRadius), 2) * Math.Sin(angle) * Scalar;
            return new(x, y);
        }

        public virtual Boolean WithinBounds(Canvas canvas, Point click, double time)
        {
            Point pos = GetScaledPosition(time);
            double objectY = canvas.RenderSize.Height / 2 + pos.Y;
            double objectX = canvas.RenderSize.Width / 2 + pos.X;

            double objectHeight = Visual.ActualHeight / 2;
            double objectWidth = Visual.ActualWidth / 2;

            return (click.X < objectX + objectWidth)
                && (click.X > objectX - objectWidth)
                && (click.Y < objectY + objectHeight)
                && (click.Y > objectY - objectHeight);
        }
    }

    public class SpaceObjectRef : SpaceObject
    {
        public SpaceObject? ReferencePoint { get; set; }

        public SpaceObjectRef(string name) : base(name) { }

        public override string GetInformation()
        {
            return base.GetInformation()
                + String.Format(" reference point: {0,10},", ReferencePoint.Name);
        }

        public override Point GetScaledPosition(double time)
        {
            double angle = 2 * Math.PI * time / OrbitalPeriod;
            double x = Math.Pow(Math.Log10(OrbitalRadius), 2) * Math.Cos(angle) * Scalar;
            double y = Math.Pow(Math.Log10(OrbitalRadius), 2) * Math.Sin(angle) * Scalar;

            if (ReferencePoint != null)
            {
                Point anchor = ReferencePoint.GetScaledPosition(time);
                return new(anchor.X + x, anchor.Y + y);
            }
            return new(x, y);
        }
    }

    public class Star : SpaceObject
    {
        public Star(string name) : base(name) { }

        public override string GetInformation()
        {
            return "Star: ".PadLeft(18) + base.GetInformation();
        }

        public override Point GetPosition(double time)
        {
            return new(300, 300);
        }

        public override Point GetScaledPosition(double time)
        {
            return new(0, 0);
        }
    }

    public class Planet : SpaceObjectRef
    {
        public Planet(string name) : base(name) { }

        public override string GetInformation()
        {
            return "Planet: ".PadLeft(18) + base.GetInformation();
        }
    }

    public class DwarfPlanet : SpaceObjectRef
    {
        public DwarfPlanet(string name) : base(name) { }

        public override string GetInformation()
        {
            return "Dwarf Planet: ".PadLeft(18) + base.GetInformation();
        }
    }

    public class Moon : SpaceObjectRef
    {
        public Moon(string name) : base(name)
        {
            ShowText = false;
            Scalar = 5;
        }
        public override string GetInformation()
        {
            return "Moon: ".PadLeft(18) + base.GetInformation();
        }
    }

    public class Comet : SpaceObject
    {
        public Comet(string name) : base(name) { }

        public override string GetInformation()
        {
            return "Comet: ".PadLeft(18) + base.GetInformation();
        }
    }

    public class Asteroid : SpaceObject
    {
        public Asteroid(string name) : base(name) { }

        public override string GetInformation()
        {
            return "Asteroid: ".PadLeft(18) + base.GetInformation();
        }
    }

    public class AsteroidBelt : SpaceObject
    {
        public AsteroidBelt(string name) : base(name) { }

        public override string GetInformation()
        {
            return "Asteroid Belt: ".PadLeft(18) + base.GetInformation();
        }
    }

    public class SpaceSystem
    {

        public List<SpaceObject> SpaceObjects { get; set; }
        public double MaxRadius { get; set; }
        public double time = 0;
        public SpaceSystem(Stream stream)
        {
            SpaceObjects = new List<SpaceObject>();

            using (StreamReader sr = new(stream))
            {
                string s;
                while ((s = sr.ReadLine()) != null)
                {
                    String[] vars = s.Split(',');

                    dynamic tempObject = null;

                    // find reference
                    string tempName = vars[0];
                    string objectType = vars[1];
                    int tempOrbitalRadius = int.Parse(vars[3]);
                    double tempPeriod;
                    double.TryParse(vars[4], out tempPeriod);
                    double tempRadius = double.Parse(vars[5]);
                    SpaceObject tempReference = SpaceObjects.Find(obj => obj.Name == vars[2]);

                    switch (objectType)
                    {
                        case ("star"):
                            tempObject = new Star(tempName);
                            tempObject.Radius = 120;
                            break;
                        case ("planet"):
                            tempObject = new Planet(tempName);
                            tempObject.ReferencePoint = tempReference;
                            tempObject.Radius = 30;
                            break;
                        case ("dwarfplanet"):
                            tempObject = new DwarfPlanet(tempName);
                            tempObject.ReferencePoint = tempReference;
                            tempObject.Radius = 15;
                            break;
                        case ("moon"):
                            tempObject = new Moon(tempName);
                            tempObject.ReferencePoint = tempReference;
                            tempObject.Radius = 7.5;
                            break;
                    }

                    if (tempObject != null)
                    {
                        tempObject.OrbitalRadius = tempOrbitalRadius;
                        tempObject.OrbitalPeriod = tempPeriod;
                        tempObject.Radius = Math.Pow(Math.Log10(tempRadius), 2);
                        SpaceObjects.Add(tempObject);
                    }
                }
            }
        }

        public void SetupDrawable(Canvas canvas)
        {
            foreach (SpaceObject obj in SpaceObjects)
            {
                obj.Visual = new Ellipse();
                obj.Visual.Width = obj.Visual.Height = obj.Radius;
                obj.Visual.Fill = new SolidColorBrush(Colors.Red);
                canvas.Children.Add(obj.Visual);
                if (obj.ShowText)
                {
                    obj.Text = new TextBlock();
                    obj.Text.Text = obj.Name;
                    obj.Text.Foreground = new SolidColorBrush(Colors.White);
                    obj.Text.FontSize = 12;
                    canvas.Children.Add(obj.Text);
                }
            }
        }

        public void Render(Canvas canvas)
        {
            foreach (SpaceObject obj in SpaceObjects)
            {
                Point pos = obj.GetScaledPosition(time);

                double posY = canvas.RenderSize.Height / 2 + pos.Y;
                double posX = canvas.RenderSize.Width / 2 + pos.X;

                Canvas.SetTop(obj.Visual, posY - obj.Visual.ActualHeight / 2);
                Canvas.SetLeft(obj.Visual, posX - obj.Visual.ActualWidth / 2);
                if (obj.ShowText)
                {
                    Canvas.SetTop(obj.Text, posY - obj.Text.ActualHeight / 2 - obj.Visual.ActualHeight / 2 - 10);
                    Canvas.SetLeft(obj.Text, posX - obj.Text.ActualWidth / 2);
                }
            }
        }

        public void Advance(double time)
        {
            this.time += time;
        }

        public void ComputeMaxRadius()
        {
            double radius = 0;
            foreach (SpaceObject obj in SpaceObjects)
            {
                if (obj.OrbitalRadius > radius)
                {
                    radius = obj.OrbitalRadius;
                }
            }
            MaxRadius = Math.Log10(radius);
        }
    }
}