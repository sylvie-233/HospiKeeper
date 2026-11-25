# Sylvie233.UI · Ant 风格 Avalonia 控件库

与业务无关的可复用 Avalonia 12 设计系统，供 HospiKeeper.Desktop 使用。主色 `#1677FF`，控件尺寸偏紧凑，交互参考 Ant Design。

## 控件

| 控件 | 用途 |
| --- | --- |
| `AntButton` | Primary / Default / Danger / Text / Link，Small / Middle / Large |
| `AntTag` | 默认 / 成功 / 处理中 / 警告 / 失败 / 紫色 / 青色 |
| `AntCard` / `AntStatCard` | 内容卡片与指标卡 |
| `AntPageHeader` / `AntSectionTitle` | 页面标题与分区标题 |
| `AntFormItem` | 标签、必填星号、错误信息 |
| `AntModal` | 主窗口内模态弹窗 |
| `AntRoomCard` | 六种房态渐变卡片 |
| `AntTable` / `AntTableColumn` | 替代已废弃 DataGrid 的轻量表格，默认每页 10 条 |
| `AntBarChart` | 自绘柱状图，无图表库依赖 |
| `AntEmpty` / `AntToast` / `AntIcons` | 空态、轻提示、图标 |

## 主题与样式

- `Theme/AntResources.axaml`：颜色、字号、圆角等 Token；
- `Theme/AntControls.axaml`：控件 `ControlTheme`；
- `Theme/AntStyles.axaml`：输入框、下拉、菜单、票据等统一样式。
- 交互动效：页面淡入、弹窗 / 遮罩淡入、Toast 淡入淡出、按钮与表格行 hover 过渡。

## 使用

桌面端通过项目引用 `Sylvie233.UI`，页面使用 `AntPageHeader` + `AntCard` + `AntTable` 组合。新控件需要同时提供 C# 属性与 `ControlTheme`，并在 `AntControls.axaml` / `AntStyles.axaml` 注册。

该模块不引用 `HospiKeeper.Domain` / `Application` / `Api` / `Caching` / `Logging`，可以被其他 Avalonia 项目直接复用。

> 本库依赖 Avalonia，不能用于 MAUI Android 管理端；App 端使用 MAUI XAML 自身样式。
