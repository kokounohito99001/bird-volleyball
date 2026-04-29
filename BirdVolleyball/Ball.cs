using System;
using System.Drawing;

namespace BirdVolleyball
{
    public class Ball
    {
        public float X, Y;
        public float VelocityX, VelocityY;
        public int Radius = 15;
        public bool IsActive;
        public int TouchCount;
        public int LastTouchedBy; // 0 = player, 1 = bot
        
        public Ball(int x, int y)
        {
            X = x;
            Y = y;
            VelocityX = 0;
            VelocityY = 0;
            IsActive = false;
            TouchCount = 0;
            LastTouchedBy = -1;
        }
        
        public void Reset(int x, int y)
        {
            X = x;
            Y = y;
            VelocityX = 0;
            VelocityY = 0;
            IsActive = false;
            TouchCount = 0;
            LastTouchedBy = -1;
        }
        
        public void ApplyGravity(float gravity)
        {
            if (IsActive)
            {
                VelocityY += gravity;
                X += VelocityX;
                Y += VelocityY;
            }
        }
        
        public void BounceWall(int fieldWidth)
        {
            if (X - Radius < 0)
            {
                X = Radius;
                VelocityX = -VelocityX * 0.8f;
            }
            else if (X + Radius > fieldWidth)
            {
                X = fieldWidth - Radius;
                VelocityX = -VelocityX * 0.8f;
            }
        }
        
        public void BounceCeiling()
        {
            if (Y - Radius < 0)
            {
                Y = Radius;
                VelocityY = -VelocityY * 0.8f;
            }
        }
        
        public void HitLowPass(float forceX, float forceY, int playerIndex)
        {
            VelocityX = forceX * (playerIndex == 0 ? 1 : -1);
            VelocityY = forceY;
            IsActive = true;
            TouchCount++;
            LastTouchedBy = playerIndex;
        }
        
        public void HitSmash(float forceX, float forceY, int playerIndex)
        {
            VelocityX = forceX * (playerIndex == 0 ? 1 : -1);
            VelocityY = forceY;
            IsActive = true;
            TouchCount++;
            LastTouchedBy = playerIndex;
        }
        
        public Rectangle GetBounds()
        {
            return new Rectangle((int)(X - Radius), (int)(Y - Radius), Radius * 2, Radius * 2);
        }
        
        public bool Intersects(Rectangle rect)
        {
            int closestX = Math.Max(rect.X, Math.Min(X, rect.X + rect.Width));
            int closestY = Math.Max(rect.Y, Math.Min(Y, rect.Y + rect.Height));
            
            float dx = X - closestX;
            float dy = Y - closestY;
            
            return (dx * dx + dy * dy) <= (Radius * Radius);
        }
        
        public bool IntersectsHead(Rectangle headRect)
        {
            return Intersects(headRect);
        }
    }
}
