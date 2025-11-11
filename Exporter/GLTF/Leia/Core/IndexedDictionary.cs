using System;
using System.Collections.Generic;

namespace ASRR.Revit.Core.Exporter.GLTF.Leia.Core
{
    /// <summary>
    ///     Container for holding a strict set of items
    ///     that is also addressable by a unique ID.
    /// </summary>
    /// <typeparam name="T">The type of item contained.</typeparam>
    public class IndexedDictionary<T>
    {
        private readonly Dictionary<string, int> dict = new Dictionary<string, int>();

        /// <summary>
        ///     Temp output used in 'this.Dict'
        /// </summary>
        private readonly Dictionary<string, T> output = new Dictionary<string, T>();

        /// <summary>
        ///     Gets all the generic elements inside the IndexedDictionary.
        /// </summary>
        public List<T> List { get; } = new List<T>();

        /// <summary>
        ///     Gets the current key from actual element.
        /// </summary>
        public string CurrentKey { get; private set; }

        /// <summary>
        ///     Gets the dictionary.
        /// </summary>
        public Dictionary<string, T> Dict
        {
            get
            {
                output.Clear();
                foreach (var kvp in dict) output.Add(kvp.Key, List[kvp.Value]);

                return output;
            }
        }

        /// <summary>
        ///     Gets the most recently accessed item (not effected by GetElement()).
        /// </summary>
        public T CurrentItem => List[dict[CurrentKey]];

        /// <summary>
        ///     Gets the index of the most recently accessed item (not effected by GetElement()).
        /// </summary>
        public int CurrentIndex => dict[CurrentKey];

        /// <summary>
        ///     Add a new item to the list, if it already exists then the current item will be set to this item.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <param name="elem">The item to add.</param>
        /// <returns>true if item did not already exist.</returns>
        public bool AddOrUpdateCurrent(string uuid, T elem)
        {
            if (!dict.ContainsKey(uuid))
            {
                List.Add(elem);
                dict.Add(uuid, List.Count - 1);
                CurrentKey = uuid;
                return true;
            }

            CurrentKey = uuid;

            return false;
        }

        /// <summary>
        ///     Add a new gltfMaterial to the list, if it already exists then the current item will be set to this item.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <param name="elem">The item to add.</param>
        /// <param name="doubleSided">Identify if the material is double sided.</param>
        /// <returns>true if item did not already exist.</returns>
        public bool AddOrUpdateCurrentMaterial(string uuid, T elem, bool doubleSided)
        {
            if (!dict.ContainsKey(uuid))
            {
                List.Add(elem);
                dict.Add(uuid, List.Count - 1);
                CurrentKey = uuid;
                return true;
            }

            CurrentKey = uuid;

            if (elem is GLTFMaterial)
            {
                var mat = GetElement(uuid) as GLTFMaterial;
                mat.doubleSided = doubleSided;
            }

            return false;
        }

        /// <summary>
        ///     Check if the container already has an item with this key.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <returns>Returns TRUE if the dictionary contains the given element, otherwise, returns FALSE.</returns>
        public bool Contains(string uuid)
        {
            return dict.ContainsKey(uuid);
        }

        /// <summary>
        ///     Returns the index for an item given it's unique identifier.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item.</param>
        /// <returns>index of item or -1. </returns>
        public int GetIndexFromUUID(string uuid)
        {
            try
            {
                return dict[uuid];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception("Specified item could not be found.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting the specified item {ex.Message}");
            }
        }

        /// <summary>
        ///     Returns an item given it's unique identifier.
        /// </summary>
        /// <param name="uuid">Unique identifier for the item. </param>
        /// <returns>Element.</returns>
        public T GetElement(string uuid)
        {
            var index = GetIndexFromUUID(uuid);
            return List[index];
        }

        public void Reset()
        {
            dict.Clear();
            List.Clear();
            Dict.Clear();
            CurrentKey = string.Empty;
        }
    }
}