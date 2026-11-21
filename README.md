# Multilingual Text

适用于 Unity 的多语言文本，提供多语言文本配置，支持外部查询、语言切换、提示音频路径和 `Text`/`TMP_Text` 自动刷新。

## 功能

- 使用一份 JSON 配置多条记录及其各语言文本和提示音频路径。
- 支持当前语言、回退语言、语言记忆和运行时切换。
- 提供 `Text` 与 `TMP_Text` 自动监听刷新组件。
- 提供可视化编辑器，支持搜索替换、数据校验、Undo/Redo 和未保存状态恢复。
- 提供 由 Inspector Groups 绘制的 Manager 与 Listener Inspector，支持配置文件诊断、词条预览和快捷操作。
- 支持通过 Hierarchy 右键菜单定位或创建多语言文本。
- 支持百度翻译 API 配置、单语言翻译、单记录翻译和整表翻译。
- 翻译任务使用 Unity 可取消进度条，可随时停止。
- 支持通过 `UnityWebRequest` 从 `StreamingAssets` 异步加载配置。

## 安装

当前源码版本为 `1.0.0`，最低 Unity 2021.3。

在 **Window > Package Manager > + > Add package from disk...** 中选择本包根目录的 `package.json`。安装本包前，先用相同方式安装本地 `com.yvsim.inspector-groups` `1.0.0`，或将两者一起作为 embedded package 放入项目 `Packages/` 目录。

也可把完整包目录（含 `.meta`）放入项目 `Packages/com.yvsim.multilingual-text`，由 Unity 作为 embedded package 加载。

本包提供本地源码、Git 和 OpenUPM 安装方式；固定版本使用 `1.0.0`，具体步骤见下文。

## 创建示例配置

配置文件默认位于 `Assets/StreamingAssets/multilingual_text.json`：

```json
{
  "nextId": 2,
  "items": [
    {
      "id": 1,
      "translations": [
        {
          "language": 0,
          "content": "第一段提示。\n第二段提示。",
          "audioPath": "Audio/Tips/Chinese/tip_001"
        },
        {
          "language": 1,
          "content": "First tip.\nSecond tip.",
          "audioPath": "Audio/Tips/English/tip_001"
        }
      ]
    }
  ]
}
```

- `id`：大于 0 的全局唯一整数 ID。编辑器新增记录时使用 `nextId` 自动递增，删除记录不会重排或复用已有 ID。
- `nextId`：编辑器下次自动分配的 ID；手动输入更大的 ID 后会自动向后推进。
- `language`：`SupportedLanguage` 的整数值。
- `content`：当前语言的完整文本内容，可直接包含换行符。
- `audioPath`：当前语言的提示音频 `Resources` 相对路径，可为空。

可通过 `Yvsim > 配置 > 多语言 > 创建示例配置` 创建示例文件，已有同名文件时不会覆盖。

## 使用方法

### 编辑多语言配置

通过 `Yvsim > 配置 > 多语言 > 文本编辑器`，或在 `MultilingualTextManager` Inspector 中打开可视化编辑器。编辑器支持：

- 表格视图与详情视图，并记住上次使用的视图。
- 搜索、查找下一个、当前替换和全部替换。
- 新增、复制、删除、Undo/Redo 和未保存状态提示。
- 多语言文本内容和各语言音频路径编辑。
- 空 ID、重复 ID、重复语言和缺少文本的数据校验。
- 选择、重载、保存、定位现有文件，以及创建新的示例文件。
- 从编辑器工具栏打开独立百度翻译配置窗口，并在详情视图翻译单条记录，或从顶部工具栏翻译整表；只生成已经添加的目标语言，并在调用前预估费用。

### 场景初始化

在 Hierarchy 中右键选择 `Yvsim > 框架 > 创建多语言系统`。场景中已存在 `MultilingualTextManager` 时会直接选中并高亮，避免重复创建；不存在时会在场景根节点创建 `[多语言文本]`。

Manager Inspector 会检查 `File Name` 对应的 `StreamingAssets` 配置文件，并可直接打开当前配置、创建默认配置或定位文件。管理器会读取该配置，切换场景时保持存在。

### 手动查询

```csharp
using Yvsim.MultilingualText;
using TMPro;

public TMP_Text targetText;

private void Refresh()
{
    targetText.text = MultilingualTextManager.GetText(1);
    string audioPath = MultilingualTextManager.GetAudioPath(1);
}
```

切换语言：

```csharp
MultilingualTextManager.CurrentLanguage = SupportedLanguage.English;
```

重新加载可调用 `MultilingualTextManager.Reload()`；停止当前加载可调用 `MultilingualTextManager.StopLoading()`。

