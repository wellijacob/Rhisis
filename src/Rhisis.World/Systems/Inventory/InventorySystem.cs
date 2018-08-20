using NLog;
using Rhisis.Core.Data;
using Rhisis.Core.Extensions;
using Rhisis.Core.Structures.Game;
using Rhisis.World.Game.Components;
using Rhisis.World.Game.Core;
using Rhisis.World.Game.Entities;
using Rhisis.World.Game.Structures;
using Rhisis.World.Packets;
using Rhisis.World.Systems.Inventory.EventArgs;
using System;
using System.Collections.Generic;

namespace Rhisis.World.Systems.Inventory
{
    [System]
    public class InventorySystem : NotifiableSystemBase
    {
        public const int RightWeaponSlot = 52;
        public const int EquipOffset = 42;
        public const int MaxItems = 73;
        public const int InventorySize = EquipOffset;
        public const int MaxHumanParts = MaxItems - EquipOffset;

        private const string UnableToEquipMessage = "Unable to equip item '{0}' for player '{1}' from '{2}'. Reason: {3}.";
        private const string UnableToUnequipMessage = "Unable to unequip item '{0}' for player '{1}' from '{2}'. Reason: {3}.";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <inheritdoc />
        protected override WorldEntityType Type => WorldEntityType.Player;

        /// <summary>
        /// Creates a new <see cref="InventorySystem"/> instance.
        /// </summary>
        /// <param name="context"></param>
        public InventorySystem(IContext context)
            : base(context)
        {
        }

        /// <inheritdoc />
        public override void Execute(IEntity entity, SystemEventArgs e)
        {
            if (!(entity is IPlayerEntity playerEntity))
                return;
            
            if (!e.CheckArguments())
            {
                Logger.Error(UnableToExecuteMessage, playerEntity.Object.Name, 
                    playerEntity.Connection.Socket.RemoteEndPoint, $"invalid arguments for '{e.GetType()}'");
                return;
            }

            switch (e)
            {
                case InventoryInitializeEventArgs inventoryInitializeEvent:
                    this.InitializeInventory(playerEntity, inventoryInitializeEvent);
                    break;
                case InventoryMoveEventArgs inventoryMoveEvent:
                    this.MoveItem(playerEntity, inventoryMoveEvent);
                    break;
                case InventoryEquipEventArgs inventoryEquipEvent:
                    this.EquipItem(playerEntity, inventoryEquipEvent);
                    break;
                case InventoryCreateItemEventArgs inventoryCreateItemEventArgs:
                    this.CreateItem(playerEntity, inventoryCreateItemEventArgs);
                    break;
                default:
                    Logger.Warn(UnableToExecuteMessage, playerEntity.Object.Name, 
                        playerEntity.Connection.Socket.RemoteEndPoint, $"Unknown action for '{e.GetType()}'");
                    break;
            }
        }

        /// <summary>
        /// Initialize the player's inventory.
        /// </summary>
        /// <param name="player">Current player</param>
        /// <param name="e"></param>
        private void InitializeInventory(IPlayerEntity player, InventoryInitializeEventArgs e)
        {
            player.Inventory = new ItemContainerComponent(MaxItems, InventorySize);

            // We populate the inventory with player items.
            if (e.Items != null)
            {
                foreach (Database.Entities.Item item in e.Items)
                {
                    int uniqueId = player.Inventory.Items[item.ItemSlot].UniqueId;

                    player.Inventory.Items[item.ItemSlot] = new Item(item)
                    {
                        UniqueId = uniqueId,
                    };
                }
            }

            // For extra items slot, we put empty value for unique id.
            for (int i = EquipOffset; i < MaxItems; ++i)
            {
                if (player.Inventory.Items[i].Id == -1)
                    player.Inventory.Items[i].UniqueId = -1;
            }
        }

        /// <summary>
        /// Move an item.
        /// </summary>
        /// <param name="player"></param>
        private void MoveItem(IPlayerEntity player, InventoryMoveEventArgs e)
        {
            var sourceSlot = e.SourceSlot;
            var destinationSlot = e.DestinationSlot;
            List<Item> items = player.Inventory.Items;

            if (sourceSlot >= MaxItems || destinationSlot >= MaxItems)
                return;
            if (items[sourceSlot].Id == -1 || items[sourceSlot].UniqueId == -1 || items[destinationSlot].UniqueId == -1)
                return;
            
            Item sourceItem = items[sourceSlot];
            Item destItem = items[destinationSlot];
            
            Logger.Debug("Move item from slot {0} to {1} for player '{2}' from {3}.", 
                sourceSlot, destinationSlot, player.Object.Name, player.Connection.Socket.RemoteEndPoint);

            if (sourceItem.Id == destItem.Id && sourceItem.Data.IsStackable)
            {
                // TODO: stack items
            }
            else
            {
                sourceItem.Slot = destinationSlot;

                if (destItem.Slot != -1)
                    destItem.Slot = sourceSlot;

                items.Swap(sourceSlot, destinationSlot);
                WorldPacketFactory.SendItemMove(player, sourceSlot, destinationSlot);
            }
        }

