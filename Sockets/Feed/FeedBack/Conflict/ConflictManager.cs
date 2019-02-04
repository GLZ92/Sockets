using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Unica.Djestit.Feed
{
    /// Costruisce una mappa degli oggetti in conflitto sia per le gesture parzialmente riconosciute in un dato momento,
    /// sia per tutte le funzioni associate alle varie gesture.
    public class ConflictManager
    {
        /* Attributi */
        // Elenco di tutti gli attributi del Programma
        public List<Modifies> listModifies { get; private set; }
        // Elenco generale degli attributi per ogni funzione
        public Dictionary<Handler, List<Modifies>> mapHandlersModifies { get; private set; }
        // Elenco generale degli attributi per ogni funzione in conflitto
        public Dictionary<Handler, List<Modifies>> mapConflicts { get; private set; }
        // Elenco degli attributi in conflitto per le funzioni, durante l'esecuzione
        public Dictionary<Handler, List<Modifies>> mapConflictExec { get; private set; }
        // Lista degli attributi non in comune
        private List<Modifies> listDistinct = new List<Modifies>();
        // Lista degli attributi in conflitto
        private List<Modifies> listConflict = new List<Modifies>();
        // Singleton
        private static ConflictManager singleton = null;

        /* Costruttore */  
        /// <summary>
        /// Singleton.
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="term"></param>
        /// <returns></returns>
        public static ConflictManager getInstance([Optional] Term term)
        {
            if (singleton == null)
            {
                singleton = new ConflictManager(term);
            }

            return singleton;
        }

        /*// Attraverso l'albero delle gesture e raccolgo per ognuno la lista di oggetti che modificano a tempo 
        /// di esecuzione. Questi dati verranno salvati in un dizionario riportando per ogni term, con una funzione
        /// associata al suo campo CompleteExecute, l'elenco delle variabili su cui agiscono.
        /// Inoltre dalla funzione ottengo anche l'elenco di tutti i modifies che il programma modifica
        public ConflictManager(Object elem, Term term)
        {
            // Inizializzazione liste
            listModifies = new List<Modifies>();
            mapHandlersModifies = new Dictionary<Handler, List<Modifies>>();
            mapConflicts = new Dictionary<Handler, List<Modifies>>();
            mapConflictExec = new Dictionary<Handler, List<Modifies>>();
            // Inizializza la lista degli attributi del programma
            listModifies = elem.GetType().GetCustomAttributes(true).OfType<Modifies>().ToList();
            // Costruisce la map che riporta per ogni funzione la sua lista di modifies
            visitTree(term);
            // Costruisce la map che riporta per ogni funzione la lista di modifies in conflitto con le altre funzioni
            determineConflict();
        }*/
        /// <summary>
        /// Attraverso l'albero delle gesture e raccolgo per ognuno la lista di oggetti che modificano a tempo 
        /// di esecuzione. Questi dati verranno salvati in un dizionario riportando per ogni term, con una funzione
        /// associata al suo campo CompleteExecute, l'elenco delle variabili su cui agiscono.
        /// </summary>
        /// <param name="term"></param>
        private ConflictManager(Term term)
        {
            // Inizializzazione liste
            listModifies = new List<Modifies>();
            mapHandlersModifies = new Dictionary<Handler, List<Modifies>>();
            mapConflicts = new Dictionary<Handler, List<Modifies>>();
            mapConflictExec = new Dictionary<Handler, List<Modifies>>();
            // Inizializza la lista degli attributi del programma
            listModifies = Feedback.listModifies;
            // Costruisce la map che riporta per ogni funzione la sua lista di modifies
            visitTree(term);
            // Costruisce la map che riporta per ogni funzione la lista di modifies in conflitto con le altre funzioni
            determineConflict();
        }

        /* Metodi */
        // Attraverso tutto l'albero e inserisce la coppia di elementi nella mappa
        private void visitTree(Term term)
        {
            // Attraverso l'albero, quindi controllo innanzittutto se il term passato in input è
            // un ground term o composite term
            if (!(term is GroundTerm))
            {
                // Casto il term in Composite Term
                CompositeTerm exp = (CompositeTerm)term;
                // Attraverso tutto l'albero e controllo se ogni term ha associata una funzione per 
                // il suo completamento. Nel caso lo inserisco nel dizionario.
                foreach (Term children in exp.Children())
                {
                    visitTree(children);
                }
            }
            // È un ground term, allora non ha figli, quindi inserisco il suo contenuto nella map.
            try
            {
                if (term.hasComplete())
                {
                    mapHandlersModifies.Add(term.CompleteHandlers.First(), term.CompleteHandlers.First().elementList);
                    //foreach(Handler h in term.handlers)
                    //mapHandlersModifies.Add(h, h.elementList);
                }
            }
            catch(NullReferenceException e)
            {
                throw new Exception("If you add a function to Complete, you must define a Handler object!", e);
            }
        }

        // Per ogni funzione va a determinare dove ci sono i conflitti
        private void determineConflict()
        {
            // Determina tutti gli attributi non in comune e in comune
            foreach(var elem in this.mapHandlersModifies)
            {
                if(listDistinct.Count == 0)
                    listDistinct = listDistinct.Union(elem.Value).ToList();
                else
                {
                    // Memorizza temporaneamente il vecchio contenuto della lista listDistinct
                    List<Modifies> temp = listDistinct.ToList();
                    // Unione con gli attributi della funzione in oggetto.
                    listDistinct = listDistinct.Union(elem.Value).ToList();
                    // Intersezione con la vecchia lista
                    listConflict = listConflict.Union(temp.Intersect(elem.Value)).ToList();
                    // Intersezione con la vecchia lista e Differenza tra le due liste
                    listDistinct = listDistinct.Except(listConflict).ToList();
                }
            }

            // Lista temporanea di Modifies
            List<Modifies> temps = new List<Modifies>();
            // Quindi faccio la differenza con la lista creata in precedenza (così determino la lista degli attributi
            // in comune).
            foreach (var elem in this.mapHandlersModifies)
            {
                // Preleva la lista di attributi su cui agisce l'handler in oggetto
                temps = elem.Value.ToList();
                // Fa la differenza tra la lista temporanea e la lista degli attributi in conflitto
                this.mapConflicts.Add(elem.Key, temps.Except(listDistinct).ToList());
            }
        }

        /// <summary>
        /// Inserisce un handler nella map.
        /// </summary>
        /// <param name="newHandler"></param>
        internal void addHandler(Handler newHandler)
        {
            // Lista di Modifies temporanea, costruita a partire dai CustomAttributes del handler che andrà
            // inserita nella mappa
            List<Modifies> listModifies = new List<Modifies>(newHandler.elementList);

            // Quindi per ogni handler presente nella mappa, si prende la sua lista di CustomAttribute,
            // e si controlla se va ad agire su attributi presenti anche negli altri handler
            foreach (var i in mapConflictExec)
            {
                // Toglie dalla lista di Modifies associato all'Handler gli eventuali elementi in conflitto
                // ovvero quelli contenuti nell'handler della nuova gesture
                foreach (Modifies element in i.Value.Intersect(newHandler.elementList).ToList())
                    i.Value.Remove(element);

                // Contemporaneamente aggiorna anche la lista di Modifies del nuovo handler che dovrà essere
                // inserito
                listModifies = listModifies.Except(i.Key.elementList).ToList();
            }

            // Inserisce l'handler nella mappa
            this.mapConflictExec.Add(newHandler, listModifies);
        }

        /// <summary>
        /// Rimuove il nodo dalla mappa, e aggiorna gli elementi degli eventuali handler ancora rimasti 
        /// nella mappa.
        /// </summary>
        internal void removeHandler(Handler handler)
        {
            // Liste temporanee utilizzate per determinare quali sono i nuovi attributi da inserire
            List<Modifies> list2 = new List<Modifies>();

            // Si determina quali sono i nuovi attributi in conflitto
            foreach (var elem in mapConflictExec)
            {
                if (listModifies.Count() > 0)
                {
                    // Memorizza temporaneamente il vecchio contenuto della lista
                    List<Modifies> temp = listModifies.ToList();
                    // Unione con gli attributi della funzione in oggetto.
                    listModifies = listModifies.Union(elem.Value).ToList();
                    // Intersezione con la vecchia lista
                    list2 = list2.Union(temp.Intersect(elem.Value)).ToList();
                    // Differenza tra le due liste
                    listModifies = listModifies.Except(list2).ToList();
                }
                else// Inizializzazione mappa
                {
                    listModifies.Union(elem.Value);
                }
            }

            /*// Prima si determina quali sono i nuovi attributi in conflitto
            foreach (var i in this.mapHandler)
            {
                if (listModifies.Count > 0)
                    listModifies.Union(i.Key.elementList);
                else
                    listModifies.Intersect(i.Key.elementList);
            }*/

            // Quindi si fa l'union con la lista degli elementi di partenza e poi se ne fa la differenza
            foreach (var i in this.mapConflictExec)
            {
                foreach (Modifies element in i.Key.elementList.Except(listModifies).Except(i.Value).ToList())
                {
                    i.Value.Add(element);
                }
            }

            mapConflictExec.Remove(handler);
        }
    }
}
