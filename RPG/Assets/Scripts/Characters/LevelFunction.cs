using UnityEngine;

namespace RPG_Character
{
    [CreateAssetMenu(fileName = "LevelFunction", menuName = "RPG/Level")]
    public class LevelFunction : ScriptableObject
    {
        [SerializeField] float Exponent;
        [SerializeField] float BaseExp;

        public int NextLevel(int level)
        {
            return Mathf.FloorToInt(BaseExp * Mathf.Pow(level, Exponent));
        }
    }
}
