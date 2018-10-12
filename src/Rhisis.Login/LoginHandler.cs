using Ether.Network.Packets;
using NLog;
using Rhisis.Core.Common;
using Rhisis.Core.DependencyInjection;
using Rhisis.Core.Services;
using Rhisis.Login.Packets;
using Rhisis.Network;
using Rhisis.Network.Packets;
using Rhisis.Network.Packets.Login;

namespace Rhisis.Login
{
    public static class LoginHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        [PacketHandler(PacketType.PING)]
        public static void OnPing(LoginClient client, INetPacketStream packet)
        {
            var pak = new PingPacket(packet);

            if (!pak.IsTimeOut)
                CommonPacketFactory.SendPong(client, pak.Time);
        }

        [PacketHandler(PacketType.CERTIFY)]
        public static void OnLogin(LoginClient client, INetPacketStream packet)
        {
            var loginServerConfiguration = client.LoginServer.ServerConfiguration;
            var cryptographyService = DependencyContainer.Instance.Resolve<ICryptographyService>();
            var certifyPacket = new CertifyPacket(packet, loginServerConfiguration.PasswordEncryption);
            string password = null;

            if (certifyPacket.BuildVersion != loginServerConfiguration.BuildVersion)
            {
                AuthenticationFailed(client, ErrorType.CERT_GENERAL, "bad client build version");
                return;
            }

            if (loginServerConfiguration.PasswordEncryption)
            {
                byte[] encryptionKey = cryptographyService.BuildEncryptionKeyFromString(loginServerConfiguration.EncryptionKey, 16);

                password = cryptographyService.Decrypt(certifyPacket.EncryptedPassword, encryptionKey);
            }
            else
            {
                password = packet.Read<string>();
            }

            var authenticationService = DependencyContainer.Instance.Resolve<IAuthenticationService>();
            var authenticationResult = authenticationService.Authenticate(certifyPacket.Username, password);

            switch (authenticationResult)
            {
                case AuthenticationResult.BadUsername:
                    AuthenticationFailed(client, ErrorType.FLYFF_ACCOUNT, "bad username");
                    break;
                case AuthenticationResult.BadPassword:
                    AuthenticationFailed(client, ErrorType.FLYFF_PASSWORD, "bad password");
                    break;
                case AuthenticationResult.AccountSuspended:
                case AuthenticationResult.AccountTemporarySuspended:
                    // TODO
                    break;
                case AuthenticationResult.Success:

                    if (client.LoginServer.IsClientConnected(certifyPacket.Username))
                    {
                        AuthenticationFailed(client, ErrorType.DUPLICATE_ACCOUNT, "client already connected", false);
                        return;
                    }

                    LoginPacketFactory.SendServerList(client, certifyPacket.Username, client.LoginServer.ClustersConnected);
                    client.SetClientUsername(certifyPacket.Username);
                    Logger.Info($"User '{client.Username}' logged succesfully from {client.RemoteEndPoint}.");
                    break;
            }
        }

        [PacketHandler(PacketType.CLOSE_EXISTING_CONNECTION)]
        public static void OnCloseExistingConnection(LoginClient client, INetPacketStream packet)
        {
            var closeConnectionPacket = new CloseConnectionPacket(packet);
            var otherConnectedClient = client.LoginServer.GetClientByUsername(closeConnectionPacket.Username);

            if (otherConnectedClient == null)
            {
                Logger.Warn($"Cannot find user with username '{closeConnectionPacket.Username}'.");
                return;
            }

            otherConnectedClient.Disconnect();
        }

        private static void AuthenticationFailed(LoginClient client, ErrorType error, string reason, bool disconnected = true)
        {
            Logger.Warn($"Unable to authenticate user from {client.RemoteEndPoint}. Reason: {reason}");
            LoginPacketFactory.SendLoginError(client, error);

            if (disconnected)
               client.Disconnect();
        }
    }
}