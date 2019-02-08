using System.Collections.Generic;
using System.Linq;

namespace Unica.TemporalExpressionSimulator
{
    public class Iterative : CompositeTerm
    {
        /* Attributi */
        // Numero di iterazioni totali
        public int Iterations { get; set; } 

        /* Costruttori */
        public Iterative() : base() { }
        
        public Iterative(Term term) : base(term)
        {
            this.Children.Add(term);
        }

        public Iterative(List<Term> terms) : base(terms)
        {
            this.Children.Add(terms.First());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Reset()
        {
            this.State = ExpressionState.Default;
            this.Children[0].Reset();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool LookAhead(Token token)
        {
            if (this.Children != null && this.Children.Count > 0)
            {
                return this.Children[0].LookAhead(token);
            }
            return false;
        }

        public override void Fire(Token token)
        {
            if (this.State == ExpressionState.Error)
                return;

            if (this.LookAhead(token) && this.Children.Count > 0)
            {
                this.Children[0].Fire(token);

                /// Verifica lo stato del term in seguito all'invio del term
                switch (this.Children[0].State)
                {
                    case ExpressionState.Error:
                        this.Iterations = 0;
                        this.Error(token);
                        this.Children[0].Reset();
                        break;
                   
                    case ExpressionState.Complete:
                        this.Iterations++;
                        this.Complete(token);
                        this.Children[0].Reset();
                        break;
                }
            }
        }
    }
}
