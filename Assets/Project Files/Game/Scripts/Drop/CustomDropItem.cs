﻿using UnityEngine;
using Watermelon;

namespace Watermelon
{
    [System.Serializable]
    public class CustomDropItem : IDropItem
    {
        [SerializeField] DropableItemType dropableItemType;
        public DropableItemType DropItemType => dropableItemType;

        [SerializeField] GameObject prefab;
        public GameObject DropPrefab => prefab;

        private Pool pool;

        public CustomDropItem(DropableItemType dropableItemType, GameObject prefab)
        {
            this.dropableItemType = dropableItemType;
            this.prefab = prefab;
        }

        public void Initialise()
        {
            pool = new Pool(prefab);
        }

        public GameObject GetDropObject(DropData dropData)
        {
            return pool.GetPooledObject();
        }

        public void Unload()
        {
            pool?.Destroy();
        }
    }
}