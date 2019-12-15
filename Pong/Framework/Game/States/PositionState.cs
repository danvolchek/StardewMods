using Pong.Framework.Menus;
using System;

namespace Pong.Framework.Game.States
{
    internal class PositionState : IState<PositionState>
    {
        private int xPosition = Menu.ScreenWidth / 2;
        private int yPosition = Menu.ScreenHeight / 2;

        public int XPosition
        {
            get => this.xPosition;
            set
            {
                this.xPosition = value;
                this.OnStateChanged();
            }
        }

        public int YPosition
        {
            get => this.yPosition;
            set
            {
                this.yPosition = value;
                this.OnStateChanged();
            }
        }

        public void SetState(PositionState state)
        {
            this.XPosition = state.XPosition;
            this.YPosition = state.YPosition;
        }

        public void Reset()
        {
            this.XPosition = Menu.ScreenWidth / 2;
            this.YPosition = Menu.ScreenHeight / 2;
        }

        public void Invert()
        {
            int newXPosition = this.XPosition;
            newXPosition -= Menu.ScreenWidth / 2;
            newXPosition *= -1;
            newXPosition += Menu.ScreenWidth / 2;

            int newYPosition = this.YPosition;
            newYPosition -= Menu.ScreenHeight / 2;
            newYPosition *= -1;
            newYPosition += Menu.ScreenHeight / 2;

            this.XPosition = newXPosition;
            this.YPosition = newYPosition;
        }

        public event EventHandler StateChanged;

        private void OnStateChanged()
        {
            this.StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
