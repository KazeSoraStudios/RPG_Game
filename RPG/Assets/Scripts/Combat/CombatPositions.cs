using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public class CombatPositions : MonoBehaviour
    {
        private int heightValue1;
        private int heightMultiplier2;
        private int partySingle = -3;
        private int partyMultiOne = -3;
        private int partyMultiTwo = -3;
        private int enemySingle = 2;
        private int enemyMultiOne = 2;
        private int enemyMultiTwo = 4;

        private Transform combatLocation;

        private void Awake()
        {
            var combat = GameObject.Find("Combat");
            if (combat != null)
                combatLocation = combat.transform;
            else
                combatLocation = transform;
        }

        public void PlaceParty(List<Actor> actors)
        {
            heightValue1 = partyMultiOne;
            heightMultiplier2 = partyMultiTwo;
            var size = actors.Count;
            if (size == 1)
                heightValue1 = partySingle;
            PlaceActors(actors);
        }

        public void PlaceEnemies(List<Actor> actors)
        {
            heightValue1 = enemyMultiOne;
            heightMultiplier2 = enemyMultiTwo;
            var size = actors.Count;
            if (size == 1)
                heightValue1 = enemySingle;
            PlaceActors(actors);
        }

        private void PlaceActors(List<Actor> actors)
        {
            var positions = GetPositions(actors.Count);
            var combatPosition = combatLocation.position;
            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                var actor = actors[i];
                position = new Vector2(position.x + combatPosition.x, position.y);
                actor.transform.position = position;
            }
        }

        private Vector2[] GetPositions(int size)
        {
            switch (size)
            {
                case 1:
                    return GetOneActorLocation();
                case 2:
                    return GetTwoActorLocations();
                case 3:
                    return GetThreeActorLocations();
                case 4:
                    return GetFourActorLocations();
                case 5:
                    return GetFiveActorLocations();
                case 6:
                    return GetSixActorLocations();
                default:
                    LogManager.LogError("Invalid size for battle positions.");
                    return GetOneActorLocation();
            }
        }

        private Vector2[] GetOneActorLocation()
        {
            return new Vector2[] { new Vector2(0, heightValue1) };
        }

        private Vector2[] GetTwoActorLocations()
        {
            int x1 = -2;
            int x2 = 2;
            int y = heightValue1;
            return new Vector2[] { new Vector2(x1, y), new Vector2(x2, y) };
        }

        private Vector2[] GetThreeActorLocations()
        {
            var combatPosition = combatLocation.position;
            int x1 = -2;
            int x2 = 0;
            int x3 = 2;
            int y = heightValue1;
            return new Vector2[] { new Vector2(x1, y), new Vector2(x2, y), new Vector2(x3, y) };
        }

        private Vector2[] GetFourActorLocations()
        {
            int x1 = -2;
            int x2 = -1;
            int x3 = 1;
            int x4 = 2;
            int y1 = heightValue1;
            int y2 = heightMultiplier2;
            return new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y1), new Vector2(x3, y2), new Vector2(x4, y2) };
        }

        private Vector2[] GetFiveActorLocations()
        {
            int x1 = -2;
            int x2 = -1;
            int x3 = 0;
            int x4 = 1;
            int x5 = 2;
            int y1 = heightValue1;
            int y2 = heightMultiplier2;
            return new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2), 
                new Vector2(x3, y1), new Vector2(x4, y2), new Vector2(x5, y1) };
        }

        private Vector2[] GetSixActorLocations()
        {
            int x1 = -3;
            int x2 = -2;
            int x3 = -1;
            int x4 = 1;
            int x5 = 2;
            int x6 = 3;
            int y1 = heightValue1;
            int y2 = heightMultiplier2;
            return new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2), new Vector2(x3, y1), 
                new Vector2(x4, y2), new Vector2(x5, y1), new Vector2(x6, y2) };
        }
    }
}
