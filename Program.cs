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
            MazeDomain maze = new MazeDomain(path + "/Domains/Maze1.txt");
            // POMDP PROB AUTOMATA //

            int amountOfDirections = 4;
            int xNumBits = Convert.ToInt32(Math.Ceiling(Math.Log(maze.Width, 2)));
            int yNumBits = Convert.ToInt32(Math.Ceiling(Math.Log(maze.Height, 2)));
            int directionNumBits = Convert.ToInt32(Math.Ceiling(Math.Log(amountOfDirections, 2)));
            int numOfAutomatas = xNumBits + yNumBits + directionNumBits;

            // Create obs files
            Boolean createObsFiles = false;

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

            string startPath = "obs";
            string endPath = ".obs";
            int numberOfIterations = 1000;
            int numberOfSteps = 1000;
            int currBit = 0;
            int numberOfAutomataStates = 10;
            if (createObsFiles)
            {
                for (currBit = 0; currBit < numOfAutomatas; currBit++)
            {
                string Path = startPath + currBit.ToString() + endPath;
                maze.WriteObservationsFile(Path, cf, p0, numberOfIterations, numberOfSteps, currBit, actions.Count() * observations.Count(), numberOfAutomataStates);
            }
            }
            else { 
            // Read Prob automatas
            string[] automataFilesPathes = new string[numOfAutomatas];
            string startAutomataPath = "automata";
            string endAutomataPath = ".fsm";
            for(int i = 0; i < numOfAutomatas; i++)
            {
                automataFilesPathes[i] = startAutomataPath + i.ToString() + endAutomataPath;
            }

            PFSAParser pfsaParser = new PFSAParser(automataFilesPathes);
            List<PFSAutomata> pfsas = pfsaParser.GetAutomatas();
            
            // noramlized automatas
            foreach(PFSAutomata pfsa in pfsas)
            {
                pfsa.NormalizeAutoamta();
                pfsa.CompleteUnknownTransitions(48, 10);
            }

            // Run MDP Valueiteration 
            double epsilon = 0.5;
            MDPValueFunction mdpVf = new MDPValueFunction(maze);
            mdpVf.ValueIteration(epsilon);

            // Simulate trail using the pfsas
            MazeViewer viewer = new MazeViewer(maze);
            viewer.Start();
            maze.SimulatePolicyPfsa(mdpVf, 10, viewer, pfsas, cf);
            }
            // POMDP PROB AUTOMATA //


            //PointBasedValueIteration pbvi = new PointBasedValueIteration(maze);
            //pbvi.PointBasedVI(100, 20);

            //MDPValueFunction v = new MDPValueFunction(maze);
            //v.ValueIteration(0.5);

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