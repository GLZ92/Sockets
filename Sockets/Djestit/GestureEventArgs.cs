using System;

namespace Unica.Djestit
{
    ///
    public class GestureEventArgs : EventArgs
    {
        /* Attributi */
        /// <summary>
        /// Term che descrive l'intera gesture.
        /// </summary>
        public readonly Term term;
        /// <summary>
        /// Ultimo token ricevuto.
        /// </summary>
        public readonly Token token;

        /* Costruttore */
        public GestureEventArgs(Term term, Token token)
        {
            this.term = term;
            this.token = token;
        }
    }
}
