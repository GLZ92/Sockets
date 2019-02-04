using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Debug
using System.Diagnostics;

namespace Unica.Djestit
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
        public override void reset()
        {
            this.state = expressionState.Default;
            this.index = 0;
            foreach (Term t in children)
            {
                t.reset();
            }
        }

        public override bool lookahead(Token token)
        {
            if (this.state == expressionState.Complete || this.state == expressionState.Error)
            {
                return false;
            }

            if ((this.children != null) && (this.children[index] != null))
            {
                return this.children[index].lookahead(token);
            }
            return false;
        }

        /// <summary>
        /// Verifica il token in input.
        /// </summary>
        /// <param name="token"></param>
        /*public override void fire(Token token)
        {
            if (this.lookahead(token))
            {
                this.children[index].fire(token);
            }
            else
            {
                this.error(token);
                return;
            }

            // Se l'indice è superiore al numero di subterm da gestire, allora è in error.
            if (index >= this.children.Count)
            {
                this.error(token);// Error
            }

            // Altrimenti si va a studiare lo stato dell'ultimo figlio visualizzato
            switch (this.children[index].state)
            {
                // Complete, si va avanti con gli altri term. Se non ci sono altri term da esaminare allora la gesture è 
                // stata completata.
                case expressionState.Complete:
                    this.index++;// Aggiorna index
                    // Verifca se sono state controllate tutte le primitive
                    if (index >= this.children.Count)
                        this.complete(token);
                    break;
                // Error, ci si ferma
                case expressionState.Error:
                    this.error(token);
                    break;
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }*/

        /* Error Tolerance Manager */
        /// <summary>
        /// La funzione fire, adattata a gestire degli eventuali errori dell'utente.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="numError"></param>
        public override void fire(Token token)
        {
            if (this.lookahead(token))
            {
                this.children[index].fire(token);
            }
            else
            {
                /// Nel caso in cui l'utente faccia un errore nell'esecuzione della gesture si provvede a gestirlo, in maniera tale
                /// da rendere il modello più flessibile agli errori iniziali dell'utente. Si possono tollerare un deltaError di errori.
                /// In futuro si prevede di utilizzare anche il concetto di somiglianza, in modo da renderlo più realistico. 
                if (getErrorTolerance().numError < deltaError)
                {
                    getErrorTolerance().errorDetect();// Errore
                    this.children[index].fire(token);// Chiamo l'apposito gestore.
                }
                error(token);
                return;
            }

            // Se l'indice è superiore al numero di subterm da gestire, allora è in error.
            if (index >= this.children.Count)
            {
                this.error(token);// Error
            }

            // Altrimenti si va a studiare lo stato dell'ultimo figlio visualizzato
            switch (this.children[index].state)
            {
                // Likely o Complete, si va avanti con gli altri term. Se non ci sono altri term da esaminare allora la gesture è 
                // stata "probabilmente" completata o completata.
                case expressionState.Likely:
                case expressionState.Complete:
                    this.index++;// Aggiorna index
                    // Verifica se sono state controllate tutte le primitive
                    if (index >= this.children.Count)
                    {
                        if (this.getErrorTolerance().numError > 0)
                            this.likely(token);// L'utente non ha eseguito perfettamente la gesture
                        else
                            this.complete(token);// L'utente ha eseguito perfettamente la gesture
                    }
                    break;
                // Error, ci si ferma
                case expressionState.Error:
                    this.error(token);
                    break;
            }
            //
            TokenFireArgs args = new TokenFireArgs(token, this);
            IsTokenFire(args);
        }
    }
}
