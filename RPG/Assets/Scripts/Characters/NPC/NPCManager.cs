using System.Collections.Generic;
using UnityEngine;

namespace RPG_Character
{
    public class NPCManager : MonoBehaviour
    {
        private Dictionary<string, List<Character>> npcsByMap = new Dictionary<string, List<Character>>();

        private void Awake()
        {
            ServiceManager.Register(this);
        }

        void OnDestroy()
        {
            ServiceManager.Unregister(this);
        }

        public void UpdateAllNPCs(float deltaTime)
        {
            foreach (var map in npcsByMap)
                foreach (var npc in map.Value)
                    npc.Controller.Update(deltaTime);
        }

        public void UpdateAllNPCsforMap(string map, float deltaTime)
        {
            if (!npcsByMap.ContainsKey(map))
                return;
            foreach (var npc in npcsByMap[map])
                    npc.Controller.Update(deltaTime);
        }

        public bool HasNPC(string map, Character character)
        {
            return HasNPC(map, character.GetHashCode());
        }

        public bool HasNPC(string map, int hash)
        {
            if (!npcsByMap.ContainsKey(map))
                return false;
            foreach (var npc in npcsByMap[map])
                if (npc.GetHashCode() == hash)
                    return true;
            return false;
        }

        public void AddNPC(string map, Character character)
        {
            if (!npcsByMap.ContainsKey(map))
                npcsByMap.Add(map, new List<Character>());
            if (HasNPC(map, character.GetHashCode()))
            {
                LogManager.LogError($"Character {character.name} is already present in NPCsByMap for Map: {map}. Cannot add.");
                return;
            }
            npcsByMap[map].Add(character);
        }

        public Character RemoveNPC(string map, Character character)
        {
            return RemoveNPC(map, character.GetHashCode(), character.name);
        }

        public Character RemoveNPC(string map, int hash, string npcName)
        {
            if (!HasNPC(map, hash))
            {
                LogManager.LogError($"Map {map} is not present in NPCsByMap. Cannot remove NPC: {npcName}.");
                return null;
            }
            var index = FindNpcIndex(map, name);
            var character = npcsByMap[map][index];
            npcsByMap[map].RemoveAt(index);
            return character;
        }


        public Character GetNPC(string map, Character character)
        {
            return GetNPC(map, character.GetHashCode());
        }


        public Character GetNPC(string map, int hash)
        {
            if (!HasNPC(map, hash))
            {
                LogManager.LogError($"Character {name} is not present in NPCsForMap for Map: {map}. Cannot get NPC.");
                return null;
            }
            return npcsByMap[map][FindNpcIndex(map, name)];
        }

        public List<Character> ClearNpcsForMap(string map)
        {
            if (!npcsByMap.ContainsKey(map))
            {
                LogManager.LogError($"Map: {map} is not present in NPCsForMap.");
                return new List<Character>();
            }
            var list = npcsByMap[map];
            npcsByMap.Remove(map);
            return list;
        }

        public void PrepareForTextboxState()
        {
            foreach (var map in npcsByMap)
                foreach (var character in map.Value)
                    character.GetComponent<Character>().PrepareForTextState();
        }

        public void ReturnFromTextboxState()
        {
            foreach (var map in npcsByMap)
                foreach (var character in map.Value)
                    character.GetComponent<Character>().ReturnFromTextState();
        }

        public void PrepareForCombat()
        {
            foreach (var map in npcsByMap)
                foreach (var character in map.Value)
                    character.GetComponent<Character>().PrepareForCombat();
        }

        public void ReturnFromCombat()
        {
            foreach (var map in npcsByMap)
                foreach (var character in map.Value)
                    character.GetComponent<Character>().ReturnFromCombat();
        }

        private int FindNpcIndex(string map, string name)
        {
            var npcs = npcsByMap[map];
            for (int i = 0; i < npcs.Count; i++)
                if (npcs[i].name.Equals(name))
                    return i;
            return -1;
        }
    }
}