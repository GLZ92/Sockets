using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Unica.Djestit
{
    // Delegate dell'evento che comunica la gestione di uno SkeletonToken
    public delegate void SensorEventHandler(object obj, EventArgs sender);

    public abstract class Sensor
    {
        // Eventi generati all'arrivo di un scheletro
        public event SensorEventHandler OnTokenStart;
        public event SensorEventHandler OnTokenMove;
        public event SensorEventHandler OnTokenEnd;
        
        /* Attributi  */
        public int capacity { get; private set; }
        public Term root { get; private set; }

        public Sensor(Term root, int capacity)
        {
            this.capacity = capacity;
            this.root = root;
        }

        /* Handler eventi di start, move ed end */
        /// <summary>
        /// Handler per l'evento start
        /// </summary>
        /// <param name="sender"></param>
        public virtual void _onTokenStart(EventArgs sender)
        {
            if (OnTokenStart != null)
                OnTokenStart(this, sender);
        }
        
        /// <summary>
        /// Handler per l'evento move
        /// </summary>
        /// <param name="sender"></param>
        public virtual void _onTokenMove(EventArgs sender)
        {
            if (OnTokenMove != null)
                OnTokenMove(this, sender);
        }
        
        /// <summary>
        /// Handler per l'evento end
        /// </summary>
        /// <param name="sender"></param>
        public virtual void _onTokenEnd(EventArgs sender)
        {
            if (OnTokenEnd != null)
                OnTokenEnd(this, sender);
        }

        /// <summary>
        /// Verifica se in state sequence è già presente quello elemento
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual bool checkId(int id)
        {
            return false;
        }
    }
}
