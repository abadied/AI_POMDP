using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace POMDP
{
    abstract class Domain
    {
        public abstract IEnumerable<State> States { get; }
        public abstract IEnumerable<Action> Actions { get; }
        public abstract IEnumerable<Observation> Observations { get; }
        public abstract BeliefState InitialBelief { get; }
        public abstract double MaxReward { get; }
        public abstract bool IsGoalState(State s);
        public abstract State GetState(int iStateIdx);
        public double DiscountFactor { get; protected set; }
        public double ComputeAverageDiscountedReward(Policy p, int cTrials, int cStepsPerTrial)
        {
            //your code here
            double to_ret = 0.0;
            List<double> rewards = new List<double>();
            for (int i = 0; i < cTrials; i++)
            {
                State target = sampleInitialState();
                BeliefState currentBeliefState = InitialBelief;
                double sumRewards = 0.0;
                int counter = 0;
                while ((!IsGoalState(target) && counter<cStepsPerTrial))
                {
                    Action a = p.GetAction(InitialBelief);
                    #region
                    /*
                    List<State> states = (List<State>)initialState.Successors(a);
                    List<KeyValuePair<State, double>> probabilitiesForNewStates = new List<KeyValuePair<State, double>>();
                    foreach (State state in states)
                    {
                        double prob = initialState.TransitionProbability(a: a, sTag: state);
                        probabilitiesForNewStates.Add(new KeyValuePair<State, double>(state, prob));
                    }
                    State newState = sampling(probabilitiesForNewStates);
                    */
                    #endregion
                    State newState = target.Apply(a: a);
                    List<KeyValuePair<Observation, double>> probabilitiesForObservation = new List<KeyValuePair<Observation, double>>();
                    foreach (Observation obs in Observations)
                    {
                        double prob = newState.ObservationProbability(a: a, o: obs);
                        probabilitiesForObservation.Add(new KeyValuePair<Observation, double>(obs, prob));
                    }
                    Observation newObservation = samplingObservations(probabilitiesForObservation);
                    BeliefState updatedBeliefState = currentBeliefState.Next(a: a, o: newObservation);
                    double reward = updatedBeliefState.Reward(a);
                    sumRewards += reward*Math.Pow(DiscountFactor,counter);
                    counter++;
                }
                rewards.Add(sumRewards);
            }

            foreach (double r in rewards)
            {
                to_ret += r;
            }
            return (to_ret) / cTrials;
        }
        //sample state from initial belief state
        private State sampleInitialState()
        {
            List<KeyValuePair<State, double>> states_probabilities = new List<KeyValuePair<State, double>>();
            double cumulativeProbs = 0.0;
            foreach (State state in States)
            {
                cumulativeProbs += InitialBelief[state];
                states_probabilities.Add(new KeyValuePair<State, double>(state, cumulativeProbs));
            }
            return samplingStates(states_probabilities);
            
        }

        private static State samplingStates(List<KeyValuePair<State, double>> states_probabilities)
        {
            Random random = new Random();
            State to_ret = null;
            double rnd = random.NextDouble();
            foreach (KeyValuePair<State, double> kp in states_probabilities)
            {
                if (rnd <= kp.Value)
                {
                    to_ret = kp.Key;
                    break;
                }
            }
            return to_ret;
        }

        private static Observation samplingObservations(List<KeyValuePair<Observation, double>> states_probabilities)
        {
            Random random = new Random();
            Observation to_ret = null;
            double rnd = random.NextDouble();
            foreach (KeyValuePair<Observation, double> kp in states_probabilities)
            {
                if (rnd <= kp.Value)
                {
                    to_ret = kp.Key;
                    break;
                }
            }
            return to_ret;
        }

    }
}

