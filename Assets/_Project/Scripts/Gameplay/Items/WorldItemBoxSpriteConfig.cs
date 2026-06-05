using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/Items/World Item Box Sprite Config", fileName = "WorldItemBoxSpriteConfig")]
    public class WorldItemBoxSpriteConfig : ScriptableObject
    {
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;

        public Sprite ClosedSprite => closedSprite;
        public Sprite OpenSprite => openSprite;
    }
}
