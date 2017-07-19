using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StacjaPaliw_final_
{
    class Person
    {
        public static int INIT_SIZE = 9;
        private int index, size;
        private float x, y;
        private float distributorX, distributorY;
        private Color color;

        public Person()
        {
            index = 0;
            x = 0;
            y = 0;
            color = Color.Green;
            this.size = INIT_SIZE;
        }

        public Person(int index, float x, float y, Color color)
        {
            this.index = index;
            this.distributorX = this.x = x;
            this.distributorY = this.y = y;
            this.color = color;
            this.size = INIT_SIZE;
        }

        public void setPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
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

        public void setSize(int size)
        {
            this.size = size;
        }

        public int getSize()
        {
            return size;
        }

        public int getDistributorX()
        {
            return (int)distributorX;
        }

        public int getDistributorY()
        {
            return (int)distributorY;
        }
    }
}
