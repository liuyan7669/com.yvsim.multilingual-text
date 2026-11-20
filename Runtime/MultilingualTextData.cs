using System;
using System.Collections.Generic;

namespace Yvsim.MultilingualText
{
    /// <summary>
    /// 系统支持的语言类型。枚举值会直接保存到 JSON 中，不应随意调整。
    /// </summary>
    public enum SupportedLanguage
    {
        ChineseSimplified = 0,
        English = 1,
        Japanese = 2,
        Korean = 3,
        French = 4,
        Spanish = 5,
        Russian = 6,
        German = 7,
        Italian = 8,
        Portuguese = 9
    }

    /// <summary>
    /// 多语言文本 JSON 根节点。
    /// </summary>
    [Serializable]
    public class MultilingualTextTable
    {
        [UnityEngine.Tooltip("创建新文本记录时使用的下一个内容 ID，应大于现有记录 ID。")]
        public int nextId = 1;
        [UnityEngine.Tooltip("多语言文本记录列表；每条记录使用唯一 ID 并可配置多种语言译文。")]
        public List<MultilingualTextItem> items = new List<MultilingualTextItem>();
    }

    /// <summary>
    /// 一条可通过 ID 查询的多语言文本。
    /// </summary>
    [Serializable]
    public class MultilingualTextItem
    {
        [UnityEngine.Tooltip("文本记录的唯一正整数 ID，供 MultilingualTextListener 绑定和代码查询。")]
        public int id;
        [UnityEngine.Tooltip("此文本记录的各语言译文列表，同一语言应只配置一项。")]
        public List<MultilingualTextTranslation> translations = new List<MultilingualTextTranslation>();
    }

    /// <summary>
    /// 指定语言下的文本内容和提示音频。
    /// </summary>
    [Serializable]
    public class MultilingualTextTranslation
    {
        [UnityEngine.Tooltip("此译文对应的语言，切换到该语言时使用本条文本与音频路径。")]
        public SupportedLanguage language = SupportedLanguage.ChineseSimplified;
        [UnityEngine.Tooltip("当前语言的完整显示文本，可包含换行。")]
        public string content;
        [UnityEngine.Tooltip("当前语言提示音频相对于 Resources 目录的路径，不包含文件扩展名；留空表示未配置音频。")]
        public string audioPath;

        /// <summary>
        /// 获取当前语言的完整文本内容。
        /// </summary>
        public string GetText()
        {
            return content ?? string.Empty;
        }

        /// <summary>
        /// 获取提示音频的 Resources 相对路径。
        /// </summary>
        public string GetAudioPath()
        {
            return string.IsNullOrEmpty(audioPath) ? string.Empty : audioPath;
        }
    }
}
