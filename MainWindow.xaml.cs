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
using System.IO;
namespace hungaryTDv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Rectangle background;
        Button tempTwrBtn;
        Rectangle tempRect;
        Rectangle healthBar = new Rectangle();
        Rectangle damageBar = new Rectangle();
        Label lblHealth = new Label();
        Label lblMoney = new Label();
        Button[] towerIcons = new Button[4];
        Button btnStart = new Button();
        ImageBrush[] towerFill = new ImageBrush[4];
        System.Windows.Threading.DispatcherTimer gameTimer = new System.Windows.Threading.DispatcherTimer();
        GameState gameState;
        enum GameState { play, store, test };
        enum TowerType { normal, police, family, tank }
        enum EnemyType { apple, pizza, donut, hamburger, fries }
        List<Enemy> enemies = new List<Enemy>();
        Polygon trackHit = new Polygon();
        Point[] track = new Point[1450];
        int[] positions = new int[1450];
        StreamReader sr;
        int tempTowerType;
        int tempCost;
        int money = 300;
        List<Tower> towers = new List<Tower>();
        int[][] waves = new int[10][];
        string[] levelMessages = new string[10];
        public MainWindow()
        {
            InitializeComponent();
            btnStart.Height = 20;
            btnStart.Width = 70;
            btnStart.Content = "start";
            btnStart.Click += BtnStart_Click;
            cBackground.Children.Add(btnStart);
            for (int i = 0; i < positions.Length; i++)
            {
                positions[i] = -1;
            }
            sr = new StreamReader("trackLine.txt");
            int counter = 0;
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                double xPosition, yPosition;
                double.TryParse(currentLine.Split(',')[0], out xPosition);
                double.TryParse(currentLine.Split(',')[1], out yPosition);
                Point point = new Point(xPosition, yPosition);
                track[counter] = point;
                counter++;
            }
            sr.Close();
            gameState = GameState.play;
            counter = 0;
            sr = new StreamReader("levels.txt");
            while (!sr.EndOfStream)
            {
                string currentLine;
                currentLine = sr.ReadLine();
                try
                {
                    string[] lineSplit = currentLine.Split();
                    for(int i = 0; i < lineSplit.Length; i++)
                    {
                        int.TryParse(lineSplit[i], out waves[counter/2][i]);
                    }
                }
                catch
                {
                    levelMessages[counter / 2] = currentLine;
                }
                counter++;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (gameState == GameState.store)
            {
                Canvas.SetTop(tempRect, Mouse.GetPosition(cBackground).Y - tempRect.Height / 2);
                Canvas.SetLeft(tempRect, Mouse.GetPosition(cBackground).X - tempRect.Width / 2);
                bool valid = true;
                double x = Mouse.GetPosition(cBackground).X;
                double y = Mouse.GetPosition(cBackground).Y;
                bool check1 = cObstacles.InputHitTest(new Point(x + tempRect.Width / 2 - 5, y + tempRect.Height / 2 - 5)) == null;
                bool check2 = cObstacles.InputHitTest(new Point(x - tempRect.Width / 2 + 5, y + tempRect.Height / 2 - 5)) == null;
                bool check3 = cObstacles.InputHitTest(new Point(x + tempRect.Width / 2 - 5, y - tempRect.Height / 2 + 5)) == null;
                bool check4 = cObstacles.InputHitTest(new Point(x - tempRect.Width / 2 + 5, y - tempRect.Height / 2 + 5)) == null;
                bool check5 = cObstacles.InputHitTest(new Point(x, y)) == null;
                bool check6 = tempCost <= money;
                if (check1 && check2 && check3 && check4 && check5 && check6)
                {
                    valid = true;
                    tempRect.Stroke = Brushes.Transparent;
                }
                else
                {
                    valid = false;
                    tempRect.Stroke = Brushes.Red;
                    tempRect.StrokeThickness = 5;
                }
                MouseButtonState pmbs = MouseButtonState.Released;
                if (Mouse.LeftButton == MouseButtonState.Pressed)
                {
                    if (valid)
                    {
                        Point temp = Mouse.GetPosition(cBackground);
                        cBackground.Children.Remove(tempRect);
                        Tower newTower = new Tower(tempTowerType, cBackground, cObstacles, positions, track, temp, cEnemies);
                        towers.Add(newTower);
                        money -= tempCost;
                        lblMoney.Content = "$ " + money;
                    }
                    else
                    {
                        cBackground.Children.Remove(tempRect);
                    }
                    cObstacles.Children.Remove(trackHit);
                    gameState = GameState.play;
                }
                else
                {
                    pmbs = Mouse.LeftButton;
                }
            }
            else if (gameState == GameState.test)
            {
            }
            else if (gameState == GameState.play)
            {
                if (enemies.Count == 0)
                {
                }
                for (int i = enemies.Count - 1; i > -1; i--)
                {
                    int tempDamage = enemies[i].update(i);
                    if (tempDamage > 0)
                    {
                        damageBar.Width += tempDamage;
                        Canvas.SetLeft(damageBar, 825 - damageBar.Width);
                        enemies.RemoveAt(i);
                        if (damageBar.Width >= 250)
                        {
                            MessageBox.Show("Game Over");
                            Close();
                        }
                    }
                }
                for (int i = 0; i < towers.Count; i++)
                {
                    List<Shape> hits = towers[i].Shoot();
                    if (hits != null && hits.Count > 0)
                    {
                        for (int x = 0; x < hits.Count; x++)
                        {
                            for (int y = 0; y < enemies.Count; y++)
                            {
                                if (enemies[y].sprite == hits[x])
                                {
                                    enemies[y].health -= towers[i].damage;
                                    if (enemies[y].health <= 0)
                                    {
                                        money += enemies[y].reward;
                                        lblMoney.Content = "$ " + money;
                                        cEnemies.Children.Remove(enemies[y].sprite);
                                        for (int a = 0; a < positions.Length; a++)
                                        {
                                            if (positions[a] == y)
                                            {
                                                positions[a] = -1;
                                            }
                                        }
                                        enemies.RemoveAt(y);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Interval = new TimeSpan(0, 0, 0, 0, 1000 / 60);
            gameTimer.Start();

            sr = new StreamReader("trackBox.txt");
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
            trackHit.Points = myPointCollection;
            trackHit.Fill = Brushes.Transparent;

            cBackground.Children.Remove(btnStart);
            background = new Rectangle();
            background.Height = 650;
            background.Width = 1125;
            BitmapImage bi = new BitmapImage(new Uri("track.png", UriKind.Relative));
            ImageBrush img = new ImageBrush(bi);
            background.Fill = img;
            cBackground.Children.Add(background);

            tempTwrBtn = new Button();
            tempTwrBtn.Height = 20;
            tempTwrBtn.Width = 40;
            tempTwrBtn.Content = "test";
            tempTwrBtn.Click += TempTwrBtn_Click;
            Canvas.SetTop(tempTwrBtn, 17);
            Canvas.SetLeft(tempTwrBtn, 1000);
            cBackground.Children.Add(tempTwrBtn);

            bi = new BitmapImage(new Uri("normal.png", UriKind.Relative));
            towerFill[0] = new ImageBrush(bi);
            bi = new BitmapImage(new Uri("police.png", UriKind.Relative));
            towerFill[1] = new ImageBrush(bi);
            bi = new BitmapImage(new Uri("family.png", UriKind.Relative));
            towerFill[2] = new ImageBrush(bi);
            bi = new BitmapImage(new Uri("tank.png", UriKind.Relative));
            towerFill[3] = new ImageBrush(bi);
            for (int i = 0; i < towerIcons.Length; i++)
            {
                towerIcons[i] = new Button();
                towerIcons[i].Background = towerFill[i];
                towerIcons[i].Height = 80;
                towerIcons[i].Width = 80;
                towerIcons[i].Click += iconsClick;
                towerIcons[i].BorderBrush = Brushes.Transparent;
                Canvas.SetTop(towerIcons[i], i * 150 + 60);
                Canvas.SetLeft(towerIcons[i], 910);
                cBackground.Children.Add(towerIcons[i]);
            }
            gameState = GameState.play;

            healthBar.Height = 25;
            healthBar.Width = 250;
            healthBar.Fill = Brushes.Green;
            healthBar.Stroke = Brushes.Black;
            Canvas.SetTop(healthBar, 85);
            Canvas.SetLeft(healthBar, 575);
            cBackground.Children.Add(healthBar);

            damageBar.Height = 25;
            damageBar.Width = 0;
            damageBar.Fill = Brushes.DarkRed;
            damageBar.Stroke = Brushes.Black;
            Canvas.SetTop(damageBar, 85);
            Canvas.SetLeft(damageBar, 575);
            cBackground.Children.Add(damageBar);

            lblHealth.Foreground = Brushes.Black;
            lblHealth.Content = "Health";
            lblHealth.FontWeight = FontWeights.UltraBold;
            lblHealth.FontFamily = new FontFamily("Consola");
            lblHealth.FontSize = 20;
            lblHealth.Height = 50;
            lblHealth.Width = 250;
            Canvas.SetTop(lblHealth, 80);
            Canvas.SetLeft(lblHealth, 575);
            cBackground.Children.Add(lblHealth);

            lblMoney.Foreground = Brushes.Gold;
            lblMoney.Content = "Health";
            lblMoney.FontWeight = FontWeights.UltraBold;
            lblMoney.FontFamily = new FontFamily("Consola");
            lblMoney.FontSize = 30;
            lblMoney.Height = 50;
            lblMoney.Width = 100;
            Canvas.SetTop(lblMoney, 10);
            Canvas.SetLeft(lblMoney, 875);
            lblMoney.Content = "$ " + money;
            cBackground.Children.Add(lblMoney);

            enemies.Add(new Enemy((int)EnemyType.hamburger, cEnemies, cBackground, track, positions));
            enemies.Add(new Enemy((int)EnemyType.apple, cEnemies, cBackground, track, positions));
            enemies.Add(new Enemy((int)EnemyType.pizza, cEnemies, cBackground, track, positions));
            enemies.Add(new Enemy((int)EnemyType.pizza, cEnemies, cBackground, track, positions));
            enemies.Add(new Enemy((int)EnemyType.pizza, cEnemies, cBackground, track, positions));
        }

        private void TempTwrBtn_Click(object sender, RoutedEventArgs e)
        {
            gameState = GameState.test;
        }
        private void iconsClick(object sender, RoutedEventArgs e)
        {
            //sw.Close();
            gameState = GameState.store;
            cObstacles.Children.Add(trackHit);
            Button button = sender as Button;
            for (int i = 0; i < towerIcons.Length; i++)
            {
                if (towerIcons[i] == button)
                {
                    tempTowerType = i;
                }
            }
            tempRect = new Rectangle();
            tempRect.Fill = towerFill[tempTowerType];
            if (tempTowerType < 2)
            {
                tempRect.Height = 35;
                tempRect.Width = 35;
                tempCost = 100;
            }
            else if (tempTowerType == 2)
            {
                tempRect.Height = 45;
                tempRect.Width = 70;
                tempCost = 100;
            }
            else
            {
                tempRect.Height = 70;
                tempRect.Width = 70;
                tempCost = 100;
            }
            cBackground.Children.Add(tempRect);
        }
    }
}
