using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POMDP
{
    class PFSAutomata
    {
        private Dictionary<int[], List<KeyValuePair<int, double>>> probDictionary = new Dictionary<int[], List<KeyValuePair<int, double>>>();
        private int currentState;

        public PFSAutomata(string[] lines)
        {
            foreach(string line in lines)
            {
                string[] parsedLine = line.Split(' ');
                int lineCurrentState = Int32.Parse(parsedLine[0]);
                int lineNextState = Int32.Parse(parsedLine[1]);
                int lineSymbol = Int32.Parse(parsedLine[2]);
                int lineProb = Int32.Parse(parsedLine[3]);
                int[] firstKey = new int[] { lineCurrentState, lineSymbol };
                KeyValuePair<int, double> firstValue = new KeyValuePair<int, double>(lineNextState, lineProb);
                if (probDictionary.ContainsKey(firstKey))
                {
                    probDictionary.Add(firstKey, new List<KeyValuePair<int, double>>());
                }
                probDictionary[firstKey].Add(firstValue);
            }
        }

        public PFSAutomata(Dictionary<int[], List<KeyValuePair<int, double>>> probDic)
        {
            probDictionary = probDic;
            currentState = 0;
        }

        public int GetCurrentState()
        {
            return currentState;
        }

        public void SetNextState(int symbol)
        {
            Random rand = new Random();
            double r = rand.NextDouble();
            List<KeyValuePair<int, double>> nextStatesList = probDictionary[new int[] { currentState, symbol }];
            nextStatesList.Sort((x, y) => y.Value.CompareTo(x.Value));
            int lastState = nextStatesList[0].Key;
            foreach(KeyValuePair<int, double> kvp in nextStatesList)
            {
                if(kvp.Value > r)
                {
                    currentState = lastState;
                    break;
                }
                lastState = kvp.Key;
            }
        }
    }
}
