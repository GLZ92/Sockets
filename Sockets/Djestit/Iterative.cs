using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.Djestit
{
    public class Iterative : CompositeTerm
    {
        /* Attributi */
        // Numero di iterazioni totali
        public int Iterations { get; set; } 
        // Numero minimo di iterazioni richieste
        const int thresold_iteration = 2;

        /* Costruttori */
        public Iterative() : base() { }
        
        public Iterative(Term term) : base(term)
        {
            this.children = new List<Term>();
            this.children.Add(term);
        }

        public Iterative(List<Term> terms) : base(terms)
        {
            this.children.Clear();
            this.children.Add(terms.First());
        }

        /// <summary>
        /// 
        /// </summary>
        public override void reset()
        {
            this.state = expressionState.Default;
            this.children[0].reset();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public override bool lookahead(Token token)
        {
            if (this.children != null && this.children[0].lookahead(token))
                return this.children[0].lookahead(token);
            else
            {
                if (getErrorTolerance().numError < deltaError && Iterations > thresold_iteration)
                {
                    getErrorTolerance().isError = true;
                    return true;
                }
            }
            return false;
        }

        /*
        public override void fire(Token token)
        {
            if(this.lookahead(token))
            {
                this.children[0].fire(token);
                switch (this.children[0].state)
                {
                    case expressionState.Complete:
                        this.complete(token);
                        this.children[0].reset();
                        break;
                    case expressionState.Error:
                        this.error(token);
                        this.children[0].reset();
                        break;
                }
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }*/

        /* Error Tolerance Manager */
        public override void fire(Token token)
        {
            if (this.lookahead(token))
            {
                // Controlla se è stato rilevato un errore nell'esecuzione del movimento
                if (getErrorTolerance().isError)
                    this.children[0].fire(token, getErrorTolerance().numError);
                else
                    this.children[0].fire(token);

                /// Verifica lo stato del term in seguito all'invio del term
                switch (this.children[0].state)
                {
                    case expressionState.Error:
                        this.Iterations = 0;
                        this.error(token);
                        break;
                    case expressionState.Likely:
                        this.getErrorTolerance().errorDetect();
                        this.complete(token);
                        break;
                    case expressionState.Complete:
                        this.Iterations++;
                        this.complete(token);                        
                        break;
                }
            }
            this.children[0].reset();// Resetta lo stato del figlio
            // Comunica che il token è stato gestito (genera l'evento TokenFire)
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }
    }
}
