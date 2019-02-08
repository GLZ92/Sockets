using System.Collections.Generic;
using System.Linq;


namespace Unica.TemporalExpressionSimulator
{
    /**
     * A composite expression of terms connected with the choice operator.
     * The sequence operator expresses that it is possible to select one among 
     * the terms in order to complete the whole expression.
     * The implementation exploits a best effort approach for dealing with the 
     * selection ambiguity problem (see [1])
     */
    public class Choice : CompositeTerm
    {
        /* Costruttori */
        public Choice() : base()
        {

        }
        /// <summary>
        /// Prende in ingresso un singolo parametro.
        /// </summary>
        /// <param name="term"></param>
        public Choice(Term term) : base(term)
        {

        }
        /// <summary>
        /// Prende in ingresso un'intera lista.
        /// </summary>
        /// <param name="terms"></param>
        public Choice(List<Term> terms) : base(terms)
        {

        }

        /* Metodi */
        /// <summary>
        /// Resetta la Choice e tutti i suoi figli.
        /// </summary>
        public override void Reset()
        {
            this.State = ExpressionState.Default;
            foreach (Term child in this.Children)
            {
                child.Reset();
                child.excluded = false;
            }
        }


        public override bool LookAhead(Token token)
        {   
            if (this.State == ExpressionState.Complete|| this.State == ExpressionState.Error)
                return false;
            if (this.Children != null)
            {

                foreach(Term c in this.Children)
                {
                    if(c.excluded && c.LookAhead(token)) 
                    {
                        return true; 
                    }
                }
            }
            return false;

        }

        public virtual void feedToken(Token token)
        {
            if (this.State == ExpressionState.Complete || this.State == ExpressionState.Error)
                return;

            if (this.Children != null)
            {
                foreach (Term c in this.Children)
                {
                    if (!c.excluded)
                    {
                        if (c.LookAhead(token))
                        {
                            c.Fire(token);
                        }
                        else
                        {
                           // The current sub-term is not able to handle the input sequence
                            c.excluded = true;
                            // TODO verificare se vada riabilitato
                           //c.Error(token);
                        }
                    }
                }
            }
        }

        public override void Fire(Token token)
        {
            if (this.State == ExpressionState.Error)
                return;

            this.feedToken(token);
            bool allExcluded = true;

            foreach(Term c in this.Children)
            {
                if (!c.excluded)
                {
                    allExcluded = false;
                    switch (c.State)
                    {
                        // one of the subterms is completed, then the
                        // entire expression is completed
                        case ExpressionState.Complete:
                            this.Complete(token);
                            return;
                        case ExpressionState.Error:
                            // this case is never executed, since
                            // feedToken excludes the subterms in error state
                            return;
                    }
                }
            }
            if (allExcluded)
            {
                foreach (Term c in this.Children)
                {
                    c.Error(token);
                }
                this.Error(token);
                this.Reset();
            }

        }

    }
}
