using System;
using System.Drawing;

namespace BirdVolleyball
{
    public class Player
    {
        public Rectangle Body;
        public float VelocityY;
        public bool IsGrounded;
        public bool HasTouchedBall;
        public Color Color;
        public int Score;
        public bool IsBot;
        
        public int Width = 40;
        public int Height = 60;
        
        public Player(int x, int y, Color color, bool isBot = false)
        {
            Body = new Rectangle(x, y, Width, Height);
            VelocityY = 0;
            IsGrounded = true;
            HasTouchedBall = false;
            Color = color;
            Score = 0;
            IsBot = isBot;
        }
        
        public void MoveLeft(int speed, int groundLevel)
        {
            if (Body.X > 0)
            {
                Body.X -= speed;
                if (Body.X < 0) Body.X = 0;
            }
        }
        
        public void MoveRight(int speed, int fieldWidth, int netX)
        {
            int limit = IsBot ? fieldWidth - Width : netX - Width;
            if (Body.X < limit)
            {
                Body.X += speed;
                if (Body.X > limit) Body.X = limit;
            }
        }
        
        public void Jump(float jumpForce)
        {
            if (IsGrounded)
            {
                VelocityY = -jumpForce;
                IsGrounded = false;
            }
        }
        
        public void ApplyGravity(float gravity, int groundLevel)
        {
            if (!IsGrounded)
            {
                VelocityY += gravity;
                Body.Y += (int)VelocityY;
                
                if (Body.Y >= groundLevel - Height)
                {
                    Body.Y = groundLevel - Height;
                    VelocityY = 0;
                    IsGrounded = true;
                }
            }
        }
        
        public void ResetPosition(int x, int groundLevel)
        {
            Body.X = x;
            Body.Y = groundLevel - Height;
            VelocityY = 0;
            IsGrounded = true;
            HasTouchedBall = false;
        }
        
        public Rectangle GetHeadRect()
        {
            return new Rectangle(Body.X + 5, Body.Y + 5, 30, 25);
        }
        
        public Rectangle GetBeakRect()
        {
            if (IsBot)
                return new Rectangle(Body.X - 10, Body.Y + 15, 10, 15);
            else
                return new Rectangle(Body.X + Width, Body.Y + 15, 10, 15);
        }
    }
}
