using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace POMDP
{
    class MazeDomain : Domain
    {
        private int[,] m_aMaze;
        private int m_cX, m_cY;
        private int m_cStates;
        private int m_iGoalSquareX, m_iGoalSquareY;
        private List<State> m_lStates;
        public int Width { get { return m_cX; } }
        public int Height { get { return m_cY; } }
        public int XGoal { get { return m_iGoalSquareX; } }
        public int YGoal { get { return m_iGoalSquareY; } }

        public MazeDomain(string sFileName)
        {
            LoadMaze(sFileName);
            DiscountFactor = 0.8;
        }

        public override IEnumerable<State> States
        {
            get 
            {
                if (m_lStates == null)
                {
                    InitStateList();
                }
                return m_lStates;
            }
        }

        public override IEnumerable<Action> Actions
        {
            get 
            {
                yield return new MazeAction("TurnLeft");
                yield return new MazeAction("TurnRight");
                yield return new MazeAction("Forward");            
            }
        }

        public override IEnumerable<Observation> Observations
        {
            get 
            {
                int i = 0;
                for (i = 0; i < 16; i++)
                    yield return new MazeObservation(i);
            }
        }

        public override BeliefState InitialBelief
        {
            get 
            {
                BeliefState bsInitial = new BeliefState(this);
                foreach( State s in States)
                {
                    bsInitial[s] = 1.0 / m_cStates;
                }
                return bsInitial;
            }
        }

        public override double MaxReward
        {
            get { return 100; }
        }

        public void SimulatePolicy(Policy p, int cTrials, MazeViewer viewer)
        {
            int iTrial = 0;
            for (iTrial = 0; iTrial < cTrials; iTrial++)
            {
                SimulateTrial(p, viewer);
            }
        }

        public void SimulatePolicyPfsa(Policy p, int cTrials, MazeViewer viewer, List<PFSAutomata> pfsas, ConvertionFunction cf)
        {
            int iTrial = 0;
            for (iTrial = 0; iTrial < cTrials; iTrial++)
            {
                Debug.WriteLine("starting iteration number: " + iTrial.ToString());
                SimulateTrialPfsa(p, viewer, pfsas, cf);
            }
        }


        private void SimulateTrialPfsa(Policy p, MazeViewer viewer, List<PFSAutomata> pfsas, ConvertionFunction cf)
        {
            State sCurrent = GetInitalState(), sNext = null, automataCurrent = null;
            int stateBitsCounter = 0;
            int bitDiffCoutner = 0;
            Action a = null;
            Observation o = null;
            int numOfBits = pfsas.Count();
            int numberOfYBits = Convert.ToInt32(Math.Ceiling(Math.Log(Height, 2)));
            int[] bits = new int[numOfBits];
            viewer.CurrentState = (MazeState)sCurrent;

            // sample intialize *legal* state from automata: TODO: fix ilegal first state
            bool legalFirstState = false;
            while (!legalFirstState)
            {
                for (int i = 0; i < numOfBits; i++)
                {
                    bits[i] = pfsas[i].GetInitialValue();
                }
                MazeState initState = (MazeState)GetNextState(bits, numberOfYBits);
                if (!BlockedSquare(initState.X, initState.Y))
                {
                    legalFirstState = true;
                    automataCurrent = initState;
                }
            }
            
            while (!IsGoalState(sCurrent))
            {
                // increment state counter
                stateBitsCounter = stateBitsCounter + bits.Length;
                
                // perform simulation
                a = p.GetAction(automataCurrent);
                sNext = sCurrent.Apply(a);
                o = sNext.RandomObservation(a); 
                int nextStateIndex = cf.GetIndex(a, o);
                for(int i = 0; i < numOfBits; i++)
                {
                    bits[i] = pfsas[i].GetAutomataResult(nextStateIndex);
                }

                // calculate difference in bits real vs automata
                for(int idx = 0; idx < bits.Length; idx++)
                {
                    int realNextBit = Convert.ToInt32(sNext.GetBitValue(idx));
                    if (realNextBit != bits[idx])
                        bitDiffCoutner = bitDiffCoutner + 1;
                }
                // check if new state is legal.
                MazeState automataNew = (MazeState)GetNextState(bits, numberOfYBits);
                if(!BlockedSquare(automataNew.X, automataNew.Y))
                {
                    automataCurrent = automataNew;
                    sCurrent = sNext;
                }

                viewer.CurrentState = (MazeState)sCurrent;
                viewer.CurrentObservation = (MazeObservation)o;
                Thread.Sleep(500);

            }

            // print mean of bit differneces of all states
            double bitDiffMean = bitDiffCoutner / (double)stateBitsCounter;
            Debug.WriteLine("Mean of wrong bits: " + bitDiffMean.ToString());
        }
        private State GetNextState(int[] bits, int numOfYBits)
        {
            int directionNum = bits[0] + bits[1] * 2;
            int iY = 0;
            int iX = 0;
            int multyplier = 1;
            for(int i = 2; i < 2 + numOfYBits; i++)
            {
                iY = iY + bits[i] * multyplier;
                multyplier *= 2;
            }
            multyplier = 1;
            for (int i = 2 + numOfYBits; i < bits.Length; i++)
            {
                iX = iX + bits[i] * multyplier;
                multyplier *= 2;
            }

            return new MazeState(iX, iY, (POMDP.MazeState.Direction)Enum.ToObject(typeof(POMDP.MazeState.Direction), directionNum), this);
        }


        private void SimulateTrial(Policy p, MazeViewer viewer)
        {
            BeliefState bsCurrent = InitialBelief, bsNext = null;
            State sCurrent = bsCurrent.RandomState(), sNext = null;
            Action a = null;
            Observation o = null;
            viewer.CurrentState = (MazeState)sCurrent;
            viewer.CurrentBelief = bsCurrent;
            while (!IsGoalState(sCurrent))
            {
                a = p.GetAction(bsCurrent);
                sNext = sCurrent.Apply(a);
                o = sNext.RandomObservation(a);
                bsNext = bsCurrent.Next(a, o);
                bsCurrent = bsNext;
                sCurrent = sNext;
                viewer.CurrentState = (MazeState)sCurrent;
                viewer.CurrentBelief = bsCurrent;
                viewer.CurrentObservation = (MazeObservation)o;
                Thread.Sleep(500);
            }
        }

        public bool IsTargetSqaure(MazeState ms)
        {
            return ms.X == m_iGoalSquareX && ms.Y == m_iGoalSquareY;
        }

        public bool IsTargetSqaure(int iX, int iY)
        {
            return iX == m_iGoalSquareX && iY == m_iGoalSquareY;
        }

        public override bool IsGoalState(State s)
        {
            MazeState ms = (MazeState)s;
            return ms.X == m_iGoalSquareX && ms.Y == m_iGoalSquareY;
        }

        private void LoadMaze(string sMazeFile)
        {
            StreamReader sr = new StreamReader(sMazeFile);
            string sLine = sr.ReadLine();
            m_cX = int.Parse(sLine.Split(',')[0]);
            m_cY = int.Parse(sLine.Split(',')[1]);
            m_aMaze = new int[m_cX, m_cY];
            int iX = 0, iY = 0;
            m_cStates = m_cX * m_cY * 4 + 1;
            for (iY = 0; iY < m_cY; iY++)
            {
                sLine = sr.ReadLine();
                for (iX = 0; iX < m_cX; iX++)
                {
                    if (sLine[iX] == 'X')
                    {
                        m_aMaze[iX, iY] = -1;
                        m_cStates -= 4;
                    }
                    else if (sLine[iX] == 'G')
                    {
                        m_iGoalSquareX = iX;
                        m_iGoalSquareY = iY;
                    }
                    else
                    {
                        m_aMaze[iX, iY] = int.Parse("" + sLine[iX]);
                    }
                }
            }
        }

        public bool BlockedSquare(int iX, int iY)
        {
            if( iX >= 0 && iX < m_cX && iY >= 0 && iY < m_cY)
                return m_aMaze[iX,iY] == -1;
            return true;
        }

        private bool WallInDirection(int iX, int iY, MazeState.Direction d)
        {
            if (d == MazeState.Direction.South)
                return BlockedSquare(iX, iY + 1);
            if (d == MazeState.Direction.North)
                return BlockedSquare(iX, iY - 1);
            if (d == MazeState.Direction.East)
                return BlockedSquare(iX + 1, iY);
            if (d == MazeState.Direction.West)
                return BlockedSquare(iX - 1, iY);
            return false;
        }

        public MazeObservation GetWallConfiguration(MazeState s)
        {
            MazeState.Direction d = s.CurrentDirection;
            int iObservation = 0;
            int iDirection = 0;
            for (iDirection = 0; iDirection < 4; iDirection++)
            {
                iObservation *= 2;
                if (WallInDirection(s.X, s.Y, d))
                    iObservation++;
                d = s.TurnRight(d);
            }
            return new MazeObservation(iObservation);
        }

        private void InitStateList()
        {
            m_lStates = new List<State>();
            int iX = 0, iY = 0;
            for (iX = 0; iX < m_cX; iX++)
            {
                for (iY = 0; iY < m_cY; iY++)
                {
                    if (m_aMaze[iX, iY] >= 0) //-1 denotes a blocked state
                    {
                        m_lStates.Add(new MazeState(iX, iY, MazeState.Direction.East, this));
                        m_lStates.Add(new MazeState(iX, iY, MazeState.Direction.South, this));
                        m_lStates.Add(new MazeState(iX, iY, MazeState.Direction.North, this));
                        m_lStates.Add(new MazeState(iX, iY, MazeState.Direction.West, this));
                    }
                }
            }
        }

        public override State GetState(int iStateIdx)
        {
            if (m_lStates == null)
                InitStateList();
            return m_lStates[iStateIdx];
        }

        // randomize first state among 3 first state in states list - for know.
        public override State GetInitalState()
        {
            Random rand = new Random();
            int stateIdx = rand.Next(0, 3);
            if (m_lStates == null)
                InitStateList();
            return m_lStates.ElementAt(stateIdx);
        }


    }
}
