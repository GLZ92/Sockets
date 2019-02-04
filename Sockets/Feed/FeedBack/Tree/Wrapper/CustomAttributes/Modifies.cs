using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Reflection
using System.Reflection;

namespace Unica.Djestit.Feed
{
    // 
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class Modifies : Attribute, ICloneable
    {
        /* Attributi */
        // Nome dell'oggetto
        public string name { get; private set; }
        // Oggetto in questione
        public object value { get; private set; }
        public object newValue { get; private set; }
        public object oldValue { get; private set; }

        /* Costruttore */
        public Modifies(string name, object value)
        {
            this.name = name;
            this.value = value;
            this.oldValue = value;
        }
        // Creazione di un Modifies
        public Modifies(string name, object value, object newValue)
        {
            this.name = name;
            this.value = value;
            this.oldValue = value;
            this.newValue = newValue;
        }

        /* Metodi */
        /// <summary>
        /// Clona l'oggeto di tipo Modifies
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            /*String nameClone = name;
            Object valueClone = value.Clone();
            Object newValueClone = newValue.Clone();
            Modifies clone = new Modifies(name, valueClone, newValueClone);
            return clone;*/
            return this.MemberwiseClone();
        }

        public override bool Equals(Object obj)
        {
            Modifies mod = (Modifies)obj;
            if (this.name == mod.name)
                return true;

            return false;
        }

        public virtual void setValue(Object newValue)
        {
            this.oldValue = this.value;
            this.value = newValue;
        }

        public virtual void rollback()
        {
            this.value = oldValue;
        }
        /* Abstract Methods */
        /// <summary>
        /// Viene utilizzata per definire una modifica all'interno dell'oggeto.
        /// </summary>
        //public abstract void set();
        /// <summary>
        /// Viene utilizzata per ripristinare il valore precedente. Necessario in caso di errore
        /// </summary>
        //public abstract void refresh();
        /// <summary>
        /// Verfica se due modifies sono uguali
        /// </summary>
        //public abstract bool equals(Modifies mod);

        /* Example Methods */
        /*public void setAttr(object newValue)
        {
            if (this.newValue != null)
            {
                this.newValue = newValue;
                setAttr();
            }
            else
            {
                // Comunico all'utente che non ha inserito un nuovo valore
                throw new InvalidModifiesException("Non è stato inserito nessun nuovo valore");
            }
        }*/
    }

    // Comparer per la classe Modifies
    class ModifiesComparer : IEqualityComparer<Modifies>
    {
        // Products are equal if their names and product numbers are equal.
        public bool Equals(Modifies x, Modifies y)
        {
            // Controlla se i due Modifies puntano alla stesso oggetto, restituisce true.
            if (Object.ReferenceEquals(x, y)) 
                return true;
            
            // Se uno dei due Modifies puntano ad un oggetto nullo, restituisce false.
            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            // Controlla se i due Modifies modificano lo stesso oggetto
            if ((x.value != null && y.value != null) && (x.value == y.value))
                return true;
            // O se hanno lo stesso nome
            if ((x.name == null) || (y.name == null) && (x.name == y.name))
                return true;

            return false;
        }

        // Calcola l'hashCode di un oggetto di tipo Modifies
        public int GetHashCode(Modifies element)
        {
            // Controlla se l'oggetto è nullo
            if (Object.ReferenceEquals(element, null)) return 0;

            // Se l'oggetto non è nullo allora calcolo l'hashcode dei singoli attributi dell'oggetto stesso
            int hashName = element.name == null ? 0 : element.name.GetHashCode();
            // Calcola l'hashcode dell'oggetto
            int hashValue = element.value == null ? 0 : element.value.GetHashCode();
            // Calcola l'hashcode dell'oggetto
            int hashNewValue = element.newValue == null ? 0 : element.newValue.GetHashCode();

            //Calculate the hash code for the product.
            return hashName ^ hashValue ^ hashNewValue;
        }
    }

}