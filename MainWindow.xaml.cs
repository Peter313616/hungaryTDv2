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
        public Rectangle background;
        public Label lblMouseTest;
        public Button tempTwrBtn;
        public Rectangle tempRect;
        //
        public Ellipse tempCirc;
        //
        public Rectangle healthBar = new Rectangle();
        public Rectangle damageBar = new Rectangle();
        public Label lblHealth = new Label();
        public Label lblMoney = new Label();
        public bool mouseTest;
        public Button[] towerIcons = new Button[4];
        public Button btnStart = new Button();
        public ImageBrush[] towerFill = new ImageBrush[4];
        public System.Windows.Threading.DispatcherTimer gameTimer = new System.Windows.Threading.DispatcherTimer();
        public GameState gameState;
        public enum GameState { play, store, pause };
        public TowerType towerType;
        public enum TowerType { normal, police, family, tank }
        public EnemyType enemyType;
        public enum EnemyType { apple, pizza, donut, hamburger, fries }
        public List<Enemy> enemies = new List<Enemy>();
        public Polygon trackHit = new Polygon();
        public Point[] track = new Point[1450];
        public int[] positions = new int[1450];
        public StreamWriter sw;
        public StreamReader sr;
        public int tempTowerType;
        public int tempCost;
        public int money = 300;
        public List<Tower> towers = new List<Tower>();
        public int level = 0;
        public int[][] waves = new int[10][];
        public string[] levelMessages = new string[10];
        public Random rand = new Random();
        bool WasPaused = false;
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
            sr = new StreamReader("levels1.txt");
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                string[] lineSplit = currentLine.Split(',');
                waves[counter] = new int[lineSplit.Length];
                for (int i = 0; i < lineSplit.Length; i++)
                {
                    int.TryParse(lineSplit[i], out waves[counter][i]);
                }
                counter++;
            }
            counter = 0;
            sr = new StreamReader("levels2.txt");
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                levelMessages[counter] = currentLine;
                counter++;
            }
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (gameState == GameState.store)
            {
                Canvas.SetTop(tempRect, Mouse.GetPosition(cBackground).Y - tempRect.Height / 2);
                Canvas.SetLeft(tempRect, Mouse.GetPosition(cBackground).X - tempRect.Width / 2);
                Canvas.SetTop(tempCirc, Mouse.GetPosition(cBackground).Y - tempCirc.Height / 2);
                Canvas.SetLeft(tempCirc, Mouse.GetPosition(cBackground).X - tempCirc.Width / 2);
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
                        cBackground.Children.Remove(tempCirc);
                        Tower newTower = new Tower(tempTowerType, cBackground, cObstacles, positions, track, temp, cEnemies);
                        towers.Add(newTower);
                        money -= tempCost;
                        lblMoney.Content = "$ " + money;
                    }
                    else
                    {
                        cBackground.Children.Remove(tempRect);
                        cBackground.Children.Remove(tempCirc);
                    }
                    cObstacles.Children.Remove(trackHit);
                    if (WasPaused == true)
                    {
                        gameState = GameState.pause;
                    }
                    else
                    {
                        gameState = GameState.play;
                    }
                }
                else
                {
                    pmbs = Mouse.LeftButton;
                }
            }
            else if (gameState == GameState.pause)
            {

            }
            else if (gameState == GameState.play)
            {
                if (enemies.Count == 0)
                {
                    if (level < 10)
                    {
                        for (int i = 0; i < waves[level].Length; i++)
                        {
                            enemies.Add(new Enemy(waves[level][i], cEnemies, cBackground, track, positions));
                        }
                        MessageBox.Show(levelMessages[level]);

                        gameState = GameState.pause;
                        Button PausedGame = new Button();
                        PausedGame.Content = "Click here to go to next wave";
                        PausedGame.Click += PausedGame_Click;
                        PausedGame.Background = Brushes.Gray;
                        cBackground.Children.Add(PausedGame);
                    }
                    else
                    {
                        for (int i = 0; i < level * level; i++)
                        {
                            enemies.Add(new Enemy(rand.Next(5), cEnemies, cBackground, track, positions));
                        }
                        MessageBox.Show("Level " + level);
                    }
                    level++;
                    /*for (int i = enemies.Count - 1; i > -1; i--)
                    {
                        enemies[i].update(i);
                    }*/
                }
                for (int i = 0; i < towers.Count; i++)//loops through each tower
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
                                    if ((int)enemies[y].type == 2)
                                    {
                                        if ((int)towers[i].towerType == 1)
                                        {
                                            enemies[y].health -= towers[i].damage;
                                        }
                                    }
                                    else
                                    {
                                        enemies[y].health -= towers[i].damage;
                                    }
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
                            MessageBox.Show("You Lost");
                            Close();
                        }
                    }
                }
                /*string tempEnemies = "";
                for (int i = 1; i < positions.Length + 1; i++)
                {
                    int index = positions[positions.Length - i];
                    if (index != -1)
                    {
                        if (!tempEnemies.Contains(index.ToString()))
                        {
                            tempEnemies += index.ToString();
                            int tempDamage = enemies[index].update(index);
                            if (tempDamage > 0)
                            {
                                damageBar.Width += tempDamage;
                                Canvas.SetLeft(damageBar, 825 - damageBar.Width);
                                enemies.RemoveAt(index);
                                if (damageBar.Width > 825)
                                {
                                    MessageBox.Show("You Lost");
                                    Close();
                                }
                            }
                        }
                    }
                }*/


                //new check because of bugs, inefficient way to do it, but we couldn't debug what was happening

                for (int i = 0; i < positions.Length; i++)
                {
                    int index = positions[i];
                    if (positions[i] != -1)
                    {
                        if (index < enemies.Count)
                        {

                            if (enemies[index].position + 9 < i || enemies[index].position - 9 > i)
                            {
                                positions[i] = -1;
                            }
                        }
                        else
                        {
                            positions[i] = -1;
                        }
                    }
                }
            }
        }

        private void PausedGame_Click(object sender, RoutedEventArgs e)
        {
            gameState = GameState.play;
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
                
                Label towerInfo = new Label();
                if (i == 0)
                {
                    towerInfo.Content = "Cost: 150\n" +
                    "Range: 100\n" +
                    "Damage: 25";
                }
                else if (i == 1)
                {
                    towerInfo.Content = "Cost: 350\n" +
                    "Range: 300\n" +
                    "Damage: 50";
                }
                else if (i == 2)
                {
                    towerInfo.Content = "Cost: 600\n" +
                    "Range: 80\n" +
                    "Damage: 10";
                }
                else
                {
                    towerInfo.Content = "Cost: 2000\n" +
                    "Range: 100\n" +
                    "Damage: 500";
                }
                towerInfo.FontWeight = FontWeights.Bold;
                towerInfo.Background = Brushes.SandyBrown;
                Canvas.SetTop(towerInfo, i * 150 + 70);
                Canvas.SetLeft(towerInfo, 1020);
                cBackground.Children.Add(towerInfo);
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
            lblMoney.Width = 200;
            Canvas.SetTop(lblMoney, 10);
            Canvas.SetLeft(lblMoney, 875);
            lblMoney.Content = "$ " + money;
            cBackground.Children.Add(lblMoney);
        }
        private void iconsClick(object sender, RoutedEventArgs e)
        {
            if (gameState == GameState.pause)
            {
                WasPaused = true;
            }

            if (gameState != GameState.store)
            {
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
                //
                tempCirc = new Ellipse();
                tempCirc.Opacity = 0.5;
                tempCirc.Fill = Brushes.White;
                //
                
                if (tempTowerType == 0)
                {
                    tempRect.Height = 35;
                    tempRect.Width = 35;
                    tempCost = 150;
                    //
                    tempCirc.Width = 100 * 2 + 1;
                    tempCirc.Height = 100 * 2 + 1;
                    //
                }
                else if (tempTowerType == 1)
                {
                    tempRect.Height = 35;
                    tempRect.Width = 35;
                    tempCost = 350;
                    //
                    tempCirc.Width = 300 * 2 + 1;
                    tempCirc.Height = 300 * 2 + 1;
                    //
                }
                else if (tempTowerType == 2)
                {
                    tempRect.Height = 45;
                    tempRect.Width = 70;
                    tempCost = 600;
                    //
                    tempCirc.Width = 80 * 2 + 1;
                    tempCirc.Height = 80 * 2 + 1;
                    //
                }
                else
                {
                    tempRect.Height = 70;
                    tempRect.Width = 70;
                    tempCost = 2000;
                    //
                    tempCirc.Width = 100 * 2 + 1;
                    tempCirc.Height = 100 * 2 + 1;
                    //
                }
                //
                cBackground.Children.Add(tempCirc);
                //
                cBackground.Children.Add(tempRect);
                
            }
        }
    }
}
