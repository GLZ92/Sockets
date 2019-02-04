using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.Djestit
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
        public override void reset()
        {
            this.state = expressionState.Default;
            foreach(Term child in this.children)
            {
                child.reset();
                child.once = false;
                child.excluded = false;
            }
        }

        public override bool lookahead(Token token)
         {   
             if(this.state == expressionState.Complete || this.state == expressionState.Error)
                 return false;
             if(this.children != null && this.children.GetType() == typeof(List<Term>))
             {
                 for(int index = 0; index < this.children.Count; index++)
                 {
                     if(!this.children[index].once && this.children[index].lookahead(token))
                         return true;
                 }
             }
             return false;
            
         }

        /*
        public override void fire(Token token)
        {
            this.feedToken(token);
            bool allComplete = true, newSequence = false, allExcluded = true;

            for(int index = 0; index < this.children.Count; index++)
            {
                if(!this.children[index].once)
                {
                    if(!this.children[index].excluded)
                    {
                        allExcluded = false;
                        switch(this.children[index].state)
                        {
                            case expressionState.Complete:
                                this.children[index].once = true;
                                this.children[index].excluded = true;
                                newSequence = true;
                                break;
                            case expressionState.Error:
                                // this case is never executed, since
                                // feedToken excludes the subterms in error state
                                break;
                            default :
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
            if(allComplete)
            {
                // we completed all sub-terms
                this.complete(token);
                return;
            }
            if(allExcluded)
            {
                // no expression was able to handle the input
                this.error(token);
                return;
            }
            if(newSequence)
            {
                // execute a new sequence among those in order independence
                for(int index = 0; index < this.children.Count; index++)
                {
                    if(!this.children[index].once)
                    {
                        this.children[index].excluded = false;
                        this.children[index].reset();
                    }
                }
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        } */

        /* Error Tollerance Manager */
        public override void fire(Token token)
        {
            this.feedToken(token);
            bool allComplete = true, newSequence = false, allExcluded = true;

            for (int index = 0; index < this.children.Count; index++)
            {
                if (!this.children[index].once)
                {
                    if (!this.children[index].excluded)
                    {
                        allExcluded = false;
                        switch (this.children[index].state)
                        {
                            case expressionState.Likely:
                            case expressionState.Complete:
                                this.children[index].once = true;
                                this.children[index].excluded = true;
                                newSequence = true;
                                break;
                            case expressionState.Error:
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
                this.complete(token);
                return;
            }
            if (allExcluded)
            {
                // no expression was able to handle the input
                this.error(token);
                return;
            }
            if (newSequence)
            {
                // execute a new sequence among those in order independence
                for (int index = 0; index < this.children.Count; index++)
                {
                    if (!this.children[index].once)
                    {
                        this.children[index].excluded = false;
                        this.children[index].reset();
                    }
                }
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }
    }
}
