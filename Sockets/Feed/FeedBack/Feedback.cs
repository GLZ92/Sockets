using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Unica.Djestit.Feed
{
    public class Feedback
    {
        /* Attributi */
        // Albero dei Feedback costruito a partire dalle gesture
        public FeedbackRoot tree { get; private set; }
        // Lista degli attributi personalizzati che possono essere modificati
        public static List<Modifies> listModifies { get; set; }
        // Singleton
        private static Feedback singleton = null;

        /* Costruttore */
        private Feedback(Term expr)
        {
            // Creo l'albero dei Feedback
            tree = new FeedbackRoot(expr);
        }

        public static Feedback getInstance([Optional] Term expr)
        {
            if (singleton == null)
            {
                singleton = new Feedback(expr);
            }

            return singleton;
        }
    }
}
