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
        private static int[][] worldMap;

        static AgentAPI agentTomek;
        static int energy;
        static WorldParameters cennikSwiata;
        static UnfinityEnergy unfinityEnergyField_1 = new UnfinityEnergy();
        static UnfinityEnergy unfinityEnergyField_2 = new UnfinityEnergy();
        static int doceloweHeight;
        static int obecneHeight = 0;
        static Orientation orientation;
        static int mapWidth = 0;
        static int locationX;
        static int locationY;

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
                    groupname = "VGrupaX";// Console.ReadLine();
                }
                if (grouppass == null)
                {
                    Console.Write("Podaj haslo: ");
                    grouppass = "qvmlmo"; //Console.ReadLine();
                }

                Console.Write("Podaj nazwe swiata: ");
                String worldname = "VGrupaX"; //Console.ReadLine();
                    
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
                else
                    Zwiedzaj();
                Console.WriteLine("Moja energia: " + energy);
                if (energy == 0)
                    alive = false;
                Thread.Sleep(2000);
            }
        }

        static void Zwiedzaj()
        {
            Console.WriteLine("Zwiedza");
        }

        #region wyszukiwanieEnergii
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

                else if (unfinityEnergyField_2.found)
                {
                    TravelToSource(unfinityEnergyField_2.locationX, unfinityEnergyField_2.locationY);
                    unfinityEnergyField_2.visited = true;
                    MakeMap();
                }
                else
                    TravelSouth();
            }
            else
                TravelNorth();                    
        }

        //Zmierza na polnocny zachod
        static void TravelNorth()
        {
            if (orientation == Orientation.north)
            {
                if (!StepForward())
                {
                    RotateLeft();
                    GoRight();
                }
            }
            else
            {
                if (!GoRight())
                    if (!StepForward())
                    {
                        RotateLeft();
                        GoRight();
                    }
            }
        }
        //Zmierza na poludniowy zachod
        static void TravelSouth()
        {
            if (orientation == Orientation.south)
            {
                if (!StepForward())
                {
                    RotateRight();
                    GoLeft();
                }
                else
                    mapWidth++;
            }
            else
            {
                if (!GoLeft())
                {
                    if (!StepForward())
                    {
                        RotateLeft();
                        GoLeft();
                    }
                    else
                    {
                        if (orientation == Orientation.north)
                            mapWidth--;
                    }
                }
            }
        }
        
        //Sprawdza czy po przekatnej na prawo moze stanac i staje przed tym polem
        static bool GoRight()
        {
            OrientedField[] pola = agentTomek.Look();
            foreach (OrientedField pole in pola)
            {
                if (pole.x == 1 && pole.y == 1)
                    if (pole.IsStepable())
                        if (StepForward())
                        {
                            RotateRight();
                            return true;
                        }
            }
            return false;
        }
        //Sprawdza czy po przekatnej na lewo moze stanac i staje przed tym polem
        static bool GoLeft()
        {
            OrientedField[] pola = agentTomek.Look();
            foreach (OrientedField pole in pola)
            {
                if (pole.x == -1 && pole.y == 1)
                    if (pole.IsStepable())
                        if (StepForward())
                        {
                            if (orientation == Orientation.north)
                                mapWidth--;
                            if (orientation == Orientation.south)
                                mapWidth++;
                            RotateLeft();
                            return true;
                        }
            }
            return false;
        }

        //Podaza do znalezionego zrodla energii
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
            Console.WriteLine("Ładowanie zakończone");
        }
        static void MakeMap()
        {
            worldMap = new int[mapWidth][];
            for (int i = 0; i < mapWidth; i++)
                worldMap[i] = new int[mapWidth];
            //dodanie pol
        }
        #endregion

        #region akcjeAgenta
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
                            if (orientation == Orientation.north)
                            {
                                mapWidth -= pole.y;
                                locationX = Math.Abs(pole.x);
                                locationY = pole.y;
                            }
                            else
                            {
                                mapWidth -= pole.x;
                                locationX = pole.y;
                                locationY = pole.x;
                            }
                        }
                        else if (!unfinityEnergyField_2.found)
                        {
                            //Poprawic to (orientacja ma wplyw na polozenie)
                            if ((unfinityEnergyField_2.locationX - locationX) !=0
                                && (unfinityEnergyField_2.locationY - locationY !=0))
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
    }
}
