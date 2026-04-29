using System;
using System.Drawing;
using System.Windows.Forms;

namespace BirdVolleyball
{
    public class GameSettings
    {
        // Controls
        public Keys MoveLeft = Keys.A;
        public Keys MoveRight = Keys.D;
        public Keys LowPass = Keys.Space;
        public Keys Smash = Keys.ShiftKey;
        public Keys ResetRound = Keys.R;

        // Game settings
        public int PointsToWin = 7;
        public int MinWinDifference = 2;
        public float Gravity = 0.5f;
        public float BallBounceFactor = 0.7f;
        public float PlayerSpeed = 8f;
        public float JumpForce = 15f;
        
        // Ball physics
        public float LowPassForceX = 12f;
        public float LowPassForceY = -10f;
        public float SmashForceX = 15f;
        public float SmashForceY = -18f;
    }
}
