using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ErrorTolerance
using Unica.Djestit.ErrorToleranceManager;
using Unica.Djestit.Feed;

namespace Unica.Djestit
{
    // Quando viene rilevato e tollerato un movimento errato dell'utente
    public delegate void GestureErrorTolerance();

    public class CompositeTerm : Term
    {
        /* Attributi */
        // Indice dei figli
        protected int index = 0;
        // Evento che descrive quando viene rilevato e gestito un errore nei movimento dell'utente
        public event GestureErrorTolerance ErrorDetect;
        // Contiene la lista di operandi da gestire
        protected List<Term> children;
        // Indica il massimo numero di errori tollerabili
        protected const int deltaError = 0;
        // Variabile che indica il numero di errori commessi
        private ErrorTolerance error_tolerance = null;
        // Flag che indica se il CompositeTerm ha l'oggetto ErrorTolerance settato
        public bool flagErrTolerance { private set; get; }

        /* Costruttore */
        public CompositeTerm()
        {
            children = new List<Term>();
        }

        public CompositeTerm(List<Term> terms) 
            : this()
        {
            // Ad ogni figlio in input provvede a settare il puntatore al padre.
            foreach(Term term in terms)
            {
                term.pointFather = this;
                children.Add(term);
            }
            ComputeLikelihood.indipendentEvents(this);
        }
        public CompositeTerm(Term term)
            : this()
        {
            // Associa al figlio in input il puntatore al padre
            term.pointFather = this;
            children.Add(term);
            ComputeLikelihood.indipendentEvents(this);
        }

        /* Metodi */
        /// <summary>
        /// Adds a child to the composite term
        /// </summary>
        /// <param name="child">the child term</param>
        public void AddChild(Term child)
        {
            this.children.Add(child);
            child.pointFather = this;
        }

        /// <summary>
        /// Iterates over the children array
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Term> Children()
        {
            foreach(Term t in children)
            {
                yield return t;
            }
        }

        /// <summary>
        /// Gets the child at the specified index
        /// </summary>
        /// <param name="i">the child index</param>
        /// <returns></returns>
        public Term GetChild(int i)
        {
            if (i< 0 || i > children.Count)
            {
                return null;
            }

            return this.children[i];
        }

        /// <summary>
        /// returns the number of children terms
        /// </summary>
        /// <returns>the number of childern terms</returns>
        public int ChildrenCount()
        {
            return this.children.Count;
        }

        /// <summary>
        /// Reset composite term
        /// </summary>
        public override void reset()
        {
	        this.state = expressionState.Default;
	        foreach(var child in this.children)
	        {
		        child.reset();
	        }
        }
       
        /// <summary>
        /// On error
        /// </summary>
        /// <param name="token"></param>
        public override void error(Token token)
        {
            this.children[index].error(token);
            base.error(token);
        }

        /// <summary>
        /// Permette di settare la variabile ErrorTolerance
        /// </summary>
        /// <param name="err"></param>
        public void setErrorTolerance()
        {
            this.error_tolerance = new ErrorTolerance();
            this.flagErrTolerance = true;
        }
        /// <summary>
        /// Restituisce la variabile ErrorTolerance. Se il flag flagErrTolerance è settato a true restituisce il proprio 
        /// errorTolerance, altrimenti restituisce l'errorTolerance del padre. Se non ha un puntatore al padre, allora
        /// restituisce null.
        /// </summary>
        /// <returns></returns>
        public ErrorTolerance getErrorTolerance()
        {
            if (flagErrTolerance)
                return error_tolerance;
            else
            {
                if (pointFather != null)
                    return pointFather.getErrorTolerance();
                else
                    return null;
            }
        }
    }
}
