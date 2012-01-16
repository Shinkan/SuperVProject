using System;
using Data.Realm;
using Data;
using System.Threading;

namespace CsClient
{
    public class Program
    {
        private struct UnfinityEnergy
        {
            public bool found;
            public bool visited;
            public int locationX;
            public int locationY;

            public void Point(int x, int y)
            {
                locationX = x;
                locationY = y;
            }
        }
        private enum Orientation { north, south, east, west };

        static AgentAPI agentTomek;
        static int energy;
        static WorldParameters cennikSwiata;
        static UnfinityEnergy unfinityEnergyField_1 = new UnfinityEnergy();
        static UnfinityEnergy unfinityEnergyField_2 = new UnfinityEnergy();
        static int doceloweHeight;
        static int obecneHeight = 0;
        static Orientation orientation;

        static void Listen(String a, String s) {
            if(a == "superktos") Console.WriteLine("~Słysze własne słowa~");
             Console.WriteLine(a + " krzyczy " + s);
        }
        
        static void Main(string[] args)
        {
            while (true)
            {
                    agentTomek = new AgentAPI(Listen);

                String ip = Settings.serverIP;
                String groupname = Settings.groupname;
                String grouppass = Settings.grouppass;

                if (ip == null)
                {
                    Console.Write("Podaj IP serwera: ");
                    ip = Console.ReadLine();
                }
                if (groupname == null)
                {
                    Console.Write("Podaj nazwe druzyny: ");
                    groupname = Console.ReadLine();
                }
                if (grouppass == null)
                {
                    Console.Write("Podaj haslo: ");
                    grouppass = Console.ReadLine();
                }

                Console.Write("Podaj nazwe swiata: ");
                String worldname = Console.ReadLine();
                    
                Console.Write("Podaj imie: ");    
                String imie = Console.ReadLine();

           
            
                try
                {   
                    cennikSwiata = agentTomek.Connect(ip, 6008, groupname, grouppass, worldname, imie);
                    Console.WriteLine(cennikSwiata.initialEnergy + " - Maksymalna energia");
                    Console.WriteLine(cennikSwiata.maxRecharge + " - Maksymalne doładowanie");
                    Console.WriteLine(cennikSwiata.sightScope + " - Zasięg widzenia");
                    Console.WriteLine(cennikSwiata.hearScope + " - Zasięg słyszenia");
                    Console.WriteLine(cennikSwiata.moveCost + " - Koszt chodzenia");
                    Console.WriteLine(cennikSwiata.rotateCost + " - Koszt obrotu");
                    Console.WriteLine(cennikSwiata.speakCost + " - Koszt mówienia");

                    energy = cennikSwiata.initialEnergy;
                    Alive();
                    //KeyReader();
                    agentTomek.Disconnect();
                    Console.ReadKey();
                    break;
                }
                catch (NonCriticalException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ReadKey();
                    break;
                }
            }
        }

        static void Alive() {
            bool alive = true;
            orientation = Orientation.north;
            while (alive)
            {
                if (!unfinityEnergyField_1.found || !unfinityEnergyField_2.found)
                    Szukaj();
                Zwiedzaj();
                Console.WriteLine("Moja energia: " + energy);
                if (energy == 0)
                    alive = false;
                Thread.Sleep(1000);
            }
        }

        static void Zwiedzaj()
        {
        }

        static void Szukaj()
        {
            Look();
            if (unfinityEnergyField_1.found)
            {
                if (!unfinityEnergyField_1.visited)
                {
                    TravelToSource(unfinityEnergyField_1.locationX, unfinityEnergyField_1.locationY);
                    unfinityEnergyField_1.visited = true;
                }
            }
            else if (unfinityEnergyField_2.found)
            {
                if (!unfinityEnergyField_2.visited)
                {
                    TravelToSource(unfinityEnergyField_2.locationX, unfinityEnergyField_2.locationY);
                    unfinityEnergyField_2.visited = true;
                }
            }
            else
            {
                if(!unfinityEnergyField_1.found)
                    TravelNorth();
                else
                    TravelSouth();
            }
        }

        static void TravelNorth()
        {
            while (!StepForward())
            {
                RotateLeft();
                if (orientation == Orientation.south)
                {
                    OrientedField[] pola = agentTomek.Look();
                    foreach (OrientedField pole in pola)
                    {
                        if (pole.x == -1 && pole.y == 1)
                            if (pole.IsStepable())
                            {
                                StepForward();
                                RotateLeft();
                            }
                        break;
                    }
                }
            }
        }
        static void TravelSouth()
        {
            while (!StepForward())
            {
                RotateLeft();
                if (orientation == Orientation.north)
                {
                    OrientedField[] pola = agentTomek.Look();
                    foreach (OrientedField pole in pola)
                    {
                        if (pole.x == -1 && pole.y == 1)
                            if (pole.IsStepable())
                            {
                                StepForward();
                                RotateLeft();
                            }
                        break;
                    }
                }
            }
        }

        static void TravelToSource(int x, int y)
        {
            if (x == 1 || x == -1)
            {
                for (int i = 0; i < y; i++)
                    if (!StepForward())
                        return;
                if (x == 1)
                {
                    RotateRight();
                    if (!StepForward())
                        return;
                }
                else
                {
                    RotateLeft();
                    if (!StepForward())
                        return;
                }
            }
            else if (x == 0)
            {
                for (int i = 0; i < y; i++)
                    if (!StepForward())
                        return;
            }
            else
            {
                //unsupported for |x|=>2
            }
            while (Recharge() != 0) ;
        }

