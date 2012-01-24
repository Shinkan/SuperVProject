using System;
using Data.Realm;
using Data;
using System.Threading;
using AIMLbot;
using AIMLbot.Utils;

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
        static String[][] lookMatrix;
        static int lookMatrixW;
        static bool mapMaking;

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
                    lookMatrixW = cennikSwiata.sightScope;
                    lookMatrixW = lookMatrixW * lookMatrixW + 1;
                    lookMatrix = new String[lookMatrixW][];
                    for (int i = 0; i < lookMatrixW; i++)
                        lookMatrix[i] = new String[lookMatrixW];
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
            mapMaking = false;
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
                Thread.Sleep(1000);
            }
        }

        static void Zwiedzaj()
        {
            Console.WriteLine("Zwiedza");
            if (!StepForward())
                if (!GoRight())
                    RotateRight();
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
                    Console.WriteLine("Osiagnieto pierwsze zrodlo energii");
                    mapMaking = true;
                }

                else if (unfinityEnergyField_2.found)
                {
                    TravelToSource(unfinityEnergyField_2.locationX, unfinityEnergyField_2.locationY);
                    unfinityEnergyField_2.visited = true;
                    Console.WriteLine("Osiagnieto drugie zrodlo energii");
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
                if (!GoRight())
                {
                    if (!StepForward())
                    {
                        RotateLeft();
                        GoRight();
                    }
                }
            }
            else
            {
                if (!GoRight())
                {
                    if (!StepForward())
                    {
                        RotateLeft();
                        GoRight();
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
                if (x == -2)
                {
                    if (StepForward())
                    {
                        RotateLeft();
                        StepForward();
                        if (StepForward())
                        {
                            RotateRight();
                            StepForward();
                        }
                        else
                        {
                            RotateRight();
                            if (StepForward())
                            {
                                RotateLeft();
                                StepForward();
                            }
                            else
                                return;

                        }
                    }
                    else
                    {
                        RotateLeft();
                        if (StepForward())
                        {
                            RotateRight();
                            StepForward();
                            if(StepForward())
                            {
                                    RotateLeft();
                                    StepForward();
                            }
                            else
                            {
                                RotateLeft();
                                if (StepForward())
                                {
                                    RotateRight();
                                    StepForward();
                                }
                                else
                                    return;
                            }
                        }
                        else
                            return;
                    }
                }
                else
                {
                    if (StepForward())
                    {
                        RotateRight();
                        StepForward();
                        if (StepForward())
                        {
                            RotateLeft();
                            StepForward();
                        }
                        else
                        {
                            RotateLeft();
                            if (StepForward())
                            {
                                RotateRight();
                                StepForward();
                            }
                            else
                                return;

                        }
                    }
                    else
                    {
                        RotateRight();
                        if (StepForward())
                        {
                            RotateLeft();
                            StepForward();
                            if (StepForward())
                            {
                                RotateRight();
                                StepForward();
                            }
                            else
                            {
                                RotateRight();
                                if (StepForward())
                                {
                                    RotateLeft();
                                    StepForward();
                                }
                                else
                                    return;
                            }
                        }
                        else
                            return;
                    }
                }
            }
            while (Recharge() != 0) ;
            Console.WriteLine("Ładowanie zakończone");
        }

        static void MakeMap()
        {
            mapMaking = false;
            worldMap = new int[mapWidth][];
            for (int i = 0; i < mapWidth; i++)
                worldMap[i] = new int[mapWidth];
            RotateLeft();
            RotateLeft();
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
                switch (orientation)
                {
                    case Orientation.north:
                        locationY--;
                        break;
                    case Orientation.east:
                        locationX++;
                        break;
                    case Orientation.south:
                        locationY++;
                        break;
                    case Orientation.west:
                        locationX--;
                        break;
                }
                if (mapMaking)
                {
                    if (orientation == Orientation.north)
                        mapWidth--;
                    if (orientation == Orientation.south)
                        mapWidth++;
                }
                return true;
            }
            return false;
        }

        private static void Look()
        {
            for (int i = 0; i < lookMatrixW; i++)
                for (int j = 0; j < lookMatrixW; j++)
                    lookMatrix[j][i] = " ";
            OrientedField[] pola = agentTomek.Look();
            int fieldX = 0;
            int fieldY = 0;

            foreach (OrientedField pole in pola)
            {
                if (pole.x == 0 && pole.y == 1)
                    doceloweHeight = pole.height;
                switch (orientation)
                {
                    case Orientation.north:
                        fieldX = pole.x + cennikSwiata.sightScope;
                        fieldY = Math.Abs(pole.y - cennikSwiata.sightScope);
                        lookMatrix[cennikSwiata.sightScope][cennikSwiata.sightScope] = "^";
                        break;
                    case Orientation.west:
                        fieldY = Math.Abs(pole.x - cennikSwiata.sightScope);
                        fieldX = Math.Abs(pole.y - cennikSwiata.sightScope);
                        lookMatrix[cennikSwiata.sightScope][cennikSwiata.sightScope] = "<";
                        break;
                    case Orientation.south:
                        fieldX = -pole.x + cennikSwiata.sightScope;
                        fieldY = pole.y + cennikSwiata.sightScope;
                        lookMatrix[cennikSwiata.sightScope][cennikSwiata.sightScope] = "v";
                        break;
                    case Orientation.east:
                        fieldY = pole.x + cennikSwiata.sightScope;
                        fieldX = pole.y + cennikSwiata.sightScope;
                        lookMatrix[cennikSwiata.sightScope][cennikSwiata.sightScope] = ">";
                        break;
                }

                lookMatrix[fieldX][fieldY] = "_";
                if (pole.energy != 0)
                {
                    lookMatrix[fieldX][fieldY] = "e";
                    if (pole.energy < 0)
                    {
                        lookMatrix[fieldX][fieldY] = "i";
                        if (!unfinityEnergyField_1.found)
                        {
                            unfinityEnergyField_1.found = true;
                            unfinityEnergyField_1.Point(pole.x, pole.y);
                            if (pole.x >= 0 && orientation == Orientation.north)
                                orientation = Orientation.west;
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
                            if (locationX * locationX + locationY * locationY > 4)
                            {
                                unfinityEnergyField_2.found = true;
                                unfinityEnergyField_2.Point(pole.x, pole.y);
                            }
                        }
                    }
                }
                if (pole.obstacle)
                    lookMatrix[fieldX][fieldY] = "W";
                if (pole.agent != null)
                {
                    lookMatrix[fieldX][fieldY] = "A";
                    Console.WriteLine("Agent " + pole.agent.fullName + " i jest obrocony na " + pole.agent.direction.ToString());
                }
            }
            for (int i = 0; i < lookMatrixW; i++)
            {
                for (int j = 0; j < lookMatrixW; j++)
                    Console.Write(lookMatrix[j][i]+" ");
                Console.WriteLine();
            }
        }
    #endregion
    }
}
