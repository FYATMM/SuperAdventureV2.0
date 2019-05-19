﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Engine
{
    public class Vendor : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public BindingList<InventoryItem> Inventory { get; private set; }

        public Vendor(string name)
        {
            Name = name;
            Inventory = new BindingList<InventoryItem>();
        }

        public void AddItemToInventory(Item itemToAdd, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);
            if (item == null)// They didn't have the item, so add it to their inventory
            {                
                Inventory.Add(new InventoryItem(itemToAdd, quantity));
            }
            else// They have the item in their inventory, so increase the quantity
            {                
                item.Quantity += quantity;
            }
            OnPropertyChanged("Inventory");
        }

        public void RemoveItemFromInventory(Item itemToRemove, int quantity = 1)
        {
            InventoryItem item = Inventory.SingleOrDefault(
                ii => ii.Details.ID == itemToRemove.ID);

            if (item == null)// The item is not in the player's inventory, so ignore it.
            {                
                // We might want to raise an error for this situation
            }
            else// They have the item in their inventory, so decrease the quantity
            {                
                item.Quantity -= quantity;                          
                if (item.Quantity < 0) // Don't allow negative quantities.We might want to raise an error for this situation     
                {
                    item.Quantity = 0;
                }               
                if (item.Quantity == 0) // If the quantity is zero, remove the item from the list
                {
                    Inventory.Remove(item);
                }                
                OnPropertyChanged("Inventory");// Notify the UI that the inventory has changed
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}