using System;
using System.Drawing;
using System.Windows.Forms;

namespace BirdVolleyball
{
    public class GameForm : Form
    {
        private GameSettings settings;
        private Player player;
        private Player bot;
        private Ball ball;
        
        private int fieldWidth = 800;
        private int fieldHeight = 500;
        private int groundLevel;
        private int netX;
        private int netHeight = 120;
        
        private Timer gameTimer;
        private Timer countdownTimer;
        private int countdownValue;
        private string gameState = "waiting"; // waiting, counting, playing, scored
        private string message = "Press SPACE to start!";
        
        private Random random = new Random();
        
        public GameForm()
        {
            settings = new GameSettings();
            
            ClientSize = new Size(fieldWidth, fieldHeight + 100);
            Text = "🐦 Bird Volleyball";
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            
            groundLevel = fieldHeight - 20;
            netX = fieldWidth / 2;
            
            InitializeGame();
            
            gameTimer = new Timer();
            gameTimer.Interval = 16;
            gameTimer.Tick += GameLoop;
            gameTimer.Start();
            
            countdownTimer = new Timer();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += CountdownTick;
            
            KeyDown += GameForm_KeyDown;
            KeyUp += GameForm_KeyUp;
        }
        
        private void InitializeGame()
        {
            player = new Player(100, groundLevel - 60, Color.Blue);
            bot = new Player(fieldWidth - 140, groundLevel - 60, Color.Red, true);
            ball = new Ball(100, 150);
            
            player.Score = 0;
            bot.Score = 0;
            gameState = "waiting";
            message = "Press SPACE to start!";
        }
        
        private void StartRound(int serverIndex)
        {
            ball.Reset(serverIndex == 0 ? 100 : fieldWidth - 100, 150);
            player.ResetPosition(100, groundLevel);
            bot.ResetPosition(fieldWidth - 140, groundLevel);
            
            countdownValue = 3;
            gameState = "counting";
            message = $"Round starts in {countdownValue}...";
            
            countdownTimer.Stop();
            countdownTimer.Start();
        }
        
        private void CountdownTick(object? sender, EventArgs e)
        {
            countdownValue--;
            if (countdownValue > 0)
            {
                message = $"Round starts in {countdownValue}...";
            }
            else
            {
                countdownTimer.Stop();
                gameState = "playing";
                message = "";
                ball.IsActive = true;
                ball.VelocityY = 2;
            }
            Invalidate();
        }
        
        private void GameLoop(object? sender, EventArgs e)
        {
            if (gameState == "playing")
            {
                UpdatePhysics();
                CheckCollisions();
                CheckScore();
                UpdateBot();
            }
            
            Invalidate();
        }
        
        private void UpdatePhysics()
        {
            ball.ApplyGravity(settings.Gravity);
            ball.BounceWall(fieldWidth);
            ball.BounceCeiling();
            
            player.ApplyGravity(settings.Gravity, groundLevel);
            bot.ApplyGravity(settings.Gravity, groundLevel);
            
            // Ball bounce on ground
            if (ball.Y + ball.Radius >= groundLevel)
            {
                ball.Y = groundLevel - ball.Radius;
                ball.VelocityY = -ball.VelocityY * settings.BallBounceFactor;
                
                if (Math.Abs(ball.VelocityY) < 1)
                    ball.VelocityY = 0;
            }
            
            // Net collision
            if (ball.X + ball.Radius > netX - 5 && ball.X - ball.Radius < netX + 5)
            {
                if (ball.Y + ball.Radius > groundLevel - netHeight)
                {
                    ball.VelocityX = -ball.VelocityX * 0.5f;
                    if (ball.X < netX)
                        ball.X = netX - 5 - ball.Radius;
                    else
                        ball.X = netX + 5 + ball.Radius;
                }
            }
        }
        
        private void CheckCollisions()
        {
            // Player collision
            CheckPlayerCollision(player, 0);
            CheckPlayerCollision(bot, 1);
        }
        
