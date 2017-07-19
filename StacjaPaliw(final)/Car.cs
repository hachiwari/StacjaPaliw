using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StacjaPaliw_final_
{
    class Car
    {
        public static int WIDTH = 15, HEIGHT = 25;
        private int index;
        private float x, y, angle;
        private Color color;

        public Car()
        {
            index = 0;
            x = 0;
            y = 0;
            color = Color.Green;
            angle = 0;
        }

        public Car(int index, float x, float y, Color color)
        {
            this.index = index;
            this.x = x;
            this.y = y;
            this.color = color;
            this.angle = 0;
        }

        public void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public int getIndex()
        {
            return index;
        }
        public float getX()
        {
            return x;
        }

        public float getY()
        {
            return y;
        }
        public Color getColor()
        {
            return color;
        }

        public float getAngle()
        {
            return angle;
        }

        public void setAngle(float angle)
        {
            this.angle = angle;
        }
    }
}
