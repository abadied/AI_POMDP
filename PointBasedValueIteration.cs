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
        private Dictionary<BeliefState, KeyValuePair<AlphaVector, double>> m_valueFunction; 


        public PointBasedValueIteration(Domain d)
        {
            m_dDomain = d;
        }

        public override Action GetAction(BeliefState bs)
        {
            AlphaVector avBest = null;
            ValueOf(bs, m_lVectors, out avBest);
            return avBest.Action;
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
                if (dValue > dMaxValue) {
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
            this.m_lVectors = new List<AlphaVector>(this.m_dDomain.Actions.Count());
            foreach (var action in this.m_dDomain.Actions)
            {
                AlphaVector curr = new AlphaVector(action);
                foreach (State state in this.m_dDomain.States)
                {
                    curr[state] = state.Reward(action);
                }
                m_lVectors.Add(curr);
            }
            initialValueFunction(setBeliefStates);
            for (int i = 0; i < cMaxIterations-1; i++)
            {
                pruneAlphaVector(setBeliefStates);
            }

            //initialize all alphavectors with zeros
            //create value function (size of beliefstates) initialize with zeros
            //while condition holds for each belief state call backup and update value function asd follow -> VF(i) += gamma * B_i * Alpha_i 
        }
        private void pruneAlphaVector(List<BeliefState> bsSet)
        {
            List<BeliefState> copyBset = new List<BeliefState>(bsSet);
            while (copyBset.Any())
            {
                BeliefState _bs = copyBset.First();
                AlphaVector _alpha = backup(_bs);
                double _reward = _alpha.InnerProduct(_bs);
                if (this.m_valueFunction[_bs].Value < _reward)
                {
                    this.m_valueFunction[_bs] = new KeyValuePair<AlphaVector, double>(_alpha, _reward);
                    copyBset.Remove(_bs);
                    while(copyBset.Any())
                    {
                        BeliefState __bs = copyBset.First();
                        double __reward = _alpha.InnerProduct(__bs);
                        if (this.m_valueFunction[__bs].Value < __reward)
                        {
                            this.m_valueFunction[__bs] = new KeyValuePair<AlphaVector, double>(_alpha, __reward);
                            copyBset.Remove(__bs);
                        }
                    }
                }
                else
                {
                    copyBset.Remove(_bs);
                    foreach (var kvp in m_valueFunction.Values)
                    {
                        AlphaVector __alpha = kvp.Key;
                        double reward = __alpha.InnerProduct(_bs);
                        if (reward > kvp.Value)
                            this.m_valueFunction[_bs] = new KeyValuePair<AlphaVector, double>(__alpha, reward);
                    }
                }
            }
        }
        private void initialValueFunction(List<BeliefState> bsSet)
        {
            foreach (var bs in bsSet)
            {
                this.m_valueFunction.Add(bs, new KeyValuePair<AlphaVector, double>());
                bool firstAlpha = true;
                foreach (var alpha in this.m_lVectors)
                {
                    if (firstAlpha)
                    {
                        this.m_valueFunction[bs] = new KeyValuePair<AlphaVector, double>(alpha, Double.NegativeInfinity);
                        firstAlpha = false;
                    }
                    else
                    {
                        Action _action = alpha.Action;
                        double _reward = bs.Reward(_action);
                        if (this.m_valueFunction[bs].Value < _reward)
                            this.m_valueFunction[bs] = new KeyValuePair<AlphaVector, double>(alpha, _reward);
                    }
                }

            }
        }
    }
}
