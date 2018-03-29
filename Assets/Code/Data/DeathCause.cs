using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class DeathCause : ScriptableObject
    {
        public string friendlyName = "Enemy";
        public Sprite icon;
        public Color color = Color.white;
        public string[] tips;

        public string GetTip
        {
            get
            {
                if (tips.Length == 0) return "Better luck next time.";
                return tips[Random.Range(0, tips.Length)];
            }
        }
    }
}