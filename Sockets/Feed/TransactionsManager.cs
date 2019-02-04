using System;
using System.Collections.Generic;
// Gestione Transazioni
using System.Transactions;
// Modifies
using Unica.Djestit.Feed;
// Handler

namespace Unica.Djestit.Concurrency
{
    /// <summary>
    ///
    /// </summary>
    public class TransactionsManager : IEnlistmentNotification
    {
        /* Attributi */
        // Indica se è già in atto una transazione
        //private bool isDoTransactions;
        // Transazione attiva
        private static Transaction transaction;
        // 
        private Modifies memberValue = default(Modifies);
        private Modifies oldMemberValue = default(Modifies);
        private Modifies newSuggestedValue = default(Modifies);
        //
        private List<Tuple<Modifies, Modifies>> listMember = new List<Tuple<Modifies, Modifies>>();
        //
        //private List<Modifies> listMemberValue = new List<Modifies>();

        /* Costruttore */
        public TransactionsManager()
        {

        }

        /* Metodi */
        /// <summary>
        /// Avvia l'esecuzione della transazione.
        /// </summary>
        /// <param name="listMemberValue"></param>
        /// <param name="listNewMemberValue"></param>
        public void onTransactionExcute(List<Modifies> listMemberValue, List<Modifies> listNewMemberValue)
        {
            transaction = Transaction.Current;
            if (transaction != null)
            {
                // Eseguiamo la transazione
                // Accoda le operazioni alla transazione 
                transaction.EnlistVolatile(this, EnlistmentOptions.None);
                // Inserisce nel Modifies da modificare il nuovo valore che deve assumere
                foreach(Modifies mod in listNewMemberValue)
                {
                    Modifies modTemp = (listMemberValue.Find(item => item.Equals(mod)));
                    if (modTemp != null)
                        this.listMember.Add(new Tuple<Modifies, Modifies>(modTemp, mod));                    
                }
            }
            else
            {
                // we are outside a transaction, immediate commit
                foreach (Modifies mod in listNewMemberValue)
                {
                    Modifies modTemp = (listMemberValue.Find(item => item.Equals(mod)));
                    if (modTemp != null)
                        modTemp.setValue(mod.newValue);
                }
            }
        }

        /// <summary>
        /// Esegue il commit.
        /// </summary>
        /// <param name="enlistment"></param>
        public void Commit(Enlistment enlistment)
        {
            // Sto eseguendo il commit della transazione, svuoto la lista delle modifiche
            this.listMember.Clear();
        }
        /// <summary>
        /// Viene richiamata quando la funzione non è stata eseguita.
        /// </summary>
        /// <param name="enlistment"></param>
        public void InDoubt(Enlistment enlistment)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Prepara l'esecuzione della transizione
        /// </summary>
        /// <param name="preparingEnlistment"></param>
        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            // La transazione è pronta per essere eseguita. Di seguito vengono riportate le operazione da
            // eseguire. Associo il vecchio valore a quello nuovo.
            foreach(var modifie in this.listMember)
            {
                modifie.Item1.setValue(modifie.Item2.newValue);
            }
            // Preparo l'enlistment
            preparingEnlistment.Prepared();
        }
        /// <summary>
        /// Esegue il rollback della transizione. Vuol dire che qualcosa non è andata a buon fine.
        /// </summary>
        /// <param name="enlistment"></param>
        public void Rollback(Enlistment enlistment)
        {
            // Inserisce il valore precedente
            foreach (var modifie in this.listMember)
            {
                modifie.Item1.rollback();
            }
            // Svuoto la lista delle modifiche non andate a buon fine
            this.listMember.Clear();
        }

        /// <summary>
        /// Riporta per ogni modifies la modifica che dev'essere effettuata
        /// </summary>
        /// <param name="list1"></param>
        /// <param name="list2"></param>
        /// <returns></returns>
        protected List<Modifies> setListTemporary(List<Modifies> list1, List<Modifies> list2)
        {
            List<Modifies> list3 = new List<Modifies>();

            foreach (Modifies mod in list1)
            {
                Modifies i = list1.Find(item => item.Equals(mod));
                if (i != null)
                {
                    Modifies temp = new Modifies(i.name, i.value, mod.value);
                    list3.Add(temp);
                }
            }
            return list3;
        }

    }
}