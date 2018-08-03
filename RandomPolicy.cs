using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POMDP
{
    class RandomPolicy : Policy
    {
        private List<Action> m_lActions;

        // dictionary to save number of actions got for every state
        private Dictionary<State, Dictionary<Action, int>> actionCoutner = new Dictionary<State, Dictionary<Action, int>>();

        public RandomPolicy(Domain d)
        {
            m_lActions = new List<Action>();
            foreach (Action a in d.Actions)
            {
                m_lActions.Add(a);
            }
        }


        public override Action GetAction(BeliefState bs)
        {
            int idx = RandomGenerator.Next(m_lActions.Count);
            return m_lActions[idx];
        }

        public override Action GetRandAction(State s)
        {
            int idx = RandomGenerator.Next(m_lActions.Count);
            return m_lActions[idx];
        }


        // more sophisticated policy for exploration
        public override Action GetAction(State s)
        {
            bool found = false;
            State stKey = null;
            Dictionary<Action, int> actionDict = null;

            foreach (State st in actionCoutner.Keys)
            {
                if (st.Equals(s))
                {
                    found = true;
                    stKey = st;
                    break;
                }
            }

            if (!found)
            {
                stKey = s;
                actionDict = new Dictionary<Action, int>();
                foreach(Action a in m_lActions)
                {
                    actionDict.Add(a, 0);
                }
                actionCoutner.Add(stKey, actionDict);
            }
            else
            {
                actionDict = actionCoutner[stKey];
            }

            // choose min action to return and update counter

            Action minAction = null;
            int minCounter = Int32.MaxValue;

            foreach(KeyValuePair<Action, int> entry in actionDict)
            {
                if(entry.Value < minCounter)
                {
                    minCounter = entry.Value;
                    minAction = entry.Key;
                }
            }

            actionDict[minAction] = minCounter + 1;

            return minAction;
        }
    }
}
