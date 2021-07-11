using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public class CombatPositions : MonoBehaviour
    {
        private float heightMultiplier1;
        private float heightMultiplier2;
        private float partySingle = 0.3f;
        private float partyMultiOne = 0.30f;
        private float partyMultiTwo = 0.30f;
        private float enemySingle = 0.7f;
        private float enemyMultiOne = 0.65f;
        private float enemyMultiTwo = 0.75f;

        private Transform combatLocation;

        private void Awake()
        {
            combatLocation = GameObject.Find("Combat").transform;
        }

        public void PlaceParty(List<Actor> actors)
        {
            heightMultiplier1 = partyMultiOne;
            heightMultiplier2 = partyMultiTwo;
            var size = actors.Count;
            if (size == 1)
                heightMultiplier1 = partySingle;
            PlaceActors(actors);
        }

        public void PlaceEnemies(List<Actor> actors)
        {
            heightMultiplier1 = enemyMultiOne;
            heightMultiplier2 = enemyMultiTwo;
            var size = actors.Count;
            if (size == 1)
                heightMultiplier1 = enemySingle;
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
            int x = (int)(Screen.width * 0.5f);
            int y = (int)(Screen.height * heightMultiplier1);
            var position = CreateVector(x,y);
            return new Vector2[] { position };
        }

        private Vector2[] GetTwoActorLocations()
        {
            int x1 = (int)(Screen.width * 0.33f);
            int x2 = (int)(Screen.width * 0.66f);
            int y = (int)(Screen.height * heightMultiplier1);
            var position1 = CreateVector(x1, y);
            var position2 = CreateVector(x2, y);
            return new Vector2[] { position1, position2 };
        }

        private Vector2[] GetThreeActorLocations()
        {
            var combatPosition = combatLocation.position;
            int x1 = (int)(Screen.width * 0.25f + combatPosition.x);
            int x2 = (int)(Screen.width * 0.50f + combatPosition.x);
            int x3 = (int)(Screen.width * 0.75f + combatPosition.x);
            int y = (int)(Screen.height * heightMultiplier1);
            var position1 = CreateVector(x1, y);
            var position2 = CreateVector(x2, y);
            var position3 = CreateVector(x3, y);
            return new Vector2[] { position1, position2, position3 };
        }

        private Vector2[] GetFourActorLocations()
        {
            int x1 = (int)(Screen.width * 0.20f);
            int x2 = (int)(Screen.width * 0.60f);
            int x3 = (int)(Screen.width * 0.40f);
            int x4 = (int)(Screen.width * 0.80f);
            int y1 = (int)(Screen.height * heightMultiplier1);
            int y2 = (int)(Screen.height * heightMultiplier2);
            var position1 = CreateVector(x1, y1);
            var position2 = CreateVector(x2, y1);
            var position3 = CreateVector(x3, y2);
            var position4 = CreateVector(x4, y2);
            return new Vector2[] { position1, position2, position3, position4 };
        }

        private Vector2[] GetFiveActorLocations()
        {
            int x1 = (int)(Screen.width * 0.20f);
            int x2 = (int)(Screen.width * 0.33f);
            int x3 = (int)(Screen.width * 0.40f);
            int x4 = (int)(Screen.width * 0.66f);
            int x5 = (int)(Screen.width * 0.80f);
            int y1 = (int)(Screen.height * heightMultiplier1);
            int y2 = (int)(Screen.height * heightMultiplier2);
            var position1 = CreateVector(x1, y1);
            var position2 = CreateVector(x2, y2);
            var position3 = CreateVector(x3, y1);
            var position4 = CreateVector(x4, y2);
            var position5 = CreateVector(x5, y1);
            return new Vector2[] { position1, position2, position3, position4, position5 };
        }

        private Vector2[] GetSixActorLocations()
        {
            int x1 = (int)(Screen.width * 0.20f);
            int x2 = (int)(Screen.width * 0.30f);
            int x3 = (int)(Screen.width * 0.40f);
            int x4 = (int)(Screen.width * 0.50f);
            int x5 = (int)(Screen.width * 0.60f);
            int x6 = (int)(Screen.width * 0.70f);
            int y1 = (int)(Screen.height * heightMultiplier1);
            int y2 = (int)(Screen.height * heightMultiplier2);
            var position1 = CreateVector(x1, y1);
            var position2 = CreateVector(x2, y2);
            var position3 = CreateVector(x3, y1);
            var position4 = CreateVector(x4, y2);
            var position5 = CreateVector(x6, y1);
            var position6 = CreateVector(x2, y2);
            return new Vector2[] { position1, position2, position3, position4, position5, position6 };
        }

        private Vector2 CreateVector(int x, int y)
        {
            var position = new Vector2(x, y);
            return Camera.main.ScreenToWorldPoint(position);
        }
    }
}