        private void CheckPlayerCollision(Player p, int playerIndex)
        {
            if (!p.HasTouchedBall)
            {
                // Head collision
                if (ball.IntersectsHead(p.GetHeadRect()))
                {
                    p.HasTouchedBall = true;
                    ball.LastTouchedBy = playerIndex;
                    
                    // Auto low pass on head touch
                    float dir = playerIndex == 0 ? 1 : -1;
                    ball.VelocityX = 8 * dir;
                    ball.VelocityY = -8;
                    ball.IsActive = true;
                }
                // Body collision - just bounce
                else if (ball.Intersects(p.Body))
                {
                    float dir = playerIndex == 0 ? 1 : -1;
                    ball.VelocityX = Math.Abs(ball.VelocityX) * dir;
                    ball.X += 5 * dir;
                }
            }
        }
        
        private void CheckScore()
        {
            // Ball touched ground
            if (ball.Y + ball.Radius >= groundLevel - 1 && Math.Abs(ball.VelocityY) < 0.5f)
            {
                ScorePoint();
            }
            
            // Ball out of bounds horizontally
            if (ball.X < 0 || ball.X > fieldWidth)
            {
                ScorePoint();
            }
        }
        
        private void ScorePoint()
        {
            gameState = "scored";
            
            int scorer;
            if (ball.X < netX)
            {
                // Ball landed on player side
                if (ball.LastTouchedBy == 0)
                {
                    // Player touched last, bot scores
                    bot.Score++;
                    scorer = 1;
                    message = "Bot scores!";
                }
                else
                {
                    // Bot touched last or nobody, player scores
                    player.Score++;
                    scorer = 0;
                    message = "Player scores!";
                }
            }
            else
            {
                // Ball landed on bot side
                if (ball.LastTouchedBy == 1)
                {
                    // Bot touched last, player scores
                    player.Score++;
                    scorer = 0;
                    message = "Player scores!";
                }
                else
                {
                    // Player touched last or nobody, bot scores
                    bot.Score++;
                    scorer = 1;
                    message = "Bot scores!";
                }
            }
            
            CheckWinCondition();
            
            if (gameState == "scored")
            {
                Timer resetTimer = new Timer();
                resetTimer.Interval = 2000;
                resetTimer.Tick += (s, e) =>
                {
                    resetTimer.Stop();
                    StartRound(scorer);
                };
                resetTimer.Start();
            }
        }
        
        private void CheckWinCondition()
        {
            int diff = Math.Abs(player.Score - bot.Score);
            if ((player.Score >= settings.PointsToWin || bot.Score >= settings.PointsToWin) && diff >= settings.MinWinDifference)
            {
                gameState = "waiting";
                string winner = player.Score > bot.Score ? "You Win!" : "Bot Wins!";
                message = $"{winner} Final: {player.Score}-{bot.Score}. Press SPACE to restart.";
                player.Score = 0;
                bot.Score = 0;
            }
        }
        
        private void UpdateBot()
        {
            if (gameState != "playing") return;
            
            int botCenter = bot.Body.X + bot.Width / 2;
            int targetX = (int)ball.X;
            
            // Simple AI: follow ball when it's on bot's side
            if (ball.X > netX)
            {
                if (botCenter < targetX - 20)
                {
                    bot.MoveRight((int)settings.PlayerSpeed * 0.7f, fieldWidth, netX);
                }
                else if (botCenter > targetX + 20)
                {
                    bot.MoveLeft((int)settings.PlayerSpeed * 0.7f, groundLevel);
                }
                
                // Jump and hit
                if (!bot.HasTouchedBall && ball.Y < groundLevel - 150 && Math.Abs(ball.X - botCenter) < 50)
                {
                    if (ball.Y < groundLevel - 200 && random.NextDouble() < 0.3)
                    {
                        // Smash attempt
                        bot.Jump(settings.JumpForce);
                        bot.HasTouchedBall = true;
                        ball.HitSmash(settings.SmashForceX, settings.SmashForceY, 1);
                    }
                    else if (ball.Y > groundLevel - 250)
                    {
                        // Low pass
                        bot.Jump(settings.JumpForce);
                        bot.HasTouchedBall = true;
                        ball.HitLowPass(settings.LowPassForceX, settings.LowPassForceY, 1);
                    }
                }
            }
            else
            {
                // Return to center position
                int homeX = fieldWidth - 140;
                if (botCenter < homeX - 30)
                {
                    bot.MoveRight((int)settings.PlayerSpeed * 0.5f, fieldWidth, netX);
                }
                else if (botCenter > homeX + 30)
                {
                    bot.MoveLeft((int)settings.PlayerSpeed * 0.5f, groundLevel);
                }
            }
        }
        
