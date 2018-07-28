using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POMDP
{
    class ConvertionFunction
    {
        private Dictionary<string, int> functionMapping = new Dictionary<string, int>();
        public ConvertionFunction(List<Action> Actions, List<Observation> Observations)
        {
            int numOfActions = Actions.Count();
            int numOfObservation = Observations.Count();
            int obsIndex = 2; // 0 and 1 are for relevant bit
            foreach(Action action in Actions)
            {
                foreach(Observation obs in Observations)
                {
                    List<string> observationString = obs.getObservationString();
                    observationString.Add(action.ToString());
                    functionMapping.Add(string.Join(" ", observationString.ToArray()), obsIndex);
                    obsIndex++;
                }
            }
        }

        public int GetIndex(Action action, Observation obs)
        {
            List<string> key = obs.getObservationString();
            key.Add(action.ToString());
            return functionMapping[string.Join(" ", key.ToArray())];
        }
    }
}
