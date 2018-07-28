using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POMDP
{
    class PFSAParser
    {
        public string[] automatasPathes;

        public PFSAParser(string[] pathes)
        {
            automatasPathes = pathes;
        }

        public List<PFSAutomata> GetAutomatas()
        {
            List<PFSAutomata> pfsas = new List<PFSAutomata>();
            foreach (string path in automatasPathes)
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                PFSAutomata automata = new PFSAutomata(lines);
                pfsas.Add(automata);
            }
            return pfsas;
        }

    }
}
