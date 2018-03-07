using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Rhisis.Core.IO;
using Rhisis.Database;
using Rhisis.Database.Structures;
using Rhisis.World.Core.Systems;
using Rhisis.World.Game.Core;
using Rhisis.World.Game.Core.Interfaces;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Structures;

namespace Rhisis.World.Systems
{
    /// <summary>
    /// Player data system
    /// Primary used to serve offline player data to connected players
    /// </summary>
    [GlobalSystem]
    public class PlayerDataSystem : SystemBase
    {
        /// <summary>
        /// Gets the <see cref="PlayerDataSystem"/> match filter.
        /// </summary>
        protected override Expression<Func<IEntity, bool>> Filter => x => x.Type == WorldEntityType.Player;

        public static Dictionary<int, PlayerData> Players { get; } = new Dictionary<int, PlayerData>();

        private static object _playersLock = new object();

        public PlayerDataSystem(IContext context) :
            base(context)
        {
        }

        public static void AddPlayer(IPlayerEntity entity)
        {
            lock (_playersLock)
            {
                Players.TryAdd(entity.Id, new PlayerData()
                {
                    Id = entity.Id,
                    Name = entity.ObjectComponent.Name,
                    Gender = entity.HumanComponent.Gender,
                    Job = (byte) 0,
                    Level = (ushort) 0
                });
            }
        }

        public static void AddPlayer(Character player)
        {
            lock (_playersLock)
            {
                Players.TryAdd(player.Id, new PlayerData()
                {
                    Id = player.Id,
                    Name = player.Name,
                    Gender = player.Gender,
                    Job = (byte)player.ClassId,
                    Level = (ushort)player.Level
                });
            }
        }

        public static void LoadPlayerData()
        {
            Logger.Loading("Loading player data...\t\t");

            using (var db = DatabaseService.GetContext())
            {
                var players = db.Characters.GetAll();

                foreach (var player in players)
                {
                    AddPlayer(player);
                }
            }
        }
    }
}