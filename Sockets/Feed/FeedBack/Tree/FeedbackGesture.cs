using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace Unica.Djestit.Feed
{
    public class FeedbackGesture : FeedbackGroup
    {
        /* Attributi */
        // Lista dei Ground Term associati alla gesture
        public List<FeedbackLeaf> groundTermChildren = new List<FeedbackLeaf>();
        // Lista dei Composite Term associati alla gesture
        public List<FeedbackComposite> compositeTermChildren = new List<FeedbackComposite>();
        // Delta utilizzato per aggiornare la probabilità di una gesture
        private float delta = 0.001f;

        /* Costruttore */
        public FeedbackGesture(Term term) : base(term)
        {

            // Creo la lista dei Ground Term associati qualora il Term sia un Composite Term
            if (term.GetType() != typeof(GroundTerm))
            {
                // Casto il composite term
                CompositeTerm compositeTerm = (CompositeTerm)term;
                // Naviga all'interno dell'albero, che definisce il Composite Term in questione, 
                // per selezionare tutti i Ground Term che lo descrivono.
                foreach (var child in compositeTerm.Children())
                {
                    // Lo aggiungo nella lista dei children
                    create(child);
                }
            }
            
            // Handler relativi all'aggiornamento di stato del term e al suo fire
            this.term.TokenFire += updateChild;
            this.term.ChangeState += updateTerm;
        }

        /* Metodi */
        /// <summary>
        /// Quando il term associato al FeedbackGesture "spara" un token, si provvede
        /// a controllare se il FeedbackGesture può essere posto in uno stato di Continue.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="sender"></param>
        private void updateChild(object obj, TokenFireArgs sender)
        {
            // Se il FeedbackGesture ha almeno un figlio nello stato di complete e se il suo stato non è in Complete o in Error
            if (this.groundTermChildren.Where(this.IsComplete).Count() > 0 && this.state != StateGroup.Complete && this.state != StateGroup.Error)
            {
                this.state = StateGroup.Continue;
                OnFeedbackGroupContinue();
            }
        }

        /// <summary>
        /// La funzione Create provvede ricorsivamente a popolare le due liste GroundTermChildren e 
        /// CompositeTermChildren. La prima lista contiene tutti i Ground Term che definiscono quel
        /// Composite Term, la seconda lista contiene invece i Composite Term interni sempre al 
        /// Composite Term.
        /// </summary>
        /// <param name="term"></param>
        public List<FeedbackLeaf> create(Term term)
        {
            // Se il Term in oggetto è un CompositeTerm, allora continuo ad attraversare l'albero
            if (!(term is GroundTerm))
            {
                // Casto il Composite Term
                CompositeTerm compositeTerm = (CompositeTerm)term;
                // Creo il FeedbackComposite legato al Composite Term trattato
                FeedbackComposite feedComposite = new FeedbackComposite(term);
                // Per ogni child richiama se stesso 
                foreach (var child in compositeTerm.Children())
                    concat(feedComposite.groundTermChildren, create(child));
                // Inserisce nella lista CompositeTermChildren il Composite in esame
                this.compositeTermChildren.Add(feedComposite);
                // Restituisce la lista
                return feedComposite.groundTermChildren;
            }
            else
            {
                // Se invece il Term in oggetto è un un Ground Term, allora lo si inserisce
                // nella lista apposita
                FeedbackLeaf leaf = new FeedbackLeaf(term);
                this.groundTermChildren.Add(new FeedbackLeaf(term));
                // E lo si restituisce.
                List<FeedbackLeaf> list = new List<FeedbackLeaf>();
                list.Add(leaf);
                return list;
            }
        }

        /// <summary>
        /// Controlla se nella lista passata in input, tutti gli elementi in esso presenti sono in Complete.
        /// Se si, allora ritorna true, altrimenti ritorna false.
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public bool isAllComplete(List<FeedbackGroup> list)
        {
            // Per ogni elemento della lista passata in input
            foreach(FeedbackGroup child in list)
            {
                // Verfica lo stato, se è diverso da Complete allora ritorna false
                if (child.state != StateGroup.Complete)
                    return false;
            }
            // Tutti gli elementi della lista sono in true
            return true;
        }

        /// <summary>
        /// Resetta il FeedbackGroup e i suoi figli (pone il suo stato a Default)
        /// </summary>
        internal override void reset()
        {
            base.reset();
            foreach (var child in this.groundTermChildren)
                child.reset();
            foreach (var child in this.compositeTermChildren)
                child.reset();
        }

        /// <summary>
        /// Concatena due liste.
        /// </summary>
        public void concat(List<FeedbackLeaf> list, List<FeedbackLeaf> children)
        {
            foreach (var child in children)
            {
                list.Add(child);
            }
        }

        public override void updateTerm()
        {
            if (this.term.state == expressionState.Complete)
            {
                // Complete
                this.state = StateGroup.Complete;// Cambio lo stato
                // Aggiorno la probabilità sommandogli un piccolo delta
                //this.likelihood.updateLikelihood(this.likelihood.probability + this.delta);
                //this.likelihood += this.delta;
                this.OnFeedbackGroupComplete();// Genero l'evento
            }
            else
            {
                // Error
                this.state = StateGroup.Error;// Cambio lo stato
                this.OnFeedbackGroupError();// Genero l'evento
            }
        }
    }
}
