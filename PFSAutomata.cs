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
                if(parsedLine.Length == 4)
                {
                    // extract key: state and observation, and value: next state and probability
                    int lineCurrentState = Convert.ToInt32(parsedLine[0]);
                    int lineNextState = Convert.ToInt32(parsedLine[1]);
                    int lineSymbol = Convert.ToInt32(parsedLine[2]);
                    double lineProb = Convert.ToDouble(parsedLine[3]);
                    int[] firstKey = new int[] { lineCurrentState, lineSymbol };
                    KeyValuePair<int, double> firstValue = new KeyValuePair<int, double>(lineNextState, lineProb);

                    // search if key is the dictionary, if so add to value list.
                    bool found = false;
                    foreach (KeyValuePair<int[], List<KeyValuePair<int, double>>> entry in probDictionary)
                    {
                        if (EqualIntArrays(entry.Key, firstKey))
                        {
                            found = true;
                            entry.Value.Add(firstValue);
                            break;
                        }
                    }

                    // if key is not inside the dictionary create the list and add the value.
                    if (!found)
                    {
                        List<KeyValuePair<int, double>> newList = new List<KeyValuePair<int, double>>();
                        newList.Add(firstValue);
                        probDictionary.Add(firstKey, newList);
                    }
                    
                }
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
            List<KeyValuePair<int, double>> nextStatesList = GetDictionaryValue(new int[] { currentState, symbol });
            double percentage = 0.0;
            foreach(KeyValuePair<int, double> kvp in nextStatesList)
            {
                percentage = percentage + kvp.Value;
                if(percentage >= r)
                {
                    currentState = kvp.Key;
                    break;
                }
               
            }
        }
        
        // add probabilitys for unknown state obs transition with uniform probability.
        public void CompleteUnknownTransitions(int numOfobs, int numOfStates)
        {
            for(int i = 0; i < numOfStates; i++)
            {
                for(int j = 0; j < numOfobs; j++)
                {
                    int[] currKey = new int[] { i, j };
                    bool found = false;

                    foreach (KeyValuePair<int[], List<KeyValuePair<int, double>>> entry in probDictionary)
                    {
                        if(EqualIntArrays(entry.Key, currKey))
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        List<KeyValuePair<int, double>> probList = new List<KeyValuePair<int, double>>();
                        double uProb = 1 / (double)numOfStates;

                        for (int k = 0; k < numOfStates; k++)
                        {
                            probList.Add(new KeyValuePair<int, double>(k, uProb));
                        }
                        probDictionary.Add(currKey, probList);
                    }
                }
            }
        }


        public int GetInitialValue()
        {
            int initialValue = this.ChoosePath();
            this.SetNextState(initialValue);
            return initialValue;
        }

        public int ChoosePath()
        {
            Random rand = new Random();
            double r = rand.NextDouble();
            double sumOne = 0;
            double sumZero = 0;
            List<KeyValuePair<int, double>> oneList = GetDictionaryValue(new int[] { currentState, 1 });
            List<KeyValuePair<int, double>> zeroList = GetDictionaryValue(new int[] { currentState, 0 });
            foreach(KeyValuePair<int, double> kvp in oneList)
            {
                sumOne = sumOne + kvp.Value;
            }

            foreach(KeyValuePair<int, double> kvp in zeroList)
            {
                sumZero = sumZero + kvp.Value;
            }

            // noramlize sums for selection
            double totalSum = sumOne + sumZero;
            sumZero = sumZero / totalSum;
            sumOne = sumOne / totalSum;
            if(sumZero > sumOne)
            {
                if(r > sumOne)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (r > sumZero)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
        public int GetAutomataResult(int symbol)
        {
            this.SetNextState(symbol);
            int nextSymbol = this.ChoosePath();
            this.SetNextState(nextSymbol);
            return nextSymbol;

        }

        private bool EqualIntArrays(int[] a, int[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

        private List<KeyValuePair<int, double>>  GetDictionaryValue(int[] keyCopy)
        {
            foreach (KeyValuePair<int[], List<KeyValuePair<int, double>>> entry in probDictionary)
            {
                if (EqualIntArrays(entry.Key, keyCopy))
                {
                    return entry.Value;
                }
            }
            return null;
        }

        public void NormalizeAutoamta()
        {
            Dictionary<int[], List<KeyValuePair<int, double>>> normedprobDictionary = new Dictionary<int[], List<KeyValuePair<int, double>>>();

            foreach (KeyValuePair<int[], List<KeyValuePair<int, double>>> entry in probDictionary)
            {
                if(entry.Key[1] == 0 || entry.Key[1] == 1)
                {
                    normedprobDictionary.Add(entry.Key, entry.Value);
                }
                else
                {
                    // sum all probabilitys
                    double probSum = 0.0;
                    List<KeyValuePair<int, double>> normedList = new List<KeyValuePair<int, double>>();

                    foreach (KeyValuePair<int, double> probTuple in entry.Value)
                    {
                        probSum = probSum + probTuple.Value;
                    }

                    // normalize them
                    foreach (KeyValuePair<int, double> probTuple in entry.Value)
                    {
                        normedList.Add(new KeyValuePair<int, double>(probTuple.Key, probTuple.Value / probSum));
                    }

                    //  sort list and update the new doctionary
                    normedList.Sort((x, y) => x.Value.CompareTo(y.Value));
                    normedprobDictionary.Add(entry.Key, normedList);
                }
                
            }
            //replace old dictionary with normed one.
            probDictionary = normedprobDictionary;
        }
    }
}
