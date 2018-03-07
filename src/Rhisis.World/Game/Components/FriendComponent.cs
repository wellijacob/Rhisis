using System;
using System.Collections.Generic;
using System.Text;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Structures;
using Rhisis.World.Systems;

namespace Rhisis.World.Game.Components
{
    public class FriendComponent
    {
        public Dictionary<int, PlayerData> Friends { get; }

        public FriendComponent()
        {
            this.Friends = new Dictionary<int, PlayerData>();
        }
    }
}
