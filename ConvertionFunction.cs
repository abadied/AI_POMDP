using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POMDP
{
    class ConvertionFunction
    {
        private Dictionary<string, int> functionMapping = new Dictionary<string, int>();
        public ConvertionFunction(List<MazeAction> Actions, List<MazeObservation> Observations)
        {
            int numOfActions = Actions.Count();
            int numOfObservation = Observations.Count();
            int obsIndex = 0;
            foreach(MazeAction action in Actions)
            {
                foreach(MazeObservation obs in Observations)
                {
                    List<string> observationString = obs.getObservationString();
                    observationString.Add(action.ToString());
                    functionMapping.Add(string.Join(" ", observationString.ToArray()), obsIndex);
                    obsIndex++;
                }
            }
        }

        public int getIndex(MazeAction action, MazeObservation obs)
        {
            List<string> key = obs.getObservationString();
            key.Add(action.ToString());
            return functionMapping[string.Join(" ", key.ToArray())];
        }
    }
}
