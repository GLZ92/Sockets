using System.Collections.Generic;


namespace Unica.TemporalExpressionSimulator
{
    /// <summary>
    /// Descrive l'operatore Sequence di GestIT
    /// </summary>
    public class Sequence : CompositeTerm
    {
        /* Costruttori */
        public Sequence() : base() { }
        /// <summary>
        /// Prende in ingresso un singolo parametro.
        /// </summary>
        /// <param name="terms"></param>
        public Sequence(Term term) : base(term)
        {

        }
        /// <summary>
        /// Prende in ingresso un'intera lista.
        /// </summary>
        /// <param name="terms"></param>
        public Sequence(List<Term> terms) : base(terms)
        {

        }

        /* Metodi */
        /// <summary>
        /// Si occupa del reset del term
        /// </summary>
        public override void Reset()
        {
            this.State = ExpressionState.Default;
            this.Index = 0;
            foreach (Term t in Children)
            {
                t.Reset();
            }
        }

        public override bool LookAhead(Token token)
        {
            if (this.State == ExpressionState.Complete || this.State == ExpressionState.Error)
            {
                return false;
            }

            if ((this.Children != null) && (this.Children[Index] != null))
            {
                return this.Children[Index].LookAhead(token);
            }
            return false;
        }


        public override void Fire(Token token)
        {
            if (this.State == ExpressionState.Error)
                return;

            if (this.LookAhead(token))
            {
                this.Children[Index].Fire(token);
            }
            else
            {
               
                Error(token);
                return;
            }

            switch (this.Children[Index].State)
            {
               
                case ExpressionState.Complete:
                    this.Index++;

                    if (this.Index >= this.Children.Count)
                    {
                        // ho completato tutti i figli, la sequenza è completa
                        this.Complete(token);
                    }
                    break;
                // Error, ci si ferma
                case ExpressionState.Error:
                    this.Error(token);
                    break;
            }
        }
    }
}
