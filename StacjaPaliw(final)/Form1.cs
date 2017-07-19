using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StacjaPaliw_final_
{
    public partial class Form1 : Form
    {
        private int STARTX = 139, STARTY = -30; // 260

        private static int DISTRIBUTOR_GAP = 40;
        private static int CAR_SPEED = 25, PERSON_SPEED = 40;
        private static Random random = new Random();
        private static Color[] carColor = { Color.Yellow, Color.Red, Color.Blue, Color.Brown, Color.Cyan, Color.Gold, Color.Lime };
        private static Color[] personColor = { Color.Yellow, Color.White };
        private static int carCount = 20, distributorCount = 8, sellerCount = 2;

        private static int[] routeAxes = { 170, 280, 290, 205, 280, 250 };
        private static float[] routeAngle = { 0.0f, 135.0f, 90.0f, 45.0f, 0.0f };
        private static int[,] freeSellerY = { { 30, 70 }, { 20, 80 } };

        private static Boolean[] freeDistribute;
        private static Boolean[] freeSeller;
        private static Semaphore distributorSemaphore;
        private static Semaphore routeToDistributeSemaphore;
        private static Semaphore routeToExitSemaphore;
        private static Semaphore routeToSellerSemaphore;
        private static Semaphore ekran;
        private static Thread[] clients;
        private static List<Car> cars = new List<Car>();
        private static List<Person> persons = new List<Person>();
        private static int indexClientInQueue = 0;

        private static Boolean isCarToExit = false;
        private static int personID = 0;
        private static int countThread = 0;

        public Form1()
        {
            InitializeComponent();
            button1.Enabled = false;
            button2.Enabled = true;
            timer1.Start();

            distributorSemaphore = new Semaphore(0, distributorCount);
            routeToExitSemaphore = new Semaphore(0, 1);
            ekran = new Semaphore(0, 1);
            routeToDistributeSemaphore = new Semaphore(0, 1);
            routeToSellerSemaphore = new Semaphore(0, sellerCount);

            clients = new Thread[carCount];
            for (int i = 0; i < carCount; i++)
            {
                Car newCar = new Car(i, STARTX, STARTY/* - (37 * i)*/, carColor[random.Next(0, carColor.Length)]);
                cars.Add(newCar);

                clients[i] = new Thread(new ParameterizedThreadStart(Client));
                clients[i].Name = "Car#" + i;
                clients[i].Start(newCar);

                //STARTY -= 37;
                countThread++;
                Thread.Sleep(20);
            }

            freeDistribute = new Boolean[distributorCount];
            for (int i = 0; i < distributorCount; i++)
            {
                freeDistribute[i] = true;
            }
            freeSeller = new Boolean[sellerCount];
            for (int i = 0; i < sellerCount; i++)
            {
                freeSeller[i] = true;
            }

            Thread.Sleep(500);
            routeToExitSemaphore.Release(1);
            ekran.Release(1);
            routeToDistributeSemaphore.Release(1);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

         private void drawCar(Graphics g, Car car) 
         {
            SolidBrush bursh = new SolidBrush(car.getColor());
            using (Matrix m = new Matrix()) 
            {
                m.RotateAt(car.getAngle(), new PointF(car.getX() + (Car.WIDTH / 2), car.getY() + (Car.HEIGHT / 2)));
                g.Transform = m;
                g.FillRectangle(bursh, car.getX(), car.getY(), Car.WIDTH, Car.HEIGHT);
                g.ResetTransform();
            }
        }

         private void drawPerson(Graphics g, Person person)
         {
             SolidBrush bursh = new SolidBrush(person.getColor());
             g.FillEllipse(bursh, person.getX(), person.getY(), person.getSize(), person.getSize());
         }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.HighQuality;
            ekran.WaitOne();
            lock (cars) {
                foreach (Car car in cars)
                {
                    drawCar(g, car);
                }
            }

            lock (persons) {
                foreach (Person person in persons)
                {
                    drawPerson(g, person);
                }
            }
            ekran.Release();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            this.Text = "Stacja paliw (" + countThread.ToString() + ")";
            if  (countThread == 0)
            {
                this.Text = "Stacja paliw (End)";
                button1.Enabled = false;
            }
        }

        private static int getFreeDistribute()
        {
            int i = 0;

            foreach (Boolean x in freeDistribute)
            {
                if (x) break;
                else i++;
            }

            return i;
        }

        private static int getFreeSeller()
        {
            int i = 0;

            foreach (Boolean x in freeSeller)
            {
                if (x) break;
                else i++;
            }

            return i;
        }

        private static void routeToDistributor(Car car, int index)
        {
            routeToDistributeSemaphore.WaitOne();
            car.setAngle(routeAngle[0]);
            while (car.getY() != routeAxes[0])
            {
                car.setPosition(car.getX(), car.getY() + 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            routeToDistributeSemaphore.Release();
            while (car.getY() != routeAxes[1])
            {
                car.setPosition(car.getX(), car.getY() + 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(routeAngle[1]);
            while (car.getY() != routeAxes[2])
            {
                car.setPosition(car.getX() + 1, car.getY() + 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(routeAngle[2]);
            while (car.getX() != routeAxes[3] + (index * DISTRIBUTOR_GAP))
            {
                car.setPosition(car.getX() + 1, car.getY());
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(routeAngle[3]);
            while (car.getY() != routeAxes[4])
            {
                car.setPosition(car.getX() + 1, car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(routeAngle[4]);
            while (car.getY() != routeAxes[5])
            {
                car.setPosition(car.getX(), car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
        }

        private static void routeToExit(Car car)
        {
            car.setAngle(0.0f);
            while (car.getY() != 213)
            {
                car.setPosition(car.getX(), car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            routeToExitSemaphore.WaitOne();
            isCarToExit = true;
            while (car.getY() != 200)
            {
                car.setPosition(car.getX(), car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(-45.0f);
            while (car.getY() != 190)
            {
                car.setPosition(car.getX() - 1, car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(-90.0f);
            while (car.getX() != 200)
            {
                car.setPosition(car.getX() - 1, car.getY());
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            car.setAngle(-45.0f);
            while (car.getX() != 167)
            {
                car.setPosition(car.getX() - 1, car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
            isCarToExit = false;
            routeToExitSemaphore.Release();
            car.setAngle(0.0f);
            while (car.getY() != 0)
            {
                car.setPosition(car.getX(), car.getY() - 1);
                System.Threading.Thread.Sleep(CAR_SPEED);
            }
        }

        private static void routeToShop(Person person)
        {
            System.Threading.Thread.Sleep(2000);
            while (person.getY() != 214)
            {
                person.setPosition(person.getX(), person.getY() - 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (isCarToExit) ;
            while (person.getY() != 180)
            {
                person.setPosition(person.getX(), person.getY() - 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getX() != 292)
            {
                person.setPosition((person.getX() > 292) ? person.getX() - 1 : person.getX() + 1, person.getY());
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getY() != 50)
            {
                person.setPosition(person.getX(), person.getY() - 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getX() != 400 - (indexClientInQueue * (person.getSize() + 3)))
            {
                if (person.getX() + 1 >= 400) { person.setPosition(400, person.getY()); }
                else
                {
                    person.setPosition(person.getX() + 1, person.getY());
                    System.Threading.Thread.Sleep(PERSON_SPEED);
                }
            }
            indexClientInQueue++;

            int indexFreeSeller = -1;
            routeToSellerSemaphore.WaitOne();
            indexFreeSeller = getFreeSeller();
            freeSeller[indexFreeSeller] = false;

            while (person.getY() != freeSellerY[0, indexFreeSeller])
            {
                person.setPosition(person.getX() + 1, (person.getY() > freeSellerY[0, indexFreeSeller]) ? person.getY() - 1 : person.getY() + 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getX() != 430)
            {
                person.setPosition(person.getX() + 1, person.getY());
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            System.Threading.Thread.Sleep(3000);
            person.setSize(person.getSize() - 2);
            System.Threading.Thread.Sleep(2000);
            freeSeller[indexFreeSeller] = true;
            routeToSellerSemaphore.Release();
            indexClientInQueue--;
            routeToCar(person, indexFreeSeller);
        }

        private static void routeToCar(Person person, int indexSeller)
        {
            int newY = freeSellerY[1, indexSeller];

            while (person.getY() != newY)
            {
                person.setPosition(person.getX() - 1, (person.getY() > newY) ? person.getY() - 1 : person.getY() + 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getX() != 283)
            {
                person.setPosition(person.getX() - 1, person.getY());
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getY() != 180)
            {
                person.setPosition(person.getX(), person.getY() + 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getX() != person.getDistributorX())
            {
                person.setPosition((person.getX() > person.getDistributorX()) ? person.getX() - 1 : person.getX() + 1, person.getY());
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (person.getY() != 183)
            {
                person.setPosition(person.getX(), person.getY() + 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            while (isCarToExit) ;
            while (person.getY() != person.getDistributorY())
            {
                person.setPosition(person.getX(), person.getY() + 1);
                System.Threading.Thread.Sleep(PERSON_SPEED);
            }
            lock (persons) { persons.Remove(person); }
        }

        private static Person makePerson(Car car)
        {
            Person newPerson = new Person(personID++, car.getX() - Person.INIT_SIZE, car.getY(), personColor[random.Next(0, personColor.Length)]);
            Console.WriteLine(personID);
            persons.Add(newPerson);
            return newPerson;
        }

        public static void Client(object newCar)
        {
            int indexDistributor = -1;
            Car car = (Car)newCar;

            distributorSemaphore.WaitOne();
            indexDistributor = getFreeDistribute();
            freeDistribute[indexDistributor] = false;
            routeToDistributor(car, indexDistributor);
            System.Threading.Thread.Sleep(random.Next(1000, 1500));
            routeToShop(makePerson(car));
            System.Threading.Thread.Sleep(2000);
            freeDistribute[indexDistributor] = true;
            distributorSemaphore.Release();

            routeToExit(car);
            lock (car) { cars.Remove(car); }
            countThread--;
            Console.WriteLine("Zakonczenie watku: " + car.getIndex());
        }

        public void endThread()
        {
            foreach (Thread t in clients)
            {
                t.Abort();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            endThread();
            button1.Enabled = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            distributorSemaphore.Release(distributorCount);
            routeToSellerSemaphore.Release(sellerCount);
            button1.Enabled = true;
            button2.Enabled = false;
        }
    }
}
