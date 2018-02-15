Eden Abadi - 305554917
Adar Vit - 305004186

Functions:


class PointBasedValueIteration

PointBasedValueIteration(Domain d):
This function is the constructor of the PointBasedValueIteration class, we added the this constructor a new parameter to represent the value funtion -m_valueFunction = new Dictionary<BeliefState, AlphaVector>
 
AlphaVector backup(BeliefState bs):
This fucntion is the backup function as in persues algorithm, it will return the best alpha vector for the given belief state according to the new policy which simulates a tree at depth n + 1.

void PointBasedVI(int cBeliefs, int cMaxIterations):
This function is the pointbased value iteration algorithm as written in presues algorithm.it will generate a set of alpha vectors accroding to the best value function calculated during cmaxiterations, 
those vectors represent a policy for each given belief state.

void pruneAlphaVector(List<BeliefState> bsSet):
As in persues algorithm the prune function is responsible of removing alpha vectors that is no longer needed - they are dominated by other alpha vector.

void initialValueFunction(List<BeliefState> bsSet):
We added this function in order to initiate our value function parameter to deafault values(starting values of alpha vectors for each belief state taken in considration).


class BeliefState

BeliefState Next(Action a, Observation o):
Returns the updated belief state given action a and observation o.

State RandomState():
This function generates a random state given according to relavent probabilities.

class Domain

double ComputeAverageDiscountedReward(Policy p, int cTrials, int cStepsPerTrial):
This function simulates cTrials times the use of given policy (in our case the policy from point base value iteration) and retuirns the mean value of all simulations.

State sampleInitialState():
This function sample state from initial belief state according to the given probabilities in it.
 
State samplingStates(List<KeyValuePair<State, double>> states_probabilities):
This function is a side function to the one above(sampleInitialState), sample states according to belief state probabilities.

Observation samplingObservations(List<KeyValuePair<Observation, double>> observations_probabilities)
This function sample observation from observation key value list according to the given probabilities in it.

class Program

static void Main(string[] args):
main function to run the program.
