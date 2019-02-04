using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Djestit
using Unica.Djestit;

namespace Unica.Djestit.Feed
{
    // Enum che indica i diversi tipi di probabilità
    public enum ProbabilityType
    {
        IndipendentEvents = 1,
        Simple = 0
    }

    public class Likelihood
    {
        /* Attributi */
        public float probability { get; private set; }

        /* Costruttore */
        public Likelihood()
        {
            this.probability = 0.0f;
        }
        public Likelihood(float value)
        {
            this.probability = value;
        }
        public Likelihood(CompositeTerm term, ProbabilityType type)
        {
            switch (type)
            {
                case ProbabilityType.IndipendentEvents:
                    // Probabilità Composta Eventi Indipendenti
                    composite(term);
                    break;
            }
        }

        /* Metodi */
        /// <summary>
        /// Aggiorna la probabilità
        /// </summary>
        /// <param name="value"></param>
        public void updateLikelihood(float value)
        {
            // La probabilità di una gesture è ovviamente sempre compresa tra 0 e 1
            if ((value > 0) && (value < 1))
                this.probability = value;
            else if (value > 1)
                this.probability = 1f;
            else
                this.probability = 0;
        }

        //// <summary>
        /// Probabilità composta calcolata come il prodotto della probabilità dei singoli GroundTerm che 
        /// compongono la gesture
        /// </summary>
        /// <param name="node"></param>
        private void composite(FeedbackGesture node)
        {
            float tempLikelihood = 1;

            foreach (FeedbackLeaf child in node.groundTermChildren)
            {
                tempLikelihood = tempLikelihood * 0;//child.likelihood.probability;
            }

            // Inserisce la probabilità nel nodo
            this.probability = tempLikelihood;
        }
     
        private void composite(CompositeTerm term)
        {
            float tempLikelihood = 1;

            if ((term.GetType() == typeof(Sequence)) || (term.GetType() == typeof(Unica.Djestit.Parallel)) || (term.GetType() == typeof(Disabling)))
            {
                foreach (var child in term.Children())
                    tempLikelihood *= 0;// child.likelihood.probability;
            }
            else if(term.GetType() == typeof(OrderIndependece))
            {
                foreach (var child in term.Children())
                    //term.likelihood.probability *= child.likelihood.probability;
                    tempLikelihood *= term.ChildrenCount();
            }
            else if(term.GetType() == typeof(Iterative))
            {
                tempLikelihood = 0;// (float)Math.Pow(term.children[0].likelihood.probability, this.n);
            }
            else if(term.GetType() == typeof(Choice))
            {
                List<float> list= new List<float>();

                foreach (var child in term.Children()) 
                    list.Add(0);//child.likelihood.probability);

                tempLikelihood = list.Max();
            }

            this.probability = tempLikelihood;
        }
        
        /// <summary>
        /// Compara tra loro il valore di due probabilità. Serve per determinare quale 
        /// handler ha la probabilità più elevata di essere eseguito.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>                        
        public int CompareTo(Likelihood other)
        {
            return (other.probability.CompareTo(this.probability));
        }
    }
}
