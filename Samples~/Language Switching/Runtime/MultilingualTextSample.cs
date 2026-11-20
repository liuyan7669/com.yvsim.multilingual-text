using UnityEngine;
using UnityEngine.UI;

namespace Yvsim.MultilingualText.Samples
{
    /// <summary>
    /// 配合示例场景展示语言切换、加载结果与手动查询；绑定文本由 Listener 自动更新。
    /// </summary>
    public sealed class MultilingualTextSample : MonoBehaviour
    {
        [SerializeField, Tooltip("显示配置加载状态、当前语言和回退语言的示例文本组件。")]
        private Text statusText;

        [SerializeField, Tooltip("显示 GetText 查询结果，以及不存在的 ID 所使用的调用方默认文本。")]
        private Text queryText;

        private void OnEnable()
        {
            MultilingualTextManager.LoadCompleted += Refresh;
            MultilingualTextManager.LanguageChanged += Refresh;
            MultilingualTextManager.LoadFailed += OnLoadFailed;
            Refresh();
        }

        private void OnDisable()
        {
            MultilingualTextManager.LoadCompleted -= Refresh;
            MultilingualTextManager.LanguageChanged -= Refresh;
            MultilingualTextManager.LoadFailed -= OnLoadFailed;
        }

        public void UseEnglish() => MultilingualTextManager.CurrentLanguage = SupportedLanguage.English;
        public void UseFrench() => MultilingualTextManager.CurrentLanguage = SupportedLanguage.French;
        public void UseSpanish() => MultilingualTextManager.CurrentLanguage = SupportedLanguage.Spanish;

        public void ReloadConfiguration()
        {
            MultilingualTextManager.Reload();
            Refresh();
        }

        private void OnLoadFailed(string error) => Refresh();

        private void Refresh()
        {
            if (statusText != null)
            {
                statusText.text = "State: " + MultilingualTextManager.LoadState
                    + " | Language: " + MultilingualTextManager.CurrentLanguage
                    + " | Fallback: " + MultilingualTextManager.FallbackLanguage;
                if (!string.IsNullOrEmpty(MultilingualTextManager.LastLoadError))
                    statusText.text += "\n" + MultilingualTextManager.LastLoadError;
            }

            if (queryText != null)
            {
                queryText.text = MultilingualTextManager.IsLoaded
                    ? "GetText(1): " + MultilingualTextManager.GetText(1)
                        + "\nGetText(9999, default): " + MultilingualTextManager.GetText(9999, "This ID does not exist.")
                    : "Waiting for the sample JSON configuration...";
            }
        }
    }
}
