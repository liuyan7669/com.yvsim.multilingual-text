using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Yvsim.MultilingualText.Samples.Editor
{
    /// <summary>生成独立的配置和演示场景，不覆盖已有配置或修改 Build Settings。</summary>
    public static class MultilingualTextSampleBuilder
    {
        private const string MenuPath = "Yvsim/示例/Multilingual Text/创建并打开语言切换示例";

        [MenuItem(MenuPath)]
        private static void CreateAndOpen()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            // A clean, untitled scene is skipped by SaveCurrentModifiedScenesIfUserWantsTo,
            // but Unity cannot create an additive scene until that scene has a saved path.
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && string.IsNullOrEmpty(scene.path) && !EditorSceneManager.SaveScene(scene))
                    return;
            }
            string scenePath = CreateSampleScene();
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log("多语言示例已打开。进入 Play Mode 后点击语言按钮；配置路径见场景中 Manager 的 File Name。场景：" + scenePath);
        }

        [MenuItem(MenuPath, true)]
        private static bool CanCreate() => !EditorApplication.isPlayingOrWillChangePlaymode;

        /// <summary>生成并保存场景，返回资源路径；保留调用前打开的场景，适合通过 CLI 验证。</summary>
        public static string CreateSampleScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("请退出 Play Mode 后再生成示例。");
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && string.IsNullOrEmpty(scene.path))
                    throw new InvalidOperationException("当前存在尚未保存命名的场景。请先通过 File > Save 保存，或关闭该场景，再生成示例；CLI 调用方应先准备已保存的场景。尚未创建任何示例文件。");
            }

            EnsureFolder("Assets/YvsimSamples");
            string outputFolder = AssetDatabase.GenerateUniqueAssetPath("Assets/YvsimSamples/MultilingualTextLanguageSwitching");
            EnsureFolder(outputFolder);
            EnsureFolder("Assets/StreamingAssets/YvsimSamples");
            string configFolder = AssetDatabase.GenerateUniqueAssetPath("Assets/StreamingAssets/YvsimSamples/MultilingualTextLanguageSwitching");
            EnsureFolder(configFolder);
            string configPath = configFolder + "/language_switching.json";
            using (var writer = new StreamWriter(File.Open(Path.Combine(Application.dataPath, configPath.Substring("Assets/".Length)), FileMode.CreateNew, FileAccess.Write), new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(CreateTable(), true));
            AssetDatabase.ImportAsset(configPath);

            Scene previousScene = SceneManager.GetActiveScene();
            Scene sampleScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            string scenePath = outputFolder + "/LanguageSwitching.unity";
            try
            {
                SceneManager.SetActiveScene(sampleScene);
                var manager = new GameObject("Multilingual Text Sample Manager").AddComponent<MultilingualTextManager>();
                var managerData = new SerializedObject(manager);
                managerData.FindProperty("fileName").stringValue = configPath.Substring("Assets/StreamingAssets/".Length);
                managerData.FindProperty("currentLanguage").enumValueIndex = (int)SupportedLanguage.English;
                managerData.FindProperty("fallbackLanguage").enumValueIndex = (int)SupportedLanguage.English;
                managerData.FindProperty("rememberLanguage").boolValue = false;
                managerData.ApplyModifiedPropertiesWithoutUndo();

                var camera = new GameObject("Sample Camera").AddComponent<Camera>();
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.08f, 0.1f, 0.15f);
                camera.cullingMask = 0;

                var canvasObject = new GameObject("Language Switching Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(960f, 640f);
                scaler.matchWidthOrHeight = 0.5f;
                Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                Transform panel = canvasObject.transform;

                CreateText(panel, font, "Title", 24f, 42f, "Multilingual Text - Language Switching", 30);
                CreateText(panel, font, "Instructions", 78f, 58f, "Switch languages below. Listener #1 updates automatically.\nListener #2 has only an English translation, so French/Spanish use the English fallback.", 20);
                CreateText(panel, font, "Binding Heading", 150f, 28f, "Listener #1 - three translations", 18);
                Text translated = CreateText(panel, font, "Bound Greeting", 183f, 50f, "Loading...", 28);
                translated.gameObject.AddComponent<MultilingualTextListener>().TextId = 1;
                CreateText(panel, font, "Fallback Heading", 250f, 28f, "Listener #2 - English fallback", 18);
                Text fallback = CreateText(panel, font, "Bound Fallback", 283f, 52f, "Loading...", 25);
                fallback.gameObject.AddComponent<MultilingualTextListener>().TextId = 2;
                Text query = CreateText(panel, font, "Manual Query", 355f, 75f, "Waiting for JSON...", 20);
                Text status = CreateText(panel, font, "Load Status", 446f, 66f, "Not loaded", 18);

                var sample = canvasObject.AddComponent<MultilingualTextSample>();
                var sampleData = new SerializedObject(sample);
                sampleData.FindProperty("statusText").objectReferenceValue = status;
                sampleData.FindProperty("queryText").objectReferenceValue = query;
                sampleData.ApplyModifiedPropertiesWithoutUndo();
                CreateButton(panel, font, 0, "English", sample.UseEnglish);
                CreateButton(panel, font, 1, "Français", sample.UseFrench);
                CreateButton(panel, font, 2, "Español", sample.UseSpanish);
                CreateButton(panel, font, 3, "Reload JSON", sample.ReloadConfiguration);
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

                if (!EditorSceneManager.SaveScene(sampleScene, scenePath))
                    throw new IOException("无法保存示例场景：" + scenePath);
                return scenePath;
            }
            finally
            {
                EditorSceneManager.CloseScene(sampleScene, true);
                if (previousScene.IsValid() && previousScene.isLoaded) SceneManager.SetActiveScene(previousScene);
            }
        }

        private static MultilingualTextTable CreateTable()
        {
            return new MultilingualTextTable
            {
                nextId = 3,
                items = new List<MultilingualTextItem>
                {
                    new MultilingualTextItem
                    {
                        id = 1,
                        translations = new List<MultilingualTextTranslation>
                        {
                            new MultilingualTextTranslation { language = SupportedLanguage.English, content = "Hello! The listener updates this text automatically." },
                            new MultilingualTextTranslation { language = SupportedLanguage.French, content = "Bonjour ! Le texte est mis à jour automatiquement." },
                            new MultilingualTextTranslation { language = SupportedLanguage.Spanish, content = "¡Hola! El texto se actualiza automáticamente." }
                        }
                    },
                    new MultilingualTextItem
                    {
                        id = 2,
                        translations = new List<MultilingualTextTranslation>
                        {
                            new MultilingualTextTranslation { language = SupportedLanguage.English, content = "English fallback: this entry has no French or Spanish translation." }
                        }
                    }
                }
            };
        }

        private static Text CreateText(Transform parent, Font font, string name, float top, float height, string value, int size)
        {
            var gameObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = new Vector2(32f, -top - height);
            rect.offsetMax = new Vector2(-32f, -top);
            Text text = gameObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.text = value;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static void CreateButton(Transform parent, Font font, int index, string label, UnityAction action)
        {
            var gameObject = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            gameObject.transform.SetParent(parent, false);
            var rect = gameObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(32f + index * 226f, -544f);
            rect.sizeDelta = new Vector2(210f, 48f);
            Image background = gameObject.GetComponent<Image>();
            background.color = new Color(0.2f, 0.38f, 0.62f);
            Button button = gameObject.GetComponent<Button>();
            button.targetGraphic = background;
            UnityEventTools.AddPersistentListener(button.onClick, action);
            Text text = CreateText(gameObject.transform, font, "Label", 0f, 48f, label, 22);
            text.rectTransform.offsetMin = new Vector2(4f, -48f);
            text.rectTransform.offsetMax = new Vector2(-4f, 0f);
            text.alignment = TextAnchor.MiddleCenter;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }
    }
}
