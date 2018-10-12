using Ether.Network.Server;
using Rhisis.Core.Structures.Configuration;
using Rhisis.Network.ISC.Structures;
using System.Collections.Generic;

namespace Rhisis.Login
{
    public interface ILoginServer : INetServer
    {
        /// <summary>
        /// Gets the login server configuration.
        /// </summary>
        LoginConfiguration ServerConfiguration { get; }

        /// <summary>
        /// Gets the list of the connected clusters.
        /// </summary>
        IEnumerable<ClusterServerInfo> ClustersConnected { get; }

        /// <summary>
        /// Gets a connected client by his username.
        /// </summary>
        /// <param name="username">Client username</param>
        /// <returns></returns>
        LoginClient GetClientByUsername(string username);

        /// <summary>
        /// Verify if a client is connected to the login server.
        /// </summary>
        /// <param name="username">Client username</param>
        /// <returns></returns>
        bool IsClientConnected(string username);
    }
}