using System.Collections.Generic;
using UnityEngine;
using RPG_Character;

namespace RPG_Combat
{
    public class CombatPositions : MonoBehaviour
    {
        private float widthMultiplier1;
        private float widthMultiplier2;
        private float partySingle = 0.8f;
        private float partyMultiOne = 0.70f;
        private float partyMultiTwo = 0.80f;
        private float enemySingle = 0.13f;
        private float enemyMultiOne = 0.17f;
        private float enemyMultiTwo = 0.10f;

        public void PlaceParty(List<Actor> actors)
        {
            widthMultiplier1 = partyMultiOne;
            widthMultiplier2 = partyMultiTwo;
            var size = actors.Count;
            if (size == 1)
                widthMultiplier1 = partySingle;
            PlaceActors(actors);
        }

        public void PlaceEnemies(List<Actor> actors)
        {
            widthMultiplier1 = enemyMultiOne;
            widthMultiplier2 = enemyMultiTwo;
            var size = actors.Count;
            if (size == 1)
                widthMultiplier1 = enemySingle;
            PlaceActors(actors);
        }

        private void PlaceActors(List<Actor> actors)
        {
            var positions = GetPositions(actors.Count);
            for (int i = 0; i < positions.Length; i++)
            {
                var position = positions[i];
                var actor = actors[i];
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
            int x = (int)(Screen.width * widthMultiplier1);
            int y = (int)(Screen.height * 0.5f);
            var position = CreateVector(x,y);
            return new Vector2[] { position };
        }

        private Vector2[] GetTwoActorLocations()
        {
            int x = (int)(Screen.width * widthMultiplier1);
            int y1 = (int)(Screen.height * 0.33f);
            int y2 = (int)(Screen.height * 0.66f);
            var position1 = CreateVector(x, y1);
            var position2 = CreateVector(x, y2);
            return new Vector2[] { position1, position2 };
        }

        private Vector2[] GetThreeActorLocations()
        {
            int x = (int)(Screen.width * widthMultiplier1);
            int y1 = (int)(Screen.height * 0.25f);
            int y2 = (int)(Screen.height * 0.50f);
            int y3 = (int)(Screen.height * 0.75f);
            var position1 = CreateVector(x, y1);
            var position2 = CreateVector(x, y2);
            var position3 = CreateVector(x, y3);
            return new Vector2[] { position1, position2, position3 };
        }

        private Vector2[] GetFourActorLocations()
        {
            int x1 = (int)(Screen.width * widthMultiplier1);
            int x2 = (int)(Screen.width * widthMultiplier2);
            int y1 = (int)(Screen.height * 0.20f);
            int y2 = (int)(Screen.height * 0.60f);
            int y3 = (int)(Screen.height * 0.40f);
            int y4 = (int)(Screen.height * 0.80f);
            var position1 = CreateVector(x1, y1);
            var position2 = CreateVector(x1, y2);
            var position3 = CreateVector(x2, y3);
            var position4 = CreateVector(x2, y4);
            return new Vector2[] { position1, position2, position3, position4 };
        }

        private Vector2[] GetFiveActorLocations()
        {
            int x1 = (int)(Screen.width * widthMultiplier1);
            int x2 = (int)(Screen.width * widthMultiplier2);
            int y1 = (int)(Screen.height * 0.20f);
            int y2 = (int)(Screen.height * 0.33f);
            int y3 = (int)(Screen.height * 0.40f);
            int y4 = (int)(Screen.height * 0.66f);
            int y5 = (int)(Screen.height * 0.80f);
            var position1 = CreateVector(x1, y1);
            var position2 = CreateVector(x2, y2);
            var position3 = CreateVector(x1, y3);
            var position4 = CreateVector(x2, y4);
            var position5 = CreateVector(x1, y5);
            return new Vector2[] { position1, position2, position3, position4, position5 };
        }

        private Vector2[] GetSixActorLocations()
        {
            int x1 = (int)(Screen.width * widthMultiplier1);
            int x2 = (int)(Screen.width * widthMultiplier2);
            int y1 = (int)(Screen.height * 0.20f);
            int y2 = (int)(Screen.height * 0.30f);
            int y3 = (int)(Screen.height * 0.40f);
            int y4 = (int)(Screen.height * 0.50f);
            int y5 = (int)(Screen.height * 0.60f);
            int y6 = (int)(Screen.height * 0.70f);
            var position1 = CreateVector(x1, y1);
            var position2 = CreateVector(x2, y2);
            var position3 = CreateVector(x1, y3);
            var position4 = CreateVector(x2, y4);
            var position5 = CreateVector(x1, y5);
            var position6 = CreateVector(x2, y6);
            return new Vector2[] { position1, position2, position3, position4, position5, position6 };
        }

        private Vector2 CreateVector(int x, int y)
        {
            var position = new Vector2(x, y);
            return Camera.main.ScreenToWorldPoint(position);
        }
    }
}
