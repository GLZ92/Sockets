using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ErrorTolerance 
using Unica.Djestit.ErrorToleranceManager;

namespace Unica.Djestit
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
        public override void reset()
        {
            this.state = expressionState.Default;
            foreach (Term child in this.children)
            {
                child.reset();
                child.excluded = false;
            }
        }
        /// <summary>
        /// Pone in Error 
        /// </summary>
        /// <param name="token"></param>
        public override void error(Token token)
        {
            // Resetta il contatore e verifica se deve generare l'evento Error
            num_discrete = 0;
            // Modifica lo stato
            state = expressionState.Error;
            // Genera gli eventi Error e ChangeState
            GestureEventArgs e = new GestureEventArgs(this, token);
            onError(e);
            onChangeState();
        }

        public override bool lookahead(Token token)
        {   
            if (this.state == expressionState.Complete|| this.state == expressionState.Error)
                return false;
            if (this.children != null && this.children.GetType() == typeof(List<Term>))
            {
                for (int index = 0; index < this.children.Count; index++)
                {
                    if (!this.children[index].excluded && this.children[index].lookahead(token) == true)
                        return true;
                }
            }
            return false;

        }

        public virtual void feedToken(Token token)
        {
            if (this.state == expressionState.Complete || this.state == expressionState.Error)
                return;

            if (this.children != null && this.children.GetType() == typeof(List<Term>))
            {
                for (int index = 0; index < this.children.Count; index++)
                {
                    if (!this.children[index].excluded)
                    {
                        if (this.children[index].lookahead(token))
                        {
                            this.children[index].fire(token);
                        }
                        else
                        {
                            ErrorTolerance err = getErrorTolerance();
                            if (err != null && err.numError < deltaError)
                            {
                                err.errorDetect();
                                this.children[index].fire(token);
                            }
                            else
                            {
                                // The current sub-term is not able to handle the input sequence
                                this.children[index].excluded = true;
                                this.children[index].error(token);
                            }
                        }
                    }
                }
            }
        }

        public override void fire(Token token)
        {
            this.feedToken(token);
            bool allExcluded = true;

            for (int index = 0; index < this.children.Count; index++)
            {
                if (!this.children[index].excluded)
                {
                    allExcluded = false;
                    switch (this.children[index].state)
                    {
                        // one of the subterms is completed or likely, then the entire expression is completed
                        case expressionState.Likely:
                        case expressionState.Complete:
                            this.complete(token);
                            this.reset();
                            return;
                        case expressionState.Error:
                            // this case is never executed, since
                            // feedToken excludes the subterms in error state
                            return;
                    }
                }
            }
            if (allExcluded)
            {
                this.error(token);
                this.reset();
            }
            
            // Comunica che è stato sparato un token
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }
    }
}


/* 
        public void feedToken(Token token)
        {
            if (this.state == expressionState.Complete || this.state == expressionState.Error)
                return;

            if (this.children != null && this.children.GetType() == typeof(List<Term>))
            {
                for (int index = 0; index < this.children.Count; index++)
                {
                    if (!this.children[index].excluded)
                    {
                        if (this.children[index].lookahead(token))
                        {
                            this.children[index].fire(token);
                        }
                        else
                        {
                            // Tollera un altro elemento

                            // the current sub-term is not able to handle the input
                            // sequence
                            this.children[index].excluded = true;
                            this.children[index].error(token);
                        }
                    }
                }
            }
        }

        public override void fire(Token token)
        {
            this.feedToken(token);
            bool allExcluded = true;

            for (int index = 0; index < this.children.Count; index++)
            {
                if (!this.children[index].excluded)
                {
                    allExcluded = false;
                    switch (this.children[index].state)
                    {
                        case expressionState.Complete:
                            // one of the subterms is completed, then the entire expression is completed
                            this.complete(token);
                            return;
                        case expressionState.Error:
                            // this case is never executed, since
                            // feedToken excludes the subterms in error state
                            return;
                    }
                }
            }
            if (allExcluded)
            {
                this.error(token);
            }
            
            // Comunica che è stato sparato un token
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }
*/
