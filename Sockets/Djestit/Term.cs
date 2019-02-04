using System.Collections.Generic;
// Handler 
using Unica.Djestit.Feed;
// Concurrency (Transazioni)
using Unica.Djestit.Concurrency;
// Transazioni
using System.Transactions;

namespace Unica.Djestit
{
    // Enum expressionState
    public enum expressionState
    {
        Complete = 1,
        Default = 0,
        Likely = -1,
        Error = -2
    }
    // Delegate per i GestureEventHandler
    public delegate void GestureEventHandler(Term term_event, GestureEventArgs sender);
    public delegate void GestureChangeStateHandler();
    // Delegate per il TokenFire
    public delegate void TokenFire(object obj, TokenFireArgs sender);

    public abstract class Term
    {
        /* Eventi */
        public event GestureEventHandler Complete;
        public event GestureEventHandler Likely;
        public event GestureEventHandler Error;
        public event GestureChangeStateHandler ChangeState;
        public event TokenFire TokenFire;
        /* Attributi */
        public expressionState state = expressionState.Default;
        public bool excluded;
        public bool once;
        // Indica quante volte è stato eseguito il Term in questione, da quando il programma è stato avviato
        public int num_total { get; protected set; }
        // Indica il numero di volte consecutive con cui è stato eseguito il Term in questione
        public int num_discrete { get; protected set; }
        // Probabilità dell'evento
        public float likelihood;
        // Handler dell'evento
        public List<Handler> CompleteHandlers = new List<Handler>();
        public List<Handler> ErrorHandlers = new List<Handler>();
        // Puntatore al padre
        internal CompositeTerm pointFather = null;
        // TransactionManager
        public TransactionsManager transactionsManager = new TransactionsManager();
        // Nome del Term
        public string Name { get; set; }

        /* Metodi */
        /// <summary>
        /// Spara il token, se si è arrivati a questo caso allora il movimento dell'utente non rispetta completamente
        /// le condizioni previste dal term; tuttavia si mantiene ancora in vita la gesture.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="numError"></param>
        public virtual void fire(Token token, int numError)
        {
            this.likely(token);
        }

        /// <summary>
        /// Spara il token, se si è arrivati a questo punto allora il movimento rispetta le condizioni previste dal term.
        /// </summary>
        /// <param name="token"></param>
        public virtual void fire(Token token)
        {
            this.complete(token);
        }

        /// <summary>
        /// Reinizializzo il termine dell'espressione
        /// </summary>
        public virtual void reset()
        {
            this.num_total = this.num_discrete = 0;
            this.state = expressionState.Default;
        }

        /// <summary>
        /// Imposta lo stato dell'espressione come completo
        /// </summary>
        /// <param name="token"></param>
        public virtual void complete(Token token)
        {
            // Aggiorna i contatori e verifica se deve generare l'evento Complete
            this.num_total++;
            this.num_discrete++;
            // Aggiorna lo stato
            this.state = expressionState.Complete;
            // Genera gli eventi OnComplete e OnChangeState
            GestureEventArgs e = new GestureEventArgs(pointFather, token);
            onComplete(e);
            onChangeState();
        }

        /// <summary>
        /// Imposta lo stato dell'espressione come "probabilmente". Per ora limitiamoci a comunicare che lo stato del term vien 
        /// modificato.
        /// </summary>
        /// <param name="token"></param>
        public virtual void likely(Token token)
        {
            // Aggiorna lo stato
            this.state = expressionState.Likely;
            // Genera gli eventi OnLikely, OnComplete e OnChangeState
            GestureEventArgs e = new GestureEventArgs(pointFather, token);
            onLikely(e);
            onComplete(e);
            onChangeState();
        }

        /// <summary>
        /// Imposta lo stato dell'espressione come errore
        /// </summary>
        /// <param name="token"></param>
        public virtual void error(Token token)
        {
            // Resetta il contatore e verifica se deve generare l'evento Error
            this.num_discrete = 0;
            // Modifica lo stato
            this.state = expressionState.Error;
            // Genera gli eventi Error e ChangeState
            GestureEventArgs e = new GestureEventArgs(pointFather, token);
            onError(e);
            onChangeState();
        }

        /// <summary>
        /// Verifica se il movimento dell'utente rispetta le condizioni del term o meno. Di default viene restituito true.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual bool lookahead(Token token)
        {
            if (token != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Genera l'evento Likely; viene richiamato quando lo stato del term va in likely.
        /// </summary>
        /// <param name="t"></param>
        protected virtual void onLikely(GestureEventArgs t)
        {
            Likely?.Invoke(this, t);
        }

        /// <summary>
        /// Genera l'evento ChangeState, viene richiamato quando viene modificato lo stato del term.
        /// </summary>
        public void onChangeState()
        {
            if (this.ChangeState != null)
                this.ChangeState();
        }

        // IsTokenFire
        public virtual void IsTokenFire(TokenFireArgs sender)
        {
            if (this.TokenFire != null)
                this.TokenFire(this, sender);
        }

        /// <summary>
        /// Controlla se al term è stato associato una funzione di complete
        /// </summary>
        /// <returns></returns>
        public virtual bool hasComplete()
        {
            if (this.Complete != null)
                return true;
            else
                return false;
        }

        // Complete /
        /// <summary>
        /// Setta la funzione Complete del term e provvede a crearne l'handler.
        /// </summary>
        /// <param name="func"></param>
        public virtual void setCompleteHandler(GestureEventHandler func)
        {
            Complete += func;
            CompleteHandlers.Add(new Handler(func, this));
        }
        /// <summary>
        /// Genera l'evento Complete; viene richiamato quando lo stato del term va in complete. Provvede anche ad eseguire
        /// le modifiche alle variabili di stato in un ambiento protetto.
        /// </summary>
        /// <param name="t"></param>
        protected virtual void onComplete(GestureEventArgs t)
        {
            if (Complete != null)
            {
                // Genera l'evento Complete
                Complete(this, t);
                foreach (Handler handler in CompleteHandlers)
                {
                    // Esegui in sicurezza l'eventuali modifiche alle variabili di stato.
                    using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required))
                    {
                        //transactionsManager.onTransactionExcute(handler.elementList);
                        transactionsManager.onTransactionExcute(Feedback.listModifies, handler.elementList);
                        // Da l'ok per completare la transizione.
                        scope.Complete();
                    }
                }
            }
        }

        // Error /
        /// <summary>
        /// Setta la funzione Error del term e provvede a crearne l'handler.
        /// </summary>
        /// <param name="func"></param>
        public virtual void setErrorHandler(GestureEventHandler func)
        {
            Error += func;
            ErrorHandlers.Add(new Handler(func, this));
        }
        /// <summary>
        /// Genera l'evento Error; viene richiamato quando lo stato del term va in error.
        /// </summary>
        /// <param name="t"></param>
        protected virtual void onError(GestureEventArgs t)
        {
            Error?.Invoke(this, t);
        }
    }
}
