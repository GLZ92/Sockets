using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Unica.TemporalExpressionSimulator
{
    public class Disabling : Choice
    {
        /* Costruttori */
        public Disabling() : base() { }
        public Disabling(Term term) : base(term)
        {
        }
        public Disabling(List<Term> terms) : base(terms)
        {
        }

        /* Metodi */
        public override bool LookAhead(Token token)
        {
            if (this.State == ExpressionState.Complete || this.State == ExpressionState.Error)
                return false;
            if (this.Children != null)
            {
                for (int index = 0; index < this.Children.Count; index++)
                {
                    if (!this.Children[index].excluded && this.Children[index].LookAhead(token) == true)
                        return true;
                }
            }
            return false;
        }

        public override void feedToken(Token token)
        {
            if (this.State == ExpressionState.Complete || this.State == ExpressionState.Error)
                return;

            if (this.Children != null)
            {
                for (int index = 0; index < this.Children.Count; index++)
                {
                    if (!this.Children[index].excluded)
                    {
                        if (this.Children[index].LookAhead(token))
                        {
                            this.Children[index].Fire(token);
                        }
                        else
                        {
                            // The current sub-term is not able to handle the input sequence
                            this.Children[index].excluded = true;
                            this.Children[index].Error(token);
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
            bool min = false;

            for (int index = 0; index < this.Children.Count; index++)
            {
                if (!this.Children[index].excluded)
                {
                    min = true;
                    allExcluded = false;
                    switch (this.Children[index].State)
                    {
                        case ExpressionState.Complete:
                            if (index == this.Children.Count - 1)
                            {
                                // the expression is completed when the
                                // last subterm is completed
                                this.Complete(token);
                            }
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if (min)
                    {
                        // re-include terms with index > min for next 
                        // disabling term selection
                        this.Children[index].excluded = false;
                        this.Children[index].Reset();
                    }
                }
            }
            if (allExcluded)
            {
                this.Error(token);
            }

        }
    }
}
