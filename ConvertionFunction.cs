using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POMDP
{
    class ConvertionFunction
    {
        private int boardSize, numOfActions, numOfObservation;
        private int range;
        private Dictionary<int[], int> functionMapping = new Dictionary<int[], int>();
        public ConvertionFunction(int boardSize, int numOfActions, int numOfObservation)
        {
            this.boardSize = boardSize;
            this.numOfActions = numOfActions;
            this.numOfObservation = numOfObservation;
            range = numOfActions * numOfObservation * (int)Math.Log(boardSize);
        }
    }
}
