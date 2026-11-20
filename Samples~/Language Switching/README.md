# Language Switching

独立演示语言切换、`MultilingualTextListener` 自动刷新、缺少译文时回退，以及 `GetText` 手动查询。

## 启动

1. 在 Package Manager 的 Multilingual Text 页面导入 **Language Switching**，等待示例编译。
2. 先保存当前场景（包括未修改的 Untitled 场景），再执行 `Yvsim > 示例 > Multilingual Text > 创建并打开语言切换示例`。如当前场景有未保存修改，先按 Unity 提示保存或取消操作。
3. 进入 Play Mode，点击 **English / Français / Español**。

菜单使用 Unity API 生成并打开独立场景；无需手动创建 Canvas、拖引用或导入 TMP 基础资源。界面使用 Unity 2021.3 内置 Arial 与 legacy Text，因此选用英、法、西三种语言；演示原理同样适用于其他语言，但中文等文字需配置包含相应字形的字体。

## 观察结果

- **Listener #1** 绑定 ID `1`，每次切换语言自动更新，控制脚本不直接赋值这块文本。
- **Listener #2** 绑定 ID `2`，仅有英语译文；切到法语或西班牙语后仍显示英语，演示回退语言。
- **GetText(1)** 演示查询当前语言；ID `9999` 不存在，因此返回调用方传入的默认文本。
- 状态区显示加载状态、当前语言及回退语言；失败时显示错误。
- 修改生成的 JSON 并保存后，点击 **Reload JSON** 重新加载。空译文与缺少译文不同：已有语言记录的空字符串不会触发回退。

## 文件与隔离

- 场景保存在新建的 `Assets/YvsimSamples/MultilingualTextLanguageSwitching*` 目录。
- 配置保存在新建的 `Assets/StreamingAssets/YvsimSamples/MultilingualTextLanguageSwitching*/language_switching.json`；Manager 的 `File Name` 指向实际生成路径。
- 重复生成时 Unity 自动选择唯一目录，不覆盖同名内容，也不使用或修改用户的 `multilingual_text.json`。
- 示例关闭语言记忆，不写入语言偏好；不修改 Build Settings。运行前以 Single 模式打开本示例场景，避免与其他场景的全局 Manager 同时运行。
- 如需构建，主动把生成的场景加入自己的构建列表；本示例的 StreamingAssets JSON 会随项目正常打包。
- Runtime 与 Editor 各有独立 asmdef；仅使用本包和 Unity uGUI，沿用本包已声明的 Groups 依赖，不依赖其他 Yvsim 业务包。

CLI 调用前也必须将当前未命名场景保存到明确路径，或关闭该场景；公开生成方法遇到未保存命名的场景时会在创建任何示例文件前抛出可操作的异常，不弹出窗口或自动保存用户场景。

CLI 可调用 `Yvsim.MultilingualText.Samples.Editor.MultilingualTextSampleBuilder.CreateSampleScene()` 获得生成的场景资源路径。此入口生成后恢复原有场景集合，不自动打开场景或进入 Play Mode。
