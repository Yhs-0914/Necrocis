using UnityEngine;

namespace Necrocis
{
    [CreateAssetMenu(menuName = "Necrocis/UI/Game UI Theme", fileName = "GameUiTheme")]
    public sealed class GameUiTheme : ScriptableObject
    {
        private const string ResourcePath = "UI/GameUiTheme";

        [SerializeField] private Font menuFont;

        public static Font LoadFont()
        {
            GameUiTheme theme = Resources.Load<GameUiTheme>(ResourcePath);
            if (theme != null && theme.menuFont != null)
            {
                return theme.menuFont;
            }

            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
    }
}
