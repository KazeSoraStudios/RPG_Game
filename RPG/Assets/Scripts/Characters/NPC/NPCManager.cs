using System.Collections.Generic;
using UnityEngine;

namespace RPG_Character
{
    public class NPCManager : MonoBehaviour
    {
        private Dictionary<string, Character> npcs = new Dictionary<string, Character>();

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
            foreach (var npc in npcs)
                npc.Value.Controller.Update(deltaTime);
        }

        public bool HasNPC(Character character)
        {
            return npcs.ContainsKey(character.name);
        }

        public bool HasNPC(string name)
        {
            return npcs.ContainsKey(name);
        }

        public void AddNPC(Character character)
        {
            if (npcs.ContainsKey(character.name))
            {
                LogManager.LogError($"Character {character.name} is already present in NPCs. Cannot add.");
                return;
            }
            npcs.Add(character.name, character);
        }

        public Character RemoveNPC(Character character)
        {
            if (!npcs.ContainsKey(character.name))
            {
                LogManager.LogError($"Character {character.name} is not present in NPCs. Cannot remove.");
                return null;
            }
            var c = npcs[character.name];
            npcs.Remove(character.name);
            return c;
        }

        public Character RemoveNPC(string name)
        {
            if (!npcs.ContainsKey(name))
            {
                LogManager.LogError($"Character {name} is not present in NPCs. Cannot remove.");
                return null;
            }
            var character = npcs[name];
            npcs.Remove(name);
            return character;
        }


        public Character GetNPC(Character character)
        {
            if (!npcs.ContainsKey(character.name))
            {
                LogManager.LogError($"Character {character.name} is not present in NPCs. Cannot get NPC.");
                return null;
            }
            return npcs[character.name];
        }


        public Character GetNPC(string name)
        {
            if (!npcs.ContainsKey(name))
            {
                LogManager.LogError($"Character {name} is not present in NPCs. Cannot get NPC.");
                return null;
            }
            return npcs[name];
        }

        public Character[] GetNPCs()
        {
            var npcs = new Character[this.npcs.Count];
            int index = 0;
            foreach (var member in this.npcs)
                npcs[index++] = member.Value;
            return npcs;
        }

        public void PrepareForTextboxState()
        {
            foreach (var character in npcs)
                character.Value.GetComponent<Character>().PrepareForTextState();
        }

        public void ReturnFromTextboxState()
        {
            foreach (var character in npcs)
                character.Value.GetComponent<Character>().ReturnFromTextState();
        }

        public void PrepareForCombat()
        {
            foreach (var character in npcs)
                character.Value.GetComponent<Character>().PrepareForCombat();
        }

        public void ReturnFromCombat()
        {
            foreach (var character in npcs)
                character.Value.GetComponent<Character>().ReturnFromCombat();
        }
    }
}