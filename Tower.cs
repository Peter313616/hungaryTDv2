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
using System.Drawing;
using System.Media;
using System.IO;

namespace hungaryTDv2
{
    public class Tower
    {
        public Point Location;
        int towerType;
        Rectangle towerRect;
        Rectangle bullet = new Rectangle();
        Polygon hitBox = new Polygon();
        Canvas cBackground;
        Canvas cObstacles;
        Canvas cEnemies;
        int[] positions;
        Point[] track;
        List<int> targets = new List<int>();
        List<Shape> hits = new List<Shape>();
        int range;
        int cost;
        int bSpeed = 10;
        public int damage;
        public bool shooting = false;
        double xMove;
        double yMove;
        BitmapImage bi;
        double totalMove;
        Rectangle[] famBullets;
        Polygon[] famHitboxes;
        public Tower(int tT, Canvas cBack, Canvas cO, int[] p, Point[] t, Point l, Canvas cE)
        {
            towerType = tT;
            towerRect = new Rectangle();
            cBackground = cBack;
            cObstacles = cO;
            cEnemies = cE;
            positions = p;
            track = t;
            Location = l;
            if (towerType == 0)//norm
            {
                range = 100;
                damage = 100;
                cost = 100;

                bi = new BitmapImage(new Uri("normal.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 35;
                towerRect.Width = 35;

            }
            else if (towerType == 1)//popo
            {
                range = 100;
                damage = 100;
                cost = 100;

                bi = new BitmapImage(new Uri("police.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 35;
                towerRect.Width = 35;

            }
            else if (towerType == 2)//fam
            {
                range = 100;
                damage = 100;
                cost = 100;

                bi = new BitmapImage(new Uri("family.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 45;
                towerRect.Width = 70;
            }
            else//thicc
            {
                range = 100;
                damage = 100;
                cost = 100;

                bi = new BitmapImage(new Uri("tank.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 70;
                towerRect.Width = 70;
            }

            Canvas.SetTop(towerRect, Location.Y - towerRect.Height / 2);
            Canvas.SetLeft(towerRect, Location.X - towerRect.Width / 2);
            cObstacles.Children.Add(towerRect);
            cBackground.Children.Remove(cObstacles);
            cBackground.Children.Add(cObstacles);

            StreamReader sr = new StreamReader("forkBox.txt");
            PointCollection myPointCollection = new PointCollection();
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                double xPosition, yPosition;
                double.TryParse(currentLine.Split(',')[0], out xPosition);
                double.TryParse(currentLine.Split(',')[1], out yPosition);
                Point point = new Point(xPosition, yPosition);
                myPointCollection.Add(point);
            }
            sr.Close();
            MessageBox.Show(myPointCollection.Count.ToString());
            hitBox.Points = myPointCollection;
            hitBox.Fill = Brushes.Transparent;

            bullet = new Rectangle();
            bullet.Height = 20;
            bullet.Width = 10;
            bi = new BitmapImage(new Uri("fork.png", UriKind.Relative));
            bullet.Fill = new ImageBrush(bi);

            for (int i = 0; i < positions.Length; i++)
            {
                double xDistance = xDistance = track[i].X - Location.X;
                double yDistance = yDistance = Location.Y - track[i].Y; ;
                double TotalDistance = Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
                if (TotalDistance < range)
                {
                    targets.Add(i);
                }
            }
        }
        public List<Shape> Shoot()
        {
            Point target;
            if (shooting)
            {
                Canvas.SetLeft(bullet, Canvas.GetLeft(bullet) + xMove);
                Canvas.SetTop(bullet, Canvas.GetTop(bullet) - yMove);
                Canvas.SetLeft(hitBox, Canvas.GetLeft(hitBox) + xMove);
                Canvas.SetTop(hitBox, Canvas.GetTop(hitBox) - yMove);
                totalMove += Math.Sqrt(Math.Pow(xMove, 2) + Math.Pow(yMove, 2));
                for (int i = 0; i < hitBox.Points.Count; i++)
                {
                    Point screenPoint = hitBox.PointToScreen(hitBox.Points[i]);
                    Point canvasPoint = cBackground.PointFromScreen(screenPoint);
                    if (cEnemies.InputHitTest(canvasPoint) != null && !hits.Contains(cEnemies.InputHitTest(canvasPoint)))
                    {
                        Rectangle test = (Rectangle)cEnemies.InputHitTest(canvasPoint);
                        hits.Add(test);
                    }
                }
                if (totalMove >= range)
                {
                    cBackground.Children.Remove(bullet);
                    cBackground.Children.Remove(hitBox);
                    shooting = false;
                    totalMove = 0;
                    xMove = 0;
                    yMove = 0;
                    return hits;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                hits = new List<Shape>();
                for (int i = 0; i < targets.Count; i++)
                {
                    if (positions[targets[targets.Count - 1 - i]] != -1)
                    {
                        if (towerType != 2)
                        {
                            target = track[targets[targets.Count - 1 - i]];
                            double xDistance = target.X - Location.X;
                            double yDistance = Location.Y - target.Y;
                            xDistance = target.X - Location.X;
                            yDistance = Location.Y - target.Y;

                            double TotalDistance = Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
                            double NumbOfTransforms = Math.Ceiling(TotalDistance / bSpeed);
                            xMove = xDistance / NumbOfTransforms;
                            yMove = yDistance / NumbOfTransforms;

                            double temp = Math.Atan(xDistance / yDistance);
                            double angle = temp * 180 / Math.PI;

                            if (target.Y > Location.Y)
                            {
                                angle += 180;
                            }
                            RotateTransform rotate = new RotateTransform(angle);
                            bullet.RenderTransformOrigin = new Point(0.5, 0.5);
                            bullet.RenderTransform = rotate;
                            Canvas.SetLeft(bullet, Location.X);
                            Canvas.SetTop(bullet, Location.Y);
                            hitBox.RenderTransformOrigin = new Point(0.5, 0.5);
                            hitBox.RenderTransform = rotate;
                            Canvas.SetLeft(hitBox, Location.X);
                            Canvas.SetTop(hitBox, Location.Y);
                            cBackground.Children.Add(bullet);
                            cBackground.Children.Add(hitBox);
                            shooting = true;
                            break;
                        }
                        else
                        {
                            for (int x = 0; x < 8; i++)
                            {
                                famBullets[x] = new Rectangle();
                                famBullets[x].Fill = bullet.Fill;
                                famBullets[x].Height = bullet.Height;
                                famBullets[x].Width = bullet.Width;
                                RotateTransform rotate = new RotateTransform(x * 45);
                                bullet.RenderTransformOrigin = new Point(0.5, 0.5);
                                bullet.RenderTransform = rotate;
                                int xDirection;
                                int yDirection;
                                if (x > 0 && x < 4)
                                {
                                    xDirection = 1;
                                }
                                else if (x == 0 || x == 4)
                                {
                                    xDirection = 0;
                                }
                                else
                                {
                                    xDirection = -1;
                                }
                                if (x < 2 || x > 6)
                                {
                                    yDirection = 1;
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }
    }
}
