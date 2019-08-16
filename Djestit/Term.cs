using System.Collections.Generic;
using System;


namespace Unica.TemporalExpressionSimulator
{
    // Enum expressionState
    public enum ExpressionState
    {
        Complete = 1,
        Default = 0,
        Error = -1
    }

    //TODO controllare se serve
    /// <summary>
    /// Delegate per ricevere notifiche sullo stato dei task
    /// </summary>
    public delegate void ExpressionEventHandler(ExpressionEventArgs sender);


    public delegate void ExpressionChangeStateHandler();

    // TODO controllare se serva
    // Delegate per il TokenFire
    //public delegate void TokenFire(object obj, TokenFireArgs sender);

    /// <summary>
    /// Superclasse di tutte le espressioni
    /// </summary>
    public abstract class Term
    {
        /* Eventi */
        public event ExpressionEventHandler OnComplete;
        public event ExpressionEventHandler OnError;

        //TODO controllare se serve
        //public event ExpressionChangeStateHandler ChangeState;

        // TODO controllare se serva
        //public event TokenFire TokenFire;

        /* Attributi */
        public ExpressionState State { get; protected set; }
        //public string Id { get; set; }
        public string Id { get; set; }
        public string kind = "";
        public List<Term> Children { get; protected set; }


        // flag per l'implementazione di alcuni operatori temporali
        internal bool excluded = false;
        internal bool once = false;

        // Indica quante volte è stato eseguito il Term in questione, da quando il programma è stato avviato
        public int ExecutionCount { get; protected set; }


        // Puntatore al padre
        public Term Parent { get; set; }

        // Nome del Term
        public string Name { get; set; }


        public Term()
        {
            this.State = ExpressionState.Default;

        }

        /* Metodi */
        /// <summary>
        /// Chiede all'espressesione di eseguire l'azione descritta dal token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="numError"></param>
        public virtual void Fire(Token token)
        {
            if (this.State == ExpressionState.Error)
                return;

            this.Complete(token);
        }

        /// <summary>
        /// Reinizializza l'espressione
        /// </summary>
        public virtual void Reset()
        {
            this.State = ExpressionState.Default;
        }

        /// <summary>
        /// Imposta lo stato dell'espressione come completo
        /// </summary>
        /// <param name="token"></param>
        public virtual void Complete(Token token)
        {
            // Aggiorna i contatori e verifica se deve generare l'evento Complete
            this.ExecutionCount++;
            // Aggiorna lo stato
            this.State = ExpressionState.Complete;
            // Genera gli eventi OnComplete e OnChangeState
            ExpressionEventArgs e = new ExpressionEventArgs(this, token);
            if(this.OnComplete != null)
                OnComplete(e);

            //TODO vedere se sia necessiaro
            //onChangeState();
        }

        /// <summary>
        /// Imposta lo stato dell'espressione come errore
        /// </summary>
        /// <param name="token"></param>
        public virtual void Error(Token token)
        {
            // Modifica lo stato
            this.State = ExpressionState.Error;
            // Genera gli eventi Error e ChangeState
            ExpressionEventArgs e = new ExpressionEventArgs(this, token);
            if(this.OnError != null)
                OnError(e);

            //TODO vedere se sia necessiaro
            //onChangeState();
        }

        /// <summary>
        /// Verifica se il token possa essere eseguito dall'espressione corrente.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual bool LookAhead(Token token)
        {
            if (token != null)
            {
                return true;
            }
            return false;
        }

        public int getX()
        {
            return Convert.ToInt32(this.Id.Split(";")[0]);
        }

        public int getY()
        {
            return Convert.ToInt32(this.Id.Split(";")[1]);
        }
    }
}
