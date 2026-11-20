using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/UI/Game UI Theme", fileName = "GameUiTheme")]
    public sealed class GameUiTheme : ScriptableObject
    {
        private const string ResourcePath = "UI/GameUiTheme";

        [SerializeField] private Font menuFont;
        [SerializeField] private Sprite statChoicePanel;

        private static Font cachedFont;

        public static Sprite LoadStatChoicePanel()
        {
            GameUiTheme theme = Resources.Load<GameUiTheme>(ResourcePath);
            return theme != null ? theme.statChoicePanel : null;
        }

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
