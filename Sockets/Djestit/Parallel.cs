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
namespace Unica.Djestit
{
    public class Parallel : CompositeTerm
    {
        // Costruttori
        public Parallel() : base() { }
        public Parallel(Term term) : base(term)
        {
            //this.children.Clear();
            //this.children.Add(term);
        }
        public Parallel(List<Term> terms) : base(terms)
        {
            //this.children.Clear();
            //this.children = terms;
        }

        /*** Metodi ***/
        // Lookahead
        /*public override bool lookahead(Token token)
        {

            if (this.state == expressionState.Complete || this.state == expressionState.Error)
                return false;
            if (this.children != null && this.children.GetType() == typeof(List<Term>))
            {
                for (int index = 0; index < this.children.Count; index++)
                {
                    if (this.children[index].lookahead(token))
                        return true;
                }
            }
            return false;

        }*/
        public override bool lookahead(Token token)
        {
            
            if(this.state == expressionState.Complete || this.state== expressionState.Error)
                return false;
            if(this.children !=null && this.children.GetType() == typeof(List<Term>))
            {
                for(int index = 0; index < this.children.Count; index++)
                {
                    if (this.children[index].lookahead(token))
                        return true;
                    else if (getErrorTolerance().isError == false && getErrorTolerance().numError < deltaError)
                    {
                        getErrorTolerance().isError = true;
                        return true;
                    }
                }
            }
            return false;
             
        }

        // Fire
        /*public override void fire(Token token)
        {
            bool all = true;

            if (this.lookahead(token))
            {
                foreach (Term child in this.children)
                {
                    if(child.lookahead(token))
                        child.fire(token);
                    if (child.state == expressionState.Error)
                        this.error(token);
                    all = all && child.state == expressionState.Complete;
                }
            }
            else
            {
                this.error(token);
            }

            if (all)
            {
                this.complete(token);
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }*/
        public override void fire(Token token)
        {
            bool all = true;

            if(this.lookahead(token))
            {
                foreach(Term child in this.children)
                {
                    if(child.lookahead(token))
                        child.fire(token);
                    else
                    {
                        getErrorTolerance().errorDetect();
                        child.fire(token);
                    }

                    if(child.state == expressionState.Error)
                        this.error(token);

                    //all = all && child.state == expressionState.Complete;
                }
            }
            else
            {
                this.error(token);
            }

            if(all)
            {
                this.complete(token);
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }
    }
}