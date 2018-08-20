using Ether.Network.Packets;
using NLog;
using Rhisis.Core.Common;
using Rhisis.Core.IO;
using Rhisis.Core.Network;
using Rhisis.Core.Network.Packets;
using Rhisis.Core.Network.Packets.World;
using Rhisis.Core.Structures;
using Rhisis.Database;
using Rhisis.Database.Entities;
using Rhisis.World.Game.Components;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Maps;
using Rhisis.World.Packets;
using Rhisis.World.Systems.Inventory;
using Rhisis.World.Systems.Inventory.EventArgs;
using System;
using System.Linq;

namespace Rhisis.World.Handlers
{
    public static class JoinGameHandler
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private const string UnableToJoinMessage = "Unable to join character '{0}' for user '{1}' from {2}. Reason: {3}.";
        private const string UnableToJoinSecurityMessage = "[SECURITY] " + UnableToJoinMessage;

        [PacketHandler(PacketType.JOIN)]
        public static void OnJoin(WorldClient client, INetPacketStream packet)
        {
            var pak = new JoinPacket(packet);
            Character dbCharacter = null;

            using (var db = DatabaseService.GetContext())
            {
                dbCharacter = db.Characters.Get(pak.PlayerId);
            }

            // Check if the character exist.
            if (dbCharacter == null)
            {
                Logger.Warn(UnableToJoinSecurityMessage, dbCharacter.Name, dbCharacter.Name,
                    client.RemoteEndPoint, "character is not exist");
                return;
            }

            // Check if given username is the real owner of this character.
            if (!pak.Username.Equals(dbCharacter.User.Username, StringComparison.OrdinalIgnoreCase))
            {
                Logger.Warn(UnableToJoinSecurityMessage, dbCharacter.Name, dbCharacter.Name, 
                    client.RemoteEndPoint, "character is not owned by this user");
                return;
            }

            // Check if the account is banned.
            if (dbCharacter.User.Authority <= 0)
            {
                Logger.Warn(UnableToJoinMessage, dbCharacter.Name, dbCharacter.Name,
                    client.RemoteEndPoint, "character is banned");
                return;
            }
            
            // Check if character's map is loaded.
            if (!WorldServer.Maps.TryGetValue(dbCharacter.MapId, out IMapInstance map))
            {
                Logger.Warn(UnableToJoinMessage, dbCharacter.Name, dbCharacter.Name,
                    client.RemoteEndPoint, $"map id '{dbCharacter.MapId}' of character is not found");
                return;
            }

            IMapLayer mapLayer = map.GetMapLayer(dbCharacter.MapLayerId) ?? map.GetDefaultMapLayer();

            // 1st: Create the player entity with the map context.
            client.Player = map.CreateEntity<PlayerEntity>();

            // 2nd: create and initialize components.
            client.Player.Object = new ObjectComponent
            {
                ModelId = dbCharacter.Gender == 0 ? 11 : 12, //TODO: implement game constants.
                Type = WorldObjectType.Mover,
                MapId = dbCharacter.MapId,
                LayerId = mapLayer.Id,
                Position = new Vector3(dbCharacter.PosX, dbCharacter.PosY, dbCharacter.PosZ),
                Angle = dbCharacter.Angle,
                Size = 100,
                Name = dbCharacter.Name,
                Spawned = false,
                Level = dbCharacter.Level
            };

            client.Player.VisualAppearance = new VisualAppearenceComponent
            {
                Gender = dbCharacter.Gender,
                SkinSetId = dbCharacter.SkinSetId,
                HairId = dbCharacter.HairId,
                HairColor = dbCharacter.HairColor,
                FaceId = dbCharacter.FaceId,
            };

            client.Player.PlayerData = new PlayerDataComponent
            {
                Id = dbCharacter.Id,
                Slot = dbCharacter.Slot,
                Gold = dbCharacter.Gold,
                Authority = (AuthorityType)dbCharacter.User.Authority
            };

            client.Player.MovableComponent = new MovableComponent
            {
                Speed = WorldServer.Movers[client.Player.Object.ModelId].Speed,
                DestinationPosition = client.Player.Object.Position.Clone(),
                LastMoveTime = UnixDateTime.GetElapsedTime(),
                NextMoveTime = UnixDateTime.GetElapsedTime() + 10
            };

            client.Player.Statistics = new StatisticsComponent(dbCharacter);
            client.Player.Behavior = WorldServer.PlayerBehaviors.DefaultBehavior;
            client.Player.Connection = client;

            // Initialize the inventory
            var inventoryEventArgs = new InventoryInitializeEventArgs(dbCharacter.Items);
            client.Player.NotifySystem<InventorySystem>(inventoryEventArgs);
            
            // 3rd: spawn the player
            WorldPacketFactory.SendPlayerSpawn(client.Player);

            // 4th: player is now spawned
            client.Player.Object.Spawned = true;
        }
    }
}
