using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Rhisis.Core.IO;
using Rhisis.World.Core.Systems;
using Rhisis.World.Game.Components;
using Rhisis.World.Game.Core;
using Rhisis.World.Game.Core.Interfaces;
using Rhisis.World.Game.Entities;
using Rhisis.World.Systems.Events.Friend;
using Rhisis.World.Systems.Events.Statistics;

namespace Rhisis.World.Systems
{
    /// <summary>
    /// Friend system
    /// </summary>
    [GlobalSystem]
    public class FriendSystem : NotifiableSystemBase
    {
        /// <summary>
        /// Gets the <see cref="FriendSystem"/> match filter.
        /// </summary>
        protected override Expression<Func<IEntity, bool>> Filter => x => x.Type == WorldEntityType.Player;

        public FriendSystem(IContext context) :
            base(context)
        {
        }

        public override void Execute(IEntity entity, EventArgs e)
        {
            if (!(e is FriendEventArgs friendEvent))
                return;

            var playerEntity = entity as IPlayerEntity;

            Logger.Debug("Execute statistics action: {0}", friendEvent.ActionType.ToString());

            switch (friendEvent.ActionType)
            {
                case FriendActionType.Initialize:
                    break;
                case FriendActionType.Unknown:
                    // Nothing to do.
                    break;
                default:
                    Logger.Warning("Unknown friend action type: {0} for player {1} ",
                        friendEvent.ActionType.ToString(), entity.ObjectComponent.Name);
                    break;
            }
        }

        private void Initialize(IPlayerEntity player, object[] args)
        {
            Logger.Debug("Initialize friends");

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (args.Length < 0)
                throw new ArgumentException("Friends event arguments cannot be empty.", nameof(args));
            
            player.FriendsComponent = new FriendComponent();

            if (args[0] is IEnumerable<Database.Structures.Friend> dbFriends && dbFriends.Any())
            {
                foreach (var friend in dbFriends)
                {
                    player.FriendsComponent.Friends.TryAdd(friend.Id, PlayerDataSystem.Players.Values.ElementAt(friend.Id));
                }
            }
        }
    }
}