        /*static void KeyReader() {
            bool loop = true;
            while(loop) {
                Console.WriteLine("Moja energia: " + energy);
                switch(Console.ReadKey().Key) {
                    case ConsoleKey.Spacebar: Look();
                        break;
                    case ConsoleKey.R: Recharge();
                        break;
                    case ConsoleKey.UpArrow: StepForward();
                        break;
                    case ConsoleKey.LeftArrow: RotateLeft();
                        break;
                    case ConsoleKey.RightArrow: RotateRight();
                        break;
                    case ConsoleKey.Enter: Speak();
                        break;
                    case ConsoleKey.Q: loop = false;
                        break;
                    case ConsoleKey.D: agentTomek.Disconnect();
                        break;
                    default: Console.Beep();
                        break;
                }
            }
        }*/
    #region moving
        private static int Recharge()
        {
            int added = agentTomek.Recharge();
            energy += added;
            Console.WriteLine("Otrzymano " + added + " energii");
            return added;
        }

        private static void Speak()
        {
            if (!agentTomek.Speak(Console.ReadLine(), 1))
                Console.WriteLine("Mowienie nie powiodlo sie - brak energii");
            else
                energy -= cennikSwiata.speakCost;
        }

        private static void RotateLeft()
        {
            if (!agentTomek.RotateLeft())
                Console.WriteLine("Obrot nie powiodl sie - brak energii");
            else
            {
                switch(orientation) {
                    case Orientation.north:
                        orientation = Orientation.west;
                        break;
                    case Orientation.west:
                        orientation = Orientation.south;
                        break;
                    case Orientation.south:
                        orientation = Orientation.east;
                        break;
                    case Orientation.east:
                        orientation = Orientation.north;
                        break;
                }
                energy -= cennikSwiata.rotateCost;
            }
        }

        private static void RotateRight()
        {
            if (!agentTomek.RotateRight())
                Console.WriteLine("Obrot nie powiodl sie - brak energii");
            else
            {
                switch (orientation)
                {
                    case Orientation.north:
                        orientation = Orientation.east;
                        break;
                    case Orientation.east:
                        orientation = Orientation.south;
                        break;
                    case Orientation.south:
                        orientation = Orientation.west;
                        break;
                    case Orientation.west:
                        orientation = Orientation.north;
                        break;
                }
                energy -= cennikSwiata.rotateCost;
            }
        }

        private static bool StepForward()
        {
            if (!agentTomek.StepForward())
                Console.WriteLine("Wykonanie kroku nie powiodlo sie");
            else if (energy >= cennikSwiata.moveCost)
            {
                energy -= cennikSwiata.moveCost * (1 + (doceloweHeight - obecneHeight) / 100);
                obecneHeight = doceloweHeight;
                return true;
            }
            return false;
        }

        private static void Look()
        {
            OrientedField[] pola = agentTomek.Look();
            foreach (OrientedField pole in pola)
            {
                if (pole.x == 0 && pole.y == 1)
                    doceloweHeight = pole.height;
                Console.WriteLine("-----------------------------");
                Console.WriteLine("POLE " + pole.x + "," + pole.y);
                Console.WriteLine("Wysokosc: " + pole.height);
                if (pole.energy != 0)
                {
                    Console.WriteLine("Energia: " + pole.energy);
                    if (pole.energy < 0)
                    {
                        if (!unfinityEnergyField_1.found)
                        {
                            unfinityEnergyField_1.found = true;
                            unfinityEnergyField_1.Point(pole.x, pole.y);
                        }
                        else if (!unfinityEnergyField_2.found)
                        {
                            if (unfinityEnergyField_2.locationX != unfinityEnergyField_1.locationX
                                && unfinityEnergyField_2.locationY != unfinityEnergyField_1.locationY)
                            {
                                unfinityEnergyField_2.found = true;
                                unfinityEnergyField_2.Point(pole.x, pole.y);
                            }
                        }
                    }
                }
                if (pole.obstacle)
                    Console.WriteLine("Przeszkoda");
                if (pole.agent != null)
                    Console.WriteLine("Agent " + pole.agent.fullName + " i jest obrocony na " + pole.agent.direction.ToString());
                Console.WriteLine("-----------------------------");
            }
        }
    #endregion
        /*private static void Look()
        {
            OrientedField[] pola = agentTomek.Look();
            foreach (OrientedField pole in pola)
            {
                Console.WriteLine("-----------------------------");
                Console.WriteLine("POLE " + pole.x + "," + pole.y);
                Console.WriteLine("Wysokosc: " + pole.height);
                if (pole.energy != 0)
                    Console.WriteLine("Energia: " + pole.energy);
                if (pole.obstacle)
                    Console.WriteLine("Przeszkoda");
                if (pole.agent != null)
                    Console.WriteLine("Agent " + pole.agent.fullName + " i jest obrocony na " + pole.agent.direction.ToString());
                Console.WriteLine("-----------------------------");
            }
        }*/
        
    }
}
