using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/UI/Game UI Theme", fileName = "GameUiTheme")]
    public sealed class GameUiTheme : ScriptableObject
    {
        private const string ResourcePath = "UI/GameUiTheme";

        [SerializeField] private Font menuFont;

        private static Font cachedFont;

        public static Font LoadFont()
        {
            if (cachedFont != null)
            {
                return cachedFont;
            }

            GameUiTheme theme = Resources.Load<GameUiTheme>(ResourcePath);
            if (theme != null && theme.menuFont != null)
            {
                cachedFont = theme.menuFont;
                return cachedFont;
            }

            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return cachedFont;
        }
    }
}
