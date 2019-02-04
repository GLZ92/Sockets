
namespace Unica.Djestit.Feed
{
    public class FeedbackGroupEventArgs
    {
        /* Attributi */
        // Term relativo al wrapper inviato
        public Term term { get; private set; }
        // Group che descrive la gesture
        public FeedbackGroup group { get; private set; }
        // Handler associata alla gesture
        public Handler handler { get; private set; }

        /* Costruttore */
        public FeedbackGroupEventArgs(Term term, FeedbackGroup group, Handler handler)
        {
            this.term = term;
            this.group = group;
            this.handler = handler;
        }
    }
}
