using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
     * A composite expression of terms connected with the parallel operator.
     * The sequence operator expresses that the connected sub-terms (two or more) 
     * can be executed at the same time
     * @param {type} terms the list of sub-terms
     * @returns {djestit.Parallel}
     * @extends djestit.CompositeTerm
     */
namespace Unica.TemporalExpressionSimulator
{
    public class Parallel : CompositeTerm
    {
        // Costruttori
        public Parallel() : base() { }
        public Parallel(Term term) : base(term)
        {

        }
        public Parallel(List<Term> terms) : base(terms)
        {
           
        }

       
        public override bool LookAhead(Token token)
        {

            if (this.State == ExpressionState.Complete || this.State == ExpressionState.Error)
            {
                return false;
            }

            foreach(Term t in this.Children) 
            {
                if (t.LookAhead(token)) 
                { 
                    return true; 
                }
            }
            
            return false;
             
        }

       
        public override void Fire(Token token)
        {
            if (this.State == ExpressionState.Error)
                return;

            bool all = true;

            if(this.LookAhead(token))
            {
                foreach(Term child in this.Children)
                {
                    if(child.LookAhead(token))
                        child.Fire(token);

                    if(child.State == ExpressionState.Error)
                        this.Error(token);

                    all = all && child.State == ExpressionState.Complete;
                }
            }
            else
            {
                this.Error(token);
            }

            if(all)
            {
                this.Complete(token);
            }
        }
    }
}