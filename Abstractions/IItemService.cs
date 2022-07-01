using System;
using System.Collections.Generic;
using System.Text;
using ChatCommands.Models;
using Unity.Entities;

namespace ChatCommands.Abstractions
{
    public interface IItemService
    {
        IReadOnlyList<SimpleItem> FindItem(string needle);
        SimpleItem GetItem(string needle);

        unsafe bool AddItemToInventory(SimpleItem item, EntityManager entityManager, Entity target,
            int amount = 1);
    }
}
