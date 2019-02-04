using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Djestit
using Unica.Djestit;

namespace Unica.Djestit.Feed
{
    public static class ComputeLikelihood
    {
        /* Attributi */
        private static int n = 10;

        /* Metodi */
        /// <summary>
        /// Aggiorna la probabilità
        /// </summary>
        /// <param name="value"></param>
        public static float updateLikelihood(float value)
        {
            // La probabilità di una gesture è ovviamente sempre compresa tra 0 e 1
            if ((value > 0) && (value < 1))
                return value;
            else if (value > 1)
                return 1f;
            else
                return 0;
        }

        //// <summary>
        /// Probabilità composta calcolata come il prodotto della probabilità dei singoli GroundTerm che 
        /// compongono la gesture
        /// </summary>
        /// <param name="node"></param>  
        public static float indipendentEvents(CompositeTerm term)
        {
            float tempLikelihood = 1;

            if ((term.GetType() == typeof(Sequence)) || (term.GetType() == typeof(Unica.Djestit.Parallel)) || (term.GetType() == typeof(Disabling)))
            {
                foreach (var child in term.Children())
                    tempLikelihood *= child.likelihood;
            }
            else if(term.GetType() == typeof(OrderIndependece))
            {
                foreach (var child in term.Children())
                    tempLikelihood *= child.likelihood;
                tempLikelihood *= term.ChildrenCount();
            }
            else if(term.GetType() == typeof(Iterative))
            {
                tempLikelihood = (float)Math.Pow(term.GetChild(0).likelihood, n);
            }
            else if(term.GetType() == typeof(Choice))
            {
                List<float> list= new List<float>();

                foreach (var child in term.Children()) 
                    list.Add(child.likelihood);

                tempLikelihood = list.Max();
            }

            return tempLikelihood;
        }
    }
}
