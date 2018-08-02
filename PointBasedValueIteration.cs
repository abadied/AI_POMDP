using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace POMDP
{
    class PointBasedValueIteration : Policy
    {

        private Domain m_dDomain;
        private List<AlphaVector> m_lVectors;
        private Dictionary<AlphaVector, Dictionary<Action, Dictionary<Observation, AlphaVector>>> m_dGCache;
        private Dictionary<BeliefState, AlphaVector> m_valueFunction;


        public PointBasedValueIteration(Domain d)
        {
            m_dDomain = d;
            m_valueFunction = new Dictionary<BeliefState, AlphaVector>();
            m_dGCache = new Dictionary<AlphaVector, Dictionary<Action, Dictionary<Observation, AlphaVector>>>();
        }

        public override Action GetAction(BeliefState bs)
        {
            AlphaVector avBest = null;
            ValueOf(bs, m_lVectors, out avBest);
            return avBest.Action;
        }

        public override Action GetRandAction(State s)
        {
            //your code here
            throw new NotImplementedException();
        }

        public override Action GetAction(State s)
        {
            //your code here
            throw new NotImplementedException();
        }

        private AlphaVector G(Action a, Observation o, AlphaVector av)
        {
            if (!m_dGCache.ContainsKey(av))
                m_dGCache[av] = new Dictionary<Action, Dictionary<Observation, AlphaVector>>();
            if (!m_dGCache[av].ContainsKey(a))
                m_dGCache[av][a] = new Dictionary<Observation, AlphaVector>();
            if (m_dGCache[av][a].ContainsKey(o))
                return m_dGCache[av][a][o];
            AlphaVector avNew = new AlphaVector(a);
            foreach (State s in m_dDomain.States)
            {
                double dSum = 0.0;
                foreach (State sTag in m_dDomain.States)
                {
                    dSum += sTag.ObservationProbability(a, o) * s.TransitionProbability(a, sTag) * av[sTag];

                }
                avNew[s] = dSum;
            }
            m_dGCache[av][a][o] = avNew;
            return avNew;
        }
        private AlphaVector G(BeliefState bs, Action a)
        {
            AlphaVector avSum = new AlphaVector(a);
            AlphaVector avGMax = null;
            double dValue = 0.0, dMaxValue = double.NegativeInfinity;
            foreach (Observation o in m_dDomain.Observations)
            {
                dMaxValue = double.NegativeInfinity;
                avGMax = null;
                foreach (AlphaVector avCurrent in m_lVectors)
                {
                    AlphaVector avG = G(a, o, avCurrent);
                    dValue = avG.InnerProduct(bs);
                    if (dValue > dMaxValue)
                    {
                        dMaxValue = dValue;
                        avGMax = avG;
                    }
                }
                avSum += avGMax;
            }
            avSum *= m_dDomain.DiscountFactor;
            AlphaVector avResult = new AlphaVector(a);
            foreach (State s in m_dDomain.States)
            {
                avResult[s] = avSum[s] + s.Reward(a);
            }
            return avResult;
        }
        private AlphaVector backup(BeliefState bs)
        {
            AlphaVector avBest = null, avCurrent = null;
            double dMaxValue = double.NegativeInfinity, dValue = 0.0;

            //your code here
            foreach (var action in m_dDomain.Actions)
            {
                avCurrent = G(bs, action);
                dValue = avCurrent.InnerProduct(bs);
                if (dValue > dMaxValue)
                {
                    dMaxValue = dValue;
                    avBest = avCurrent;
                }
            }
            return avBest;
        }

        private List<BeliefState> SimulateTrial(Policy p, int cMaxSteps)
        {
            BeliefState bsCurrent = m_dDomain.InitialBelief, bsNext = null;
            State sCurrent = bsCurrent.RandomState(), sNext = null;
            Action a = null;
            Observation o = null;
            List<BeliefState> lBeliefs = new List<BeliefState>();
            while (!m_dDomain.IsGoalState(sCurrent) && lBeliefs.Count < cMaxSteps)
            {
                a = p.GetAction(bsCurrent);
                sNext = sCurrent.Apply(a);
                o = sNext.RandomObservation(a);
                bsNext = bsCurrent.Next(a, o);
                bsCurrent = bsNext;
                lBeliefs.Add(bsCurrent);
                sCurrent = sNext;
            }
            return lBeliefs;
        }
        private List<BeliefState> CollectBeliefs(int cBeliefs)
        {
            Debug.WriteLine("Started collecting " + cBeliefs + " points");
            RandomPolicy p = new RandomPolicy(m_dDomain);
            int cTrials = 100, cBeliefsPerTrial = cBeliefs / cTrials;
            List<BeliefState> lBeliefs = new List<BeliefState>();
            while (lBeliefs.Count < cBeliefs)
            {
                lBeliefs.AddRange(SimulateTrial(p, cBeliefsPerTrial));
            }
            Debug.WriteLine("Collected " + lBeliefs.Count + " points");
            return lBeliefs;
        }

        private double ValueOf(BeliefState bs, List<AlphaVector> lVectors, out AlphaVector avBest)
        {
            double dValue = 0.0, dMaxValue = double.NegativeInfinity;
            avBest = null;
            foreach (AlphaVector av in lVectors)
            {
                dValue = av.InnerProduct(bs);
                if (dValue > dMaxValue)
                {
                    dMaxValue = dValue;
                    avBest = av;
                }
            }
            return dMaxValue;
        }

        public void PointBasedVI(int cBeliefs, int cMaxIterations)
        {
            // your code here
            List<BeliefState> setBeliefStates = CollectBeliefs(cBeliefs);
            this.m_lVectors = new List<AlphaVector>(this.m_dDomain.States.Count());
            foreach (BeliefState bs in setBeliefStates)
            {
                m_lVectors.Add(new AlphaVector());
            }
             List<AlphaVector> _m_lVectors = new List<AlphaVector>();
            foreach (BeliefState bs in setBeliefStates)
            {
                AlphaVector curr = backup(bs);
                _m_lVectors.Add(curr);
            }
            this.m_lVectors = new List<AlphaVector>(_m_lVectors);
            initialValueFunction(setBeliefStates);
            for (int i = 0; i < cMaxIterations; i++)
            {
                pruneAlphaVector(setBeliefStates);
            }

        }
        private void pruneAlphaVector(List<BeliefState> bsSet)
        {
            List<BeliefState> copyBset = new List<BeliefState>(bsSet);
            List<AlphaVector> temp_lVectors = new List<AlphaVector>();
            while (copyBset.Any())
            {
                BeliefState _bs = copyBset.ElementAt(0);
                AlphaVector _alpha = backup(_bs);
                double _reward = _alpha.InnerProduct(_bs);
                if (this.m_valueFunction[_bs].InnerProduct(_bs) < _reward)
                {
                    this.m_valueFunction[_bs] = _alpha;
                    temp_lVectors.Add(_alpha);
                    copyBset.Remove(_bs);
                    List<BeliefState> copyBset_inner = new List<BeliefState>(copyBset);
                    foreach (BeliefState temp_bs in copyBset_inner)
                    {
                        double __reward = _alpha.InnerProduct(temp_bs);
                        double curr_val = this.m_valueFunction[temp_bs].InnerProduct(temp_bs);
                        if (curr_val < __reward)
                        {
                            this.m_valueFunction[temp_bs] = _alpha;
                            copyBset.Remove(temp_bs);
                        }

                    }
                }

                else
                {
                    copyBset.Remove(_bs);
                    double max_reward = double.NegativeInfinity;
                    AlphaVector max_alpha = null;
                    foreach (AlphaVector alpha in m_lVectors)
                    {
                        double reward = alpha.InnerProduct(_bs);
                        if (reward > max_reward)
                        {
                            max_reward = reward;
                            max_alpha = alpha;
                        }
                    }
                    if(!temp_lVectors.Contains(max_alpha))
                        temp_lVectors.Add(max_alpha);
                    this.m_valueFunction[_bs] = max_alpha;

                }
            }
            //this.m_lVectors = new List<AlphaVector>();
            this.m_lVectors = temp_lVectors;
            //foreach (AlphaVector updated_alpha in m_valueFunction.Values)
            //{
             //   if(!this.m_lVectors.Contains(updated_alpha))
              //      this.m_lVectors.Add(updated_alpha);
            //}
                                  
        }
        private void initialValueFunction(List<BeliefState> bsSet)
        {
            foreach (var bs in bsSet)
            {
                
                bool firstAlpha = true;
                foreach (var alpha in this.m_lVectors)
                {
                    if (firstAlpha)
                    {
                        this.m_valueFunction.Add(bs, alpha);
                        firstAlpha = false;
                    }
                    else
                    {
                        Action _action = alpha.Action;
                        double _reward = bs.Reward(_action);
                        if (this.m_valueFunction[bs].InnerProduct(bs) < _reward)
                            this.m_valueFunction[bs] =alpha;
                    }
                }
            }
        }
    }
}