        /// <summary>
        /// Equips an item.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="e"></param>
        private void EquipItem(IPlayerEntity player, InventoryEquipEventArgs e)
        {
            var uniqueId = e.UniqueId;
            var part = e.Part;
            bool equip = part == -1;
            Item item = player.Inventory.GetItem(uniqueId);

            if (item == null)
                return;
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("{0} item (Name: {1}, UniqueId: {2}, Part: {3}) for player '{4}' from {5}.", equip ? "Equip" : "Unequip",
                    item.Data.Name, uniqueId, part, player.Object.Name, player.Connection.Socket.RemoteEndPoint);
            }

            if (equip)
            {
                // TODO: check if the player fits the item requirements
                if (item.Data.ItemKind1 == ItemKind1.ARMOR && item.Data.ItemSex != player.VisualAppearance.Gender)
                {
                    Logger.Debug(UnableToEquipMessage, item.Data.Name, player.Object.Name, 
                        player.Connection.Socket.RemoteEndPoint, "wrong sex for this armor");
                    // TODO: Send invalid sex error
                    return;
                }

                if (player.Object.Level < item.Data.LimitLevel)
                {
                    Logger.Debug(UnableToEquipMessage, item.Data.Name, player.Object.Name,
                        player.Connection.Socket.RemoteEndPoint, "player has no the level required");
                    // TODO: Send low level error
                    return;
                }

                // TODO: SPECIAL: double weapon for blades...

                int sourceSlot = item.Slot;
                int destinationSlot = EquipOffset + item.Data.Parts;
                Item currentEquipedItem = player.Inventory[destinationSlot];
                
                // Check if destination slot is already used.
                if (currentEquipedItem != null && currentEquipedItem.Slot != -1)
                    this.UnequipItem(player, currentEquipedItem);

                // Move item
                item.Slot = destinationSlot;
                player.Inventory.Items.Swap(sourceSlot, destinationSlot);
                WorldPacketFactory.SendItemEquip(player, item, item.Data.Parts, true);
            }
            else
            {
                this.UnequipItem(player, item);
            }
        }

        /// <summary>
        /// Unequip an item.
        /// </summary>
        /// <param name="player">Player entity</param>
        /// <param name="item">Item to unequip</param>
        private void UnequipItem(IPlayerEntity player, Item item)
        {
            int sourceSlot = item.Slot;
            int availableSlot = player.Inventory.GetAvailableSlot();

            if (availableSlot < 0)
            {
                Logger.Debug(UnableToUnequipMessage, item.Data.Name, player.Object.Name,
                        player.Connection.Socket.RemoteEndPoint, "no space available in inventory");
                WorldPacketFactory.SendDefinedText(player, DefineText.TID_GAME_LACKSPACE);
                return;
            }

            if (item.Id > 0 && item.Slot > EquipOffset)
            {
                int parts = Math.Abs(sourceSlot - EquipOffset);

                item.Slot = availableSlot;
                player.Inventory.Items.Swap(sourceSlot, availableSlot);
                WorldPacketFactory.SendItemEquip(player, item, parts, false);
            }
        }

        /// <summary>
        /// Create a new item.
        /// </summary>
        /// <param name="player">Player entity</param>
        /// <param name="e"></param>
        private void CreateItem(IPlayerEntity player, InventoryCreateItemEventArgs e)
        {
            ItemData itemData = e.ItemData;

            if (itemData.IsStackable)
            {
                // TODO: stackable items
            }
            else
            {
                for (var i = 0; i < e.Quantity; i++)
                {
                    int availableSlot = player.Inventory.GetAvailableSlot();

                    if (availableSlot < 0)
                    {
                        WorldPacketFactory.SendDefinedText(player, DefineText.TID_GAME_LACKSPACE);
                        break;
                    }

                    Logger.Debug("Available slot: {0}", availableSlot);

                    var newItem = new Item(e.ItemId, 1, e.CreatorId)
                    {
                        Slot = availableSlot,
                        UniqueId = availableSlot,
                    };

                    player.Inventory.Items[availableSlot] = newItem;
                    WorldPacketFactory.SendItemCreation(player, newItem);
                }
            }
        }
    }
}
