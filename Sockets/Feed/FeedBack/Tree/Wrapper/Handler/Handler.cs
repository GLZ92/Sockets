using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Unica.Djestit.Feed
{
    public class Handler : ICloneable, IComparable<Handler>
    {
        /* Attributi */
        // Nome della funzione associata a questo Handler
        public string name { get; private set; }
        // Lista dei custom attributes di tipo Modifies dichiarati nella funzione
        public List<Modifies> elementList { get; private set; }
        // Funzione che gestisce la gesture
        public GestureEventHandler function { get; private set; }
        // Probabilità associata alla gesture
        public Likelihood likelihood { get; internal set; }

        /* Costruttore */
        public Handler(GestureEventHandler function, Term term)
        {
            // Probabilità
            if (term.GetType() == typeof(CompositeTerm))
                this.likelihood = new Likelihood ((CompositeTerm)term, ProbabilityType.IndipendentEvents);
            else
                this.likelihood = new Likelihood (term.likelihood);
            // Funzione 
            this.function = (GestureEventHandler)function.Clone();
            // Lista dei modifies modificati dalla funzione della gesture
            this.elementList = this.getModifiesAttribute();
            // Nome associata alla funzione
            this.name = function.Method.Name;
        }

        /// <summary>
        /// Costruttore che inizializza a zero (o a null), tutti gli elementi dell'handler
        /// </summary>
        public Handler()
        {
            this.function = null;
            this.likelihood = null;
        }
        
        /* Metodi */
        /// <summary>
        /// Clona l'Handler
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            // Clona i suoi membri
            return this.MemberwiseClone();
        }

        /// <summary>
        /// Prende gli attributi della funzione, sfruttando la funzione GetCustomAttributes, 
        /// e li trasforma in una lista.
        /// </summary>
        /// <returns></returns>
        public List<Modifies> getModifiesAttribute()
        {
            // Se nella funzione vengono utilizzati degli attributi personalizzati, ne viene restituita la lista 
            // completa; altrimenti si restituisce una lista vuota.
            if (this.function.GetMethodInfo().GetCustomAttributes().OfType<Modifies>().ToList().Count > 0)
                return this.function.GetMethodInfo().GetCustomAttributes().OfType<Modifies>().ToList();
            else
                return new List<Modifies>();
            
        }

        public override bool Equals(object obj)
        {
            // Controlla per prima cosa se i due handler puntano allo stesso oggetto
            // in tal caso restituisce vero.
            if (Object.ReferenceEquals(this, obj))
                return true;

            // Controlla se uno dei due oggetti è null, in tal caso
            // restituisce false.
            if (Object.ReferenceEquals(this, null) || Object.ReferenceEquals(obj, null))
                return false;

            return this.GetHashCode().Equals(((Handler)obj).GetHashCode());
        }

        /// <summary>
        /// Funzione che compara due Handler tra loro. Viene utilizzata per ordinare, per valore
        /// crescente di probabilità, gli handler all'interno della mappa che riporta i possibili 
        /// stati dell'interfaccia.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(Handler other)
        {
            /// Se entrambi i likelihood da comparare sono inizializzati allora li compara; qualora la probabilità
            /// sia per entrambi uguale a zero, restituisce -1. Se invece uno dei due handler ha un likelihood
            /// non definito, viene posto per prima l'altro. Infine se entrambi per gli handler non è stato definito
            /// l'attributo likelihood, viene restituito -1 di default.

            // Both different from null
            if (this.likelihood != null && other.likelihood != null)
            {
                // Determina se i due handler hanno la stessa probabilità
                int likelihoodCompare = this.likelihood.CompareTo(other.likelihood);
                return likelihoodCompare > 0 ? likelihoodCompare : (-1);
            }
            // One is null
            if (this.likelihood != null)
                return (this.likelihood.CompareTo(new Likelihood()));
            if (other.likelihood != null)
                return (other.likelihood.CompareTo(new Likelihood()));
            // Default
            return (-1);
        }

        public override int GetHashCode()
        {
            // Se l'handler non punta a nulla restituisce zero
            if (Object.ReferenceEquals(this, null)) return 0;

            // Calcola l'hashcode del nome
            int hashName = this.name == null ? 0 : this.name.GetHashCode();

            // Calcola l'hashcode della lista di elementi
            int hashList = this.elementList.GetHashCode();

            // Calcola l'hashcode dell'handler associata alla gesture
            int hashFunction = this.function.GetHashCode();

            // Calcola l'hashcode della probabilità associata alla gesture
            int hashLikelihood = this.likelihood.GetHashCode();

            // Calcola l'hashcode di tutto l'oggetto
            return hashName ^ hashList ^ hashFunction ^ hashLikelihood;
        }
    }

    /// <summary>
    /// Comparer della classe Handler
    /// </summary>
    public class HandlerComparer : IEqualityComparer<Handler>
    {

        // Funzione che verfica l'equalità tra due Handler
        public bool Equals(Handler x, Handler y)
        {
            // Controlla per prima cosa se i due handler puntano allo stesso oggetto
            // in tal caso restituisce vero.
            if (Object.ReferenceEquals(x, y)) return true;

            // Controlla se uno dei due oggetti è null, in tal caso
            // restituisce false.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Controlla se le due funzioni sono equivalenti, in tal caso restituisce true
            return x.GetHashCode().Equals(y.GetHashCode());
            //return x.function.Equals(y.function);
        }

        // Restituisce l'HashCode di un Handler
        public int GetHashCode(Handler handler)
        {
            // Se l'handler non punta a nulla restituisce zero
            if (Object.ReferenceEquals(handler, null)) return 0;

            // Calcola l'hashcode del nome
            int hashName = handler.name == null ? 0 : handler.name.GetHashCode();

            // Calcola l'hashcode della lista di elementi
            int hashList = handler.elementList.GetHashCode();

            // Calcola l'hashcode dell'handler associata alla gesture
            int hashFunction = handler.function.GetHashCode();

            // Calcola l'hashcode della probabilità associata alla gesture
            int hashLikelihood = handler.likelihood.GetHashCode();

            // Calcola l'hashcode di tutto l'oggetto
            return hashName ^ hashList ^ hashFunction ^ hashLikelihood;
        }
    }
}
