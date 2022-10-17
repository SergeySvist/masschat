using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace name_server
{
    internal class ClientMessageEventArgs : EventArgs
    {
        public DateTime Date { get; set; }
        public string Message { get; set; }

        public ClientMessageEventArgs(DateTime date, string message)
        {
            Date = date;
            Message = message;
        }
    }

}
