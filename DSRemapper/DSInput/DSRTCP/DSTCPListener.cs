using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DSRemapper.DSInput.DSRTCP
{
    internal class DSTCPListener:IDisposable
    {
        TcpListener server = new(IPAddress.Any, 1234);

        List<TcpClient> clients = new List<TcpClient>();

        public DSTCPListener()
        {
            server.Start();
        }

        public void Dispose()
        {
            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close();
            }
            server.Stop();
        }

        public List<TcpClient> RefreshClients()
        {
            while (server.Pending())
                clients.Add(server.AcceptTcpClient());

            for (int i = 0; i < clients.Count; i++)
            {
                if (!clients[i].Connected)
                {
                    clients[i].Close();
                    clients.RemoveAt(i);
                    i--;
                }
            }

            return clients;
        }

    }
}