        private void GameForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (gameState == "waiting" && e.KeyCode == Keys.Space)
            {
                StartRound(0);
                return;
            }
            
            if (gameState == "playing")
            {
                if (e.KeyCode == settings.MoveLeft)
                {
                    player.MoveLeft((int)settings.PlayerSpeed, groundLevel);
                }
                if (e.KeyCode == settings.MoveRight)
                {
                    player.MoveRight((int)settings.PlayerSpeed, fieldWidth, netX);
                }
                if (e.KeyCode == settings.LowPass && !player.HasTouchedBall)
                {
                    player.Jump(settings.JumpForce);
                    if (ball.Intersects(player.GetHeadRect()) || Math.Abs(ball.X - (player.Body.X + player.Width/2)) < 50)
                    {
                        player.HasTouchedBall = true;
                        ball.HitLowPass(settings.LowPassForceX, settings.LowPassForceY, 0);
                    }
                }
                if (e.KeyCode == settings.Smash && !player.HasTouchedBall)
                {
                    player.Jump(settings.JumpForce * 1.2f);
                    if (ball.Intersects(player.GetHeadRect()) || Math.Abs(ball.X - (player.Body.X + player.Width/2)) < 50)
                    {
                        player.HasTouchedBall = true;
                        ball.HitSmash(settings.SmashForceX, settings.SmashForceY, 0);
                    }
                }
                if (e.KeyCode == settings.ResetRound)
                {
                    StartRound(0);
                }
            }
        }
        
        private void GameForm_KeyUp(object? sender, KeyEventArgs e)
        {
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            
            // Background
            g.Clear(Color.SkyBlue);
            
            // Ground
            g.FillRectangle(Brushes.Green, 0, groundLevel, fieldWidth, fieldHeight - groundLevel + 50);
            
            // Net
            g.FillRectangle(Brushes.White, netX - 3, groundLevel - netHeight, 6, netHeight);
            
            // Draw players
            DrawPlayer(g, player);
            DrawPlayer(g, bot);
            
            // Draw ball
            g.FillEllipse(Brushes.Yellow, ball.X - ball.Radius, ball.Y - ball.Radius, ball.Radius * 2, ball.Radius * 2);
            g.DrawEllipse(Pens.Black, ball.X - ball.Radius, ball.Y - ball.Radius, ball.Radius * 2, ball.Radius * 2);
            
            // Draw score
            g.DrawString($"Player: {player.Score}", new Font("Arial", 16), Brushes.Blue, 20, 10);
            g.DrawString($"Bot: {bot.Score}", new Font("Arial", 16), Brushes.Red, fieldWidth - 120, 10);
            
            // Draw message
            if (!string.IsNullOrEmpty(message))
            {
                g.DrawString(message, new Font("Arial", 20), Brushes.Black, fieldWidth / 2 - 150, groundLevel / 2);
            }
            
            // Controls hint
            g.DrawString("A/D: Move | Space: Low Pass | Shift: Smash | R: Reset", new Font("Arial", 10), Brushes.Gray, 10, fieldHeight + 10);
        }
        
        private void DrawPlayer(Graphics g, Player p)
        {
            // Body
            g.FillRectangle(new SolidBrush(p.Color), p.Body);
            g.DrawRectangle(Pens.Black, p.Body);
            
            // Head
            Rectangle head = p.GetHeadRect();
            g.FillEllipse(new SolidBrush(p.Color), head);
            g.DrawEllipse(Pens.Black, head);
            
            // Eye
            int eyeX = p.IsBot ? head.X + 5 : head.X + 20;
            g.FillEllipse(Brushes.White, eyeX, head.Y + 8, 8, 8);
            g.FillEllipse(Brushes.Black, eyeX + 2, head.Y + 10, 4, 4);
            
            // Beak
            Rectangle beak = p.GetBeakRect();
            g.FillEllipse(Brushes.Orange, beak);
            g.DrawEllipse(Pens.Black, beak);
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            gameTimer.Stop();
            countdownTimer.Stop();
            base.OnFormClosing(e);
        }
    }
}
