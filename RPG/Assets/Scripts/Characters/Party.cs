using System.Collections.Generic;
using UnityEngine;

namespace RPG_Character
{
    public class Party : MonoBehaviour
    {
        public List<Actor> Members = new List<Actor>();

        private void Awake()
        {
            ServiceManager.Register(this);
        }

        void OnDestroy()
        {
            ServiceManager.Unregister(this);
        }

        public void Reset()
        {
            Members.Clear();
        }

        public int Count()
        {
            return Members.Count;
        }

        public bool HasMemeber(string id)
        {
            return IndexOf(id) != -1;
        }

        public Actor GetActor(int index)
        {
            if (index < 0 || index > Members.Count)
            {
                LogManager.LogError($"Index [{index}] is outside the range of Party Memebers.");
                return null;
            }
            return Members[index];
        }

        public Actor GetActor(string id)
        {
            var index = IndexOf(id);
            if (index == -1)
            {
                LogManager.LogError($"Actor {id} is not in the Party.");
                return null;
            }
            return Members[index];
        }

        public void Add(Actor member)
        {
            if (IndexOf(member.GameDataId) != -1)
                return;
            Members.Add(member);
        }

        public void RemoveById(string id)
        {
            var index = IndexOf(id);
            if (index == -1)
                return;
            Members.RemoveAt(index);
        }

        public int EquipCount(Item item)
        {
            int count = 0;
            foreach (var member in Members)
                count += member.EquipCount(item.ItemInfo);
            return count;
        }

        public void Rest()
        {
            foreach (var member in Members)
                if (member.Stats.Get(Stat.HP) > 0)
                    member.Stats.ResetHpMp();
        }

        public void RestAll()
        {
            foreach (var member in Members)
                member.Stats.ResetHpMp();
        }

        public void DebugPrintParty()
        {
            foreach (var member in Members)
            {
                var actor = member;
                var name = actor.name;
                var stats = actor.Stats;
                var hp = stats.Get(Stat.HP);
                var maxHp = stats.Get(Stat.MaxHP);
                LogManager.LogDebug($"{name}: , hp: {hp}/{maxHp}");
            }
        }

        public void PrepareForTextboxState()
        {
            foreach (var character in Members)
                character.GetComponent<Character>().PrepareForTextState();
        }

        public void ReturnFromTextboxState()
        {
            foreach (var character in Members)
                character.GetComponent<Character>().ReturnFromTextState();
        }

        public void PrepareForCombat()
        {
            foreach (var character in Members)
                character.GetComponent<Character>().PrepareForCombat();
        }

        public void ReturnFromCombat()
        {
            foreach (var character in Members)
                character.GetComponent<Character>().ReturnFromCombat();
            Members[0].gameObject.SafeSetActive(true);
        }

        public void PrepareForSceneChange()
        {
            foreach (var character in Members)
                character.GetComponent<Character>().PrepareForSceneChange();
        }

        public void GiveExp(int exp)
        {
            foreach (var member in Members)
            {
                var actor = member;
                actor.AddExp(exp);
                while (actor.ReadyToLevelUp())
                {
                    var levelup = actor.CreateLevelUp(true);
                    actor.ApplyLevel(levelup);

                }
            }
        }

        public List<RPG_GameState.CharacterInfo> ToCharacterInfoList()
        {
            var info = new List<RPG_GameState.CharacterInfo>();
            foreach (var member in Members)
            {
                var actor = member;
                var character = new RPG_GameState.CharacterInfo(actor.Exp, actor.name, actor.Stats.ToStatsData(), actor.Equipment);
                info.Add(character);
            }
            return info;
        }

        public int IndexOf(string id)
        {
            for(int i = 0; i <Members.Count; i++)
                if (Members[i].GameDataId.Equals(id))
                    return i;
            return -1;
        }

        public int GetEquippedCount(string itemId)
        {
            int count = 0;
            foreach (var actor in Members)
                count += actor.EquipCount(itemId);
            return count;
        }
    }
}