using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.TemporalExpressionSimulator
{
    public class OrderIndependece : Choice
    {
        // Costruttori
        public OrderIndependece() : base() { }
        public OrderIndependece(Term term) : base(term)
        {
        }
        public OrderIndependece(List<Term> terms) : base(terms)
        {
        }

        // Metodi
        public override void Reset()
        {
            this.State = ExpressionState.Default;
            foreach(Term child in this.Children)
            {
                child.Reset();
                child.once = false;
                child.excluded = false;
            }
        }

        public override bool LookAhead(Token token)
         {   
             if(this.State == ExpressionState.Complete || this.State == ExpressionState.Error)
                 return false;
             if(this.Children != null )
             {
                 for(int index = 0; index < this.Children.Count; index++)
                 {
                     if(!this.Children[index].once && this.Children[index].LookAhead(token))
                         return true;
                 }
             }
             return false;
            
         }
         

        public override void Fire(Token token)
        {
            if (this.State == ExpressionState.Error)
                return;

            this.feedToken(token);
            bool allComplete = true, newSequence = false, allExcluded = true;

            foreach(Term c in this.Children)
            {
                if (!c.once)
                {
                    if (!c.excluded)
                    {
                        allExcluded = false;
                        switch (c.State)
                        {
                            case ExpressionState.Complete:
                                c.once = true;
                                c.excluded = true;
                                newSequence = true;
                                break;
                            case ExpressionState.Error:
                                // this case is never executed, since
                                // feedToken excludes the subterms in error state
                                break;
                            default:
                                allComplete = false;
                                break;
                        }
                    }
                    else
                    {
                        allComplete = false;
                    }
                }
            }
            if (allComplete)
            {
                // we completed all sub-terms
                this.Complete(token);
                return;
            }
            if (allExcluded)
            {
                // no expression was able to handle the input
                this.Error(token);
                return;
            }
            if (newSequence)
            {
                // execute a new sequence among those in order independence
                foreach (Term c in this.Children)
                {
                    if (!c.once)
                    {
                        c.excluded = false;
                        c.Reset();
                    }
                }
            }
        }
    }
}
