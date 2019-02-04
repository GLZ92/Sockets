
namespace Unica.Djestit.Feed
{
    public class FeedbackLeaf : FeedbackGroup
    {
        /* Attributi */

        /* Costruttore */
        public FeedbackLeaf(Term term) : base(term)
        {
            // Handler che aggiorna lo stato del term quando questo viene aggiornato
            this.term.ChangeState += updateTerm;
        }

        /* Metodi */
    }
}
