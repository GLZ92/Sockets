using System.Collections.Generic;

namespace Unica.TemporalExpressionSimulator
{
    // Quando viene rilevato e tollerato un movimento errato dell'utente
    public delegate void GestureErrorTolerance();

    public class CompositeTerm : Term
    {
        /* Attributi */
        // Contiene la lista di operandi da gestire
        //public List<Term> Children { get; protected set; }
        public int Index { get; protected set; }


        /* Costruttore */
        public CompositeTerm()
        {
            this.Children = new List<Term>();
        }

        public CompositeTerm(List<Term> terms) 
            : this()
        {
            // Ad ogni figlio in input provvede a settare il puntatore al padre.
            foreach(Term term in terms)
            {
                term.Parent = this;
                Children.Add(term);
            }
        }
        public CompositeTerm(Term term)
            : this()
        {
            // Associa al figlio in input il puntatore al padre
            term.Parent = this;
            Children.Add(term);
        }

        /* Metodi */
        /// <summary>
        /// Adds a child to the composite term
        /// </summary>
        /// <param name="child">the child term</param>
        public void AddChild(Term child)
        {
            this.Children.Add(child);
            child.Parent = this;
        }

        /// <summary>
        /// Iterates over the children array
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Term> ChildrenEnumeration()
        {
            foreach(Term t in Children)
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
            if (i< 0 || i > Children.Count)
            {
                return null;
            }

            return this.Children[i];
        }

        /// <summary>
        /// returns the number of children terms
        /// </summary>
        /// <returns>the number of childern terms</returns>
        public int ChildrenCount()
        {
            return this.Children.Count;
        }

        /// <summary>
        /// Reset composite term
        /// </summary>
        public override void Reset()
        {
	        this.State = ExpressionState.Default;
	        foreach(var child in this.Children)
	        {
		        child.Reset();
	        }
        }
       
        /// <summary>
        /// On error
        /// </summary>
        /// <param name="token"></param>
        public override void Error(Token token)
        {
            this.State = ExpressionState.Error;
            foreach (var child in this.Children)
            {
                child.Error(token);
            }
        }
    }
}
