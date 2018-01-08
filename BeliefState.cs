using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace POMDP
{
    class BeliefState
    {
        private Dictionary<State,double> m_dBeliefs;
        private Domain m_dDomain;

        public double this[State s]
        {
            get 
            { 
                if(m_dBeliefs.ContainsKey(s))    
                    return m_dBeliefs[s];
                return 0.0;
            }
            set { m_dBeliefs[s] = value; }
        }

        public IEnumerable<KeyValuePair<State, double>> Beliefs(double dMin)
        {
            foreach (KeyValuePair<State, double> p in m_dBeliefs)
                if (p.Value >= dMin)
                    yield return p;
        }

        public BeliefState(Domain d)
        {
            m_dDomain = d;
            m_dBeliefs = new Dictionary<State, double>();
        }

        private void AddBelief(State s, double dProb)
        {
            if (!m_dBeliefs.ContainsKey(s))
                m_dBeliefs[s] = 0;
            m_dBeliefs[s] += dProb;
        }

        public BeliefState Next(Action a, Observation o)
        {
            BeliefState bsNext = new BeliefState(m_dDomain);
            //your code here
            double normalizationFactor = 0.0;
            foreach (State stateForUpdate in m_dDomain.States)
            {
                double updateProbabilityForState = 0.0;
                foreach (State state in stateForUpdate.Successors(a)) //or in States????????????
                {
                    double transitionProbability = state.TransitionProbability(a: a, sTag: stateForUpdate);
                    double beliefOfstate = this.m_dBeliefs[state];
                    updateProbabilityForState += transitionProbability * beliefOfstate;
                }
                updateProbabilityForState *= stateForUpdate.ObservationProbability(a: a, o: o);
                bsNext.AddBelief(stateForUpdate, updateProbabilityForState);
                normalizationFactor += updateProbabilityForState;
            }
            foreach (State stateToNormalize in bsNext.m_dBeliefs.Keys)
                bsNext.m_dBeliefs[stateToNormalize] /= normalizationFactor;
            Debug.Assert(bsNext.Validate());
            return bsNext;
        }

       

        public override string ToString()
        {
            string s = "<";
            foreach (KeyValuePair<State, double> p in m_dBeliefs)
            {
                if( p.Value > 0.01 )
                    s += p.Key + "=" + p.Value.ToString("F") + ",";
            }
            s += ">";
            return s;
        }

        public bool Validate()
        {
            //validate that every state appears at most once
            List<State> lStates = new List<State>(m_dBeliefs.Keys);
            int i = 0, j = 0;
            for (i = 0; i < lStates.Count; i++)
            {
                for (j = i + 1; j < lStates.Count; j++)
                {
                    if (lStates[i].Equals(lStates[j]))
                        return false;
                }
            }
            double dSum = 0.0;
            foreach (double d in m_dBeliefs.Values)
                dSum += d;
            if (Math.Abs(1.0 - dSum) > 0.001)
                return false;
            return true;
        }

        public double Reward(Action a)
        {
            double dSum = 0.0;
            foreach (KeyValuePair<State, double> p in m_dBeliefs)
            {
                dSum += p.Value * p.Key.Reward(a);
            }
            return dSum;
        }

        public State RandomState()
        {
            State to_ret = null;
            Random rnd = new Random();
            int i = rnd.Next(0, this.m_dBeliefs.Keys.Count);
            foreach (var state in this.m_dBeliefs.Keys)
            {
                if (i == 0)
                    to_ret = state;
                else
                    i -= 1;
            }
            return to_ret;
        }
    }
}
