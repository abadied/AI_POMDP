using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace POMDP
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = System.IO.Directory.GetCurrentDirectory();
            FileStream fs = new FileStream(path + "Debug.txt", FileMode.Create);
            Debug.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Debug.Listeners.Add(new TextWriterTraceListener(fs));
            MazeDomain maze = new MazeDomain(path + "/Domains/Maze3.txt");

            // POMDP PROB AUTOMATA //
            List<Action> actions = new List<Action> {
            new MazeAction("TurnLeft"),
            new MazeAction("TurnRight"),
            new MazeAction("Forward")
            };

            List<Observation> observations = new List<Observation>();
            for (int i = 0; i < 16; i++)
                observations.Add(new MazeObservation(i));

            ConvertionFunction cf = new ConvertionFunction(actions, observations);
            RandomPolicy p0 = new RandomPolicy(maze);

            string firstPath = "";
            int numberOfIterations = 100;
            int numberOfSteps = 100;
            int currBit = 0;
            // TODO: add for loop that creates obs file for each bit ( add new paths as well).
            maze.WriteObservationsFile(firstPath, cf, p0, numberOfIterations, numberOfSteps, currBit);
            // POMDP PROB AUTOMATA //


            //PointBasedValueIteration pbvi = new PointBasedValueIteration(maze);
            //pbvi.PointBasedVI(100, 20);

            //MDPValueFunction v = new MDPValueFunction(maze);
            //v.ValueIteration(0.5);

            RandomPolicy p0 = new RandomPolicy(maze);
            //MostLikelyStatePolicy p1 = new MostLikelyStatePolicy(v);
            //VotingPolicy p2 = new VotingPolicy(v);
            //QMDPPolicy p3 = new QMDPPolicy(v, maze);
            
            //double dADR1 = maze.ComputeAverageDiscountedReward(p1, 100, 100);
            //double dADR2 = maze.ComputeAverageDiscountedReward(p2, 100, 100);
            //double dADR3 = maze.ComputeAverageDiscountedReward(p3, 100, 100);
            //double dADR4 = maze.ComputeAverageDiscountedReward(pbvi, 100, 100);

            //MazeViewer viewer = new MazeViewer(maze);
            //viewer.Start();
            //maze.SimulatePolicy(pbvi, 10, viewer);

            Debug.Close();
        }
    }
}