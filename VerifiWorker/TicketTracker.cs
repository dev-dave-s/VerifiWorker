using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VerifiWorker
{
    public class TicketTracker
    {
        private List<string> _tickets;

        public TicketTracker()
        {
            _tickets = new List<string>();
        }

        public void AddTicket(string ticket)
        {
            _tickets.Add(ticket);
        }

        public bool IsProcessed(string ticket)
        {
            return _tickets.Any(x => x.Equals(ticket));
        }
    }
}