### 自动刷新文本

将 `MultilingualTextListener` 添加到带有 `Text` 或 `TMP_Text` 的同一个 GameObject，并填写文本 ID。语言切换或配置加载完成后，组件会使用完整 `content` 自动刷新。

Listener Inspector 会按照当前场景 Manager 的 `File Name` 显示词条内容预览，并在缺少文本组件时提供快捷添加入口。

- `Text Id = 0`：未绑定任何多语言内容，新添加组件时的默认状态。
- `Text Id > 0`：绑定 `multilingual_text.json` 中对应的内容 ID。
- `Clear Text When Missing`：配置加载完成但找不到对应 ID 时，决定清空文本还是保留当前内容。

## 可导入示例

在 Package Manager 导入 **Language Switching**，先保存当前场景（包括 Untitled），然后执行 `Yvsim > 示例 > Multilingual Text > 创建并打开语言切换示例`，进入 Play Mode 即可操作。

示例使用 Text 自动绑定、英/法/西语言切换、英语回退和手动查询；场景与 StreamingAssets 配置均写入新的唯一示例目录，不覆盖 `multilingual_text.json`，不保存语言偏好，也不修改 Build Settings。示例使用内置 Arial，无需导入 TMP 资源。具体观察结果、配置位置和 CLI 入口见 [示例说明](Samples~/Language%20Switching/README.md)。

## 主要类型

- `MultilingualTextManager`：加载配置、切换语言并提供文本和音频路径查询。
- `MultilingualTextListener`：监听语言和配置变化，自动刷新同一 GameObject 上的 `Text` 或 `TMP_Text`。
- `MultilingualTextConfig`：读取、重新读取或停止读取 JSON 配置。
- `SupportedLanguage`：定义系统支持的语言。

## 注意事项

- `id` 必须是大于 0 的唯一整数；`0` 专门表示监听组件尚未绑定记录。
- `audioPath` 保存的是 `Resources` 相对路径，不包含 `Resources` 目录名和文件扩展名。
- 百度翻译的 App ID 和密钥由编辑器翻译配置窗口管理，不应提交真实密钥到公开仓库。
- 本包只返回音频路径，不加载或播放 AudioClip；由调用方按项目音频方案处理。
- 回退语言只在缺少该语言记录时使用，已有记录的空文本不会自动换成其他语言。
- 运行时加载是异步的；手动查询应等待 `IsLoaded` 或 `LoadCompleted`，加载失败可检查 `LastLoadError`。
- 可导入 Language Switching 示例；现有“创建示例配置”菜单仍可独立使用。

## 环境要求

- Unity 2021.3 或更高版本
- TextMeshPro 3.0.6

## 许可证

本包使用 MIT License，详情参见 `LICENSE.md`。

## Inspector 统一依赖

本包依赖 `com.yvsim.inspector-groups` `1.0.0`，只声明字段、状态与业务操作。Inspector 的布局、样式和控件绘制由 Inspector Groups 提供；包内分组使用框架样式，用户新增分组默认使用普通样式。核心包不依赖其他 Yvsim 业务包，Samples 的明确可选组合除外。

## 从 GitHub 安装

仓库：[com.yvsim.multilingual-text](https://github.com/liuyan7669/com.yvsim.multilingual-text)。仓库名与 UPM 包名一致。

先通过 Package Manager 的 Add package from git URL 安装以下 Yvsim 依赖，再安装本包：

- `https://github.com/liuyan7669/com.yvsim.inspector-groups.git#main`（1.0.0）

在 Unity Package Manager 选择 Add package from git URL，输入：

```text
https://github.com/liuyan7669/com.yvsim.multilingual-text.git#main
```

此地址跟随 main 分支。需要固定 1.0.0 源码时，将 `#main` 改为 `#v1.0.0`；Unity 官方依赖仍按 package.json 解析。

## 从 OpenUPM 安装

在 Unity 的 Project Settings > Package Manager 中添加 Scoped Registry：

- Name：OpenUPM
- URL：`https://package.openupm.com`
- Scope：`com.yvsim`

然后在 Package Manager 中通过 Add package by name 输入 `com.yvsim.multilingual-text`，版本填 `1.0.0`。也可从 My Registries 中选择本包。声明的 Yvsim 依赖由 OpenUPM 解析，Unity 官方依赖仍由 Unity 注册表提供。

如果已安装 OpenUPM CLI，可在工程根目录执行：

```sh
openupm add com.yvsim.multilingual-text@1.0.0
```

[OpenUPM 包页面](https://openupm.com/packages/com.yvsim.multilingual-text/)
