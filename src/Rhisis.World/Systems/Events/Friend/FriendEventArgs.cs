using System;
using Rhisis.World.Systems.Events.Statistics;

namespace Rhisis.World.Systems.Events.Friend
{
    public class FriendEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="FriendActionType"/> action type to execute.
        /// </summary>
        public FriendActionType ActionType { get; }

        /// <summary>
        /// Gets the <see cref="FriendActionType"/> optional arguments.
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Creates a new <see cref="FriendActionType"/> instance.
        /// </summary>
        /// <param name="type">Action type to execute</param>
        /// <param name="args">Optional arguments</param>
        public FriendEventArgs(FriendActionType type, params object[] args)
        {
            this.ActionType = type;
            this.Arguments = args;
        }
    }
}