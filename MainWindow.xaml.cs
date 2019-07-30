using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;
using System.Windows.Media.Imaging;
using System.Collections;

namespace app
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>  
    public partial class MainWindow : Window {
        // Game variables
        private int gameSpeed = 1500000;
        private int score = 100;
        private int level = 1;
        private TextBlock scoreboardText = new TextBlock();
        private Rectangle scoreboardDisplay = new Rectangle();
        private Stopwatch sw = new Stopwatch();
        private ImageBrush gameBackground = new ImageBrush();
        public enum snakeDirections{
            right,
            down,
            left,
            up
        };
        //snakeHead
        public SnakeHead snakeHead;
        // Snakebody
        public List<SnakeHead> snakeBody = new List<SnakeHead>();
        // Food
        private Food food;
        public MainWindow(){
            InitializeComponent();
            start(); 
        }
        public void start(){
            // Initiate canvas updater
            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += new EventHandler(updateCanvas);
            timer.Interval = new TimeSpan((int)gameSpeed);
            timer.Start();
            // Keyboard event handler
            this.KeyDown += new KeyEventHandler(keyInput);
            //instantiate objects
            snakeHead = new SnakeHead(350, 350, 25, 25, Brushes.DarkGreen);
            food = new Food(25, Brushes.Red);
            //Set scoreboard
            createScoreboard(score);
            createLevelCounter();
            // Start stopwatch to control keyinput (Only one keyinput per gamespeed-tick - Solved: going backwards crashing into own tail)
            sw.Start();
        }
        public void createScoreboard(int score) {
            scoreboardText.Text = "Score: " + score + "          Level: " + level + "         Eaten: " + (score/100) + "/" + (5*level);
            scoreboardText.FontSize = 30;
            scoreboardText.Foreground = Brushes.Black;
            Canvas.SetLeft(scoreboardText, 500);
            Canvas.SetTop(scoreboardText, 10);
            Scoreboard.Children.Add(scoreboardText);
        }
        public void updateScoreboard(int score){
            Scoreboard.Children.Remove(scoreboardText);
            scoreboardText.Text = "Score: " + score + "          Level: " + level + "         Eaten: " + (score/100) + "/" + (5*level);
            Scoreboard.Children.Add(scoreboardText);
        }
        public double measureScoreboardWidth(){
            // Measure scoreboard
            scoreboardText.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
            var scoreboardSize = scoreboardText.DesiredSize;
            return scoreboardSize.Width;    
        }
        public void createLevelCounter(){
            scoreboardDisplay.Fill = Brushes.Red;
            scoreboardDisplay.Height = 25;
            scoreboardDisplay.Width = 25;
            scoreboardDisplay.Stroke = new SolidColorBrush(Colors.Black);
            scoreboardDisplay.StrokeThickness = 2;
            // Scoreboard x-starting point + scoreboard width + extra space;
            Canvas.SetLeft(scoreboardDisplay, (measureScoreboardWidth() + (Canvas.GetLeft(scoreboardText) + 25)));
            Canvas.SetTop(scoreboardDisplay, 15);
            Scoreboard.Children.Add(scoreboardDisplay);
        }
        public void updateLevelCounter(Boolean reset){
            if(!reset){
                Scoreboard.Children.Remove(scoreboardDisplay);
                scoreboardDisplay.Width += 25;
                // Scoreboard x-starting point + scoreboard width + extra space;
                Canvas.SetLeft(scoreboardDisplay, (measureScoreboardWidth() + (Canvas.GetLeft(scoreboardText) + 25)));
                Scoreboard.Children.Add(scoreboardDisplay);
            }else{
                Scoreboard.Children.Remove(scoreboardDisplay);
                scoreboardDisplay.Width = 25;
                // Scoreboard x-starting point + scoreboard width + extra space;
                Canvas.SetLeft(scoreboardDisplay, (measureScoreboardWidth() + (Canvas.GetLeft(scoreboardText) + 25)));
                Scoreboard.Children.Add(scoreboardDisplay);
            }
        }
        public void changeLevel(){
            switch(level){
                case 2:
                    gameBackground.ImageSource = new BitmapImage(new Uri("images/level2.jpg", UriKind.Relative));
                    gameBackground.TileMode = TileMode.Tile;
                    gameBackground.ViewportUnits = BrushMappingMode.Absolute;
                    gameBackground.Viewport = new Rect(0, 0, gameBackground.ImageSource.Width, gameBackground.ImageSource.Height);
                    this.Background = gameBackground;
                    break;
                case 3:
                    gameBackground.ImageSource = new BitmapImage(new Uri("images/level3.jpg", UriKind.Relative));
                    gameBackground.TileMode = TileMode.Tile;
                    gameBackground.ViewportUnits = BrushMappingMode.Absolute;
                    gameBackground.Viewport = new Rect(0, 0, gameBackground.ImageSource.Width, gameBackground.ImageSource.Height);
                    this.Background = gameBackground;
                    break;
                case 4:
                    gameBackground.ImageSource = new BitmapImage(new Uri("images/level4.jpg", UriKind.Relative));
                    gameBackground.TileMode = TileMode.Tile;
                    gameBackground.ViewportUnits = BrushMappingMode.Absolute;
                    gameBackground.Viewport = new Rect(0, 0, gameBackground.ImageSource.Width, gameBackground.ImageSource.Height);
                    this.Background = gameBackground;
                    break;
                case 5:

                    gameBackground.ImageSource = new BitmapImage(new Uri("images/level5.jpg", UriKind.Relative));
                    gameBackground.TileMode = TileMode.None;
                    gameBackground.Stretch = Stretch.Fill;
                    gameBackground.Viewport = new Rect(0, 0, GameCanvas.Width+50, GameCanvas.Height+50);
                    this.Background = gameBackground;
                    break;
            }
        }
        public void updateCanvas(object sender, EventArgs e){
            food.clear(GameCanvas);
            food.draw(GameCanvas);
            snakeHead.clear(GameCanvas);
            snakeHead.move();
            snakeHead.draw(GameCanvas);
            // Tail
            for(var i = snakeBody.Count-1; i >= 0; i--){
                // Link first element in list to snakeHead
                if(i == 0){
                    snakeBody[i].clear(GameCanvas);
                    snakeBody[i].x = snakeHead.prevX;
                    snakeBody[i].y = snakeHead.prevY;
                    snakeBody[i].draw(GameCanvas);
                }else{
                    snakeBody[i].clear(GameCanvas);
                    snakeBody[i].x = snakeBody[i-1].x;
                    snakeBody[i].y = snakeBody[i-1].y;
                    snakeBody[i].draw(GameCanvas);
                }
            }
            snakeCollision();
            foodCollision();
            wallCollision();
        }
        public void keyInput(object sender, KeyEventArgs e){
            if(sw.ElapsedMilliseconds > (gameSpeed/10000)){
                switch(e.Key){
                    case Key.Right:
                        if(snakeHead.snakeCurrentDirection != (int)snakeDirections.left){
                            snakeHead.snakeCurrentDirection = (int)snakeDirections.right;
                            sw.Restart();
                        }
                        break;
                    case Key.Down:
                        if(snakeHead.snakeCurrentDirection != (int)snakeDirections.up){
                            snakeHead.snakeCurrentDirection = (int)snakeDirections.down;
                            sw.Restart();
                        }
                        break;
                    case Key.Left:
                        if(snakeHead.snakeCurrentDirection != (int)snakeDirections.right){
                            snakeHead.snakeCurrentDirection = (int)snakeDirections.left;
                            sw.Restart();
                        }
                        break;
                    case Key.Up:
                        if(snakeHead.snakeCurrentDirection != (int)snakeDirections.down){
                            snakeHead.snakeCurrentDirection = (int)snakeDirections.up;
                            sw.Restart();
                        }
                        break;
                }
            }
        }
        public void wallCollision(){
            if( (snakeHead.x < -25 || snakeHead.x > 1500) || (snakeHead.y < 25 || snakeHead.y > 750)){
                gameOver();
            }
        }
        public void foodCollision(){
            if(snakeHead.x == food.x && snakeHead.y == food.y){
                score += 100;
                food.newPosition();
                snakeBody.Add(new SnakeHead(snakeHead.prevX, snakeHead.prevY, 25, 25, Brushes.Green));
                updateScoreboard(score);
                updateLevelCounter(false);
                if((score % 500) == 0){
                    level += 1;
                    updateLevelCounter(true);
                    changeLevel();
                }
                updateScoreboard(score);
            }
        }
        public void snakeCollision(){
            for(var i = 0; i < snakeBody.Count; i++){
                if(snakeHead.x == snakeBody[i].x && snakeHead.y == snakeBody[i].y){
                    gameOver();
                }
            }
        }
        public void gameOver(){
           var result = MessageBox.Show("Du tapte! Starte på nytt?", "Game Over",  MessageBoxButton.YesNo);
           if(result == MessageBoxResult.Yes){
               restart();
           }else if(result == MessageBoxResult.No){
               this.Close();
           }
        }
        public void restart(){
            GameCanvas.Children.Clear();
            snakeHead.x = 350;
            snakeHead.y = 350;
            snakeHead.snakeCurrentDirection = (int)snakeDirections.right;
            score = 0;
            level = 0;
            updateScoreboard(score);
            updateLevelCounter(true);
            snakeBody.Clear();
        }
    }
    public class SnakeHead{
        public int x;
        public int y;
        public int prevX;
        public int prevY;
        private int snakeSpeed;
        public int snakeCurrentDirection;
        public Rectangle headRectangle = new Rectangle();
        public SnakeHead(int x, int y, int size, int speed, Brush color){
            this.x = x;
            this.y = y;
            this.snakeSpeed = speed;
            headRectangle.Fill = color;
            headRectangle.Width = size;
            headRectangle.Height = size;
        }
        public void draw(object sender){
            Canvas gameCanvas = sender as Canvas;
            Canvas.SetTop(headRectangle, y);
            Canvas.SetLeft(headRectangle, x);
            gameCanvas.Children.Add(this.headRectangle);
        }
        public void move(){
            prevX = x;
            prevY = y;
            switch(snakeCurrentDirection){
                // Right
                case 0:
                    x += snakeSpeed;
                    break;
                // Down                
                case 1:
                    y += snakeSpeed;
                    break;
                // Left
                case 2:
                    x -= snakeSpeed;
                    break;
                // Up
                case 3:
                    y -= snakeSpeed;
                    break;
            }
        }
        public void clear(object sender){
            Canvas gameCanvas = sender as Canvas;
            gameCanvas.Children.Remove(this.headRectangle);
        }
    }
    public class Food{
        public int x;
        public int y;
        Rectangle foodRectangle = new Rectangle();
        private Random rnd = new Random();
        public Food(int size, Brush color){
            foodRectangle.Fill = color;
            foodRectangle.Height = size;
            foodRectangle.Width = size;
            this.x =  (rnd.Next(0, 60) * 25);
            this.y =  (rnd.Next(2, 30) * 25);

        }
        public void draw(object sender){
            Canvas gameCanvas = sender as Canvas;
            Canvas.SetTop(this.foodRectangle, this.y);
            Canvas.SetLeft(this.foodRectangle, this.x);
            gameCanvas.Children.Add(this.foodRectangle);
        }
        public void newPosition(){
            // Random(0, canvasWidth/snakeHeadSize)*snakeHeadsize
            // Random(scoreboardHeight, canvasWidth/snakeHeadSize)*snakeHeadsize
            this.x =  (rnd.Next(0, 60) * 25);
            this.y =  (rnd.Next(2, 30) * 25);
        }
        public void clear(object sender){
            Canvas gameCanvas = sender as Canvas;
            gameCanvas.Children.Remove(foodRectangle);
        }
    }
}
