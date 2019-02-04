using System.Collections.Generic;
using System.Linq;



namespace Unica.Djestit.Feed
{
    /// Enum StateGroup: 
    /// Complete (la gesture è stata eseguita);
    /// Continue (la gesture è in esecuzione);
    /// Default (si attende un input dall'utente);
    /// False (la gesture è in errore).
    public enum StateGroup
    {
        Complete = 2,
        Continue = 1,
        Default = 0,
        Error = -1
    }

    // Delegate per gli eventi di tipo FeedbackGroupEvent 
    public delegate void FeedbackGroupEvent(FeedbackGroupEventArgs sender);

    public abstract class FeedbackGroup
    {
        /* Eventi */
        // Evento che viene lanciato quando il FeedbackGroup raggiunge lo stato di Complete
        public event FeedbackGroupEvent FeedbackGroupComplete;
        // Evento che viene lanciato quando il FeedbackGroup raggiunge lo stato di Continue
        public event FeedbackGroupEvent FeedbackGroupContinue;
        // Evento che viene lanciato quando il FeedbackGroup raggiunge lo stato di Error
        public event FeedbackGroupEvent FeedbackGroupError;

        /* Attributi */
        // Term associato al Group in questione
        public Term term { get; private set; }
        /// Flag per indicare lo stato del term:
        /// Se assume il valore Complete vuol dire che la gesture è stata eseguita;
        /// Se assume il valore Continue vuol dire che la gesture è in esecuzione;
        /// Se assume il valore Default vuol dire che il sistema sta aspettando che l'utente esegua una gesture;
        /// Se assume il valore False vuol dire che l'utente sta eseguendo una gesture totalmente dissimile, e quindi non dev'essere visualizzato.
        public StateGroup state;
        // Probabilità associata al Term
        public Likelihood likelihood { get; internal set; }
        // Handler associato alla Gesture
        public Handler handler;
        public List<Handler> handlers;

        /* Costruttore */
        public FeedbackGroup(Term term)
        {
            // Assegna al nodo il term a cui dev'essere associato
            this.term = term;
            // Inizializzo la classe Likelihood
            this.likelihood = new Likelihood();
            // Inizializza la stato del Nodo a Default
            this.state = StateGroup.Default;
            // Associa al FeedbackGroup l'handler associato al term (se questo non è nullo)
            if (term.CompleteHandlers != null && term.CompleteHandlers.Count > 0)
            {
                this.handler = term.CompleteHandlers.First();
            }
        }

        /* Metodi */
        /// <summary>
        /// Si occupa di lanciare l'evento associato al raggiungimento del Complete da parte di un FeedbackGroup.
        /// </summary>
        internal virtual void OnFeedbackGroupComplete()
        {
            // Se l'evento ha un gestore provvede a lanciarlo
            if (this.FeedbackGroupComplete != null)
            {
                // Creo il parametro FeedbackGroupEventArgs
                FeedbackGroupEventArgs sender = new FeedbackGroupEventArgs(this.term, this, this.handler);
                this.FeedbackGroupComplete(sender);// Lancia l'evento
            }
        }

        /// <summary>
        /// Comunica che la gesture associata al FeedbackGroup è ancora in esecuzione
        /// </summary>
        public virtual void OnFeedbackGroupContinue()
        {
            // Se l'evento ha un gestore provvede a lanciarlo
            if (this.FeedbackGroupContinue != null)
            {
                // Creo il parametro FeedbackGroupEventArgs
                FeedbackGroupEventArgs sender = new FeedbackGroupEventArgs(this.term, this, this.handler);
                this.FeedbackGroupContinue(sender);// Lancia l'evento
            }
        }

        /// <summary>
        /// Comunica che la gesture associata al FeedbackGroup è andata in error
        /// </summary>
        internal virtual void OnFeedbackGroupError()
        {
            // Se l'evento ha un gestore provvede a lanciarlo
            if (this.FeedbackGroupError != null)
            {
                // Creo il parametro FeedbackGroupEventArgs
                FeedbackGroupEventArgs sender = new FeedbackGroupEventArgs(this.term, this, this.handler);
                this.FeedbackGroupError(sender);// Lancia l'evento
            }
        }

        /// <summary>
        /// Indica se un FeedbackGroup si trova in uno stato di Complete oppure no
        /// </summary>
        /// <param name="leaf"></param>
        /// <returns></returns>
        public bool IsComplete(FeedbackGroup child)
        {
            if (child.state == StateGroup.Complete)
                return true;
            else
                return false;

        }
        /// <summary>
        /// Controlla quali sono i Child che si trovano in un stato di Complete. Nel caso restituisce true, altrimenti false.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal bool ChildComplete(FeedbackGroup child)
        {
            if (child.state == StateGroup.Complete)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Controlla quali sono i Child che si trovano in uno stato di Continue o di Default. Nel caso restituisce true, altrimenti false.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal bool ChildContinueDefault(FeedbackGroup child)
        {
            if (child.state == StateGroup.Continue || child.state == StateGroup.Default)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Controlla quali sono i Child che si trovano in uno stato di Continue o di Default. Nel caso restituisce true, altrimenti false.
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal bool ChildCompleteContinue(FeedbackGroup child)
        {
            if (child.state == StateGroup.Complete || child.state == StateGroup.Continue)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Controlla quali sono i Child che si trovano in uno stato di Continue. Nel caso restituisce true, altrimenti false
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        internal bool ChildContinue(FeedbackGroup child)
        {
            if (child.state == StateGroup.Continue)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Funzione che provvede a resettare tutti i nodi dell'albero nello stato di default.
        /// </summary>
        internal virtual void reset()
        {
            this.state = StateGroup.Default;
        }

        /// <summary>
        /// Quando il term aggiorna il proprio stato (Complete o Error), allora 
        /// viene aggiornato anche lo stato del suo FeedbackGesture (ponendolo
        /// di conseguenza in Complete o Error).
        /// </summary>
        public virtual void updateTerm()
        {
            if (this.term.state == expressionState.Complete)
            {
                // Complete
                this.state = StateGroup.Complete;// Cambio lo stato
                this.OnFeedbackGroupComplete();// Genero l'evento
            }
           
            else if(this.term.state == expressionState.Error)
            {
                // Error
                this.state = StateGroup.Error;// Cambio lo stato
                this.OnFeedbackGroupError();// Genero l'evento
            }
        }
    }
}
