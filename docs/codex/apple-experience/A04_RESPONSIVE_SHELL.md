# A04 公共响应式与窗口尺寸契约

日期：2026-10-15。基线08c4f8c。范围仅共享壳层、公共表单及指定自有弹窗，不代表12页固定多栏正文已经完成手机适配。状态：最终版本已独立PASS-OFFLINE共享层；两项P2和其余页面/实机范围未完成。

## 根因与处理

- 原XAML、smoke、设置编辑器和持久化分别使用1000×700与1100×720下限，390×640保存后被放大；真实红测149/150。
- 四处统一引用AppSettings最低390×640、最高7680×4320，保留默认1440×900。非有限或越界的编辑值明确拒绝。
- 外窗宽度低于700使用同一导航对象的抽屉；700至1099默认72图标栏，1100及以上默认212侧栏。手工偏好不因断点被覆盖；抽屉通过选择、Escape和遮罩关闭，返回可见焦点。
- 窄屏单设备目标换行；设置仅进入设置。网络连接与立即更新目录收进可点击的“更多”菜单，不再要求右键。页脚分行、长错误有界滚动。
- Field按自身宽度459/460重排，保留同一输入、绑定、文本、选区、光标和焦点；Toolbar窄时约束输入宽度，变宽恢复原上限，不缩放整页或字体。
- 自有弹窗依据Owner所在显示器工作区约束；保留原最小尺寸，只在可用上限不足时降低。复用窗口重开重新适配，可见窗口随Owner移动/调整；自动跟随失败保留最后有效位置并记录诊断，不冒泡崩溃。
- 资源表单采用可滚动正文与固定底部取消/提交。应用计划、目录日志、设备详情、网络扫描和原生投屏容器接入边界助手。投屏几何校验在构造/注册host之前，不改变A02身份守卫。

## 文件范围

| 文件 | 改动 |
|---|---|
| Core/Models/Contracts.cs | 窗口尺寸常量与原默认值 |
| Host/Persistence/SettingsStore.cs | Normalize使用统一窗口约定 |
| Desktop/MainWindow.xaml、App.xaml.cs、WorkspaceViewModel.cs | XAML、启动参数及设置编辑校验；独立shell验收模式强制Demo、禁API/自动连接并互斥 |
| Desktop/ResponsiveLayout.cs | Field、Toolbar、当前显示器坐标与Owner-fit助手 |
| Desktop/Views/WorkspaceShell.cs | 单一导航、断点、目标/菜单/页脚/错误与复用弹窗 |
| Desktop/MainWindow.xaml.cs | 公共助手接入、长日志、仅Demo启用的模态取证钩子 |
| Desktop/Views/ApplicationsView.cs、UnifiedDevicesView.cs、NativeView.cs | 指定弹窗接入；资源表单取消与滚动；原生host几何前置 |
| Desktop/Views/ResponsiveShellSmoke.cs、Tests/Program.cs | 共享层交互/几何与保存重载正式回归 |
| Desktop/Views/AppleAppearanceSmoke.cs | 仅Toggle焦点前置、真实渲染帧及失败诊断，不改生产主题或动效条件 |

以上均位于src/MatrixDevice.*或tests/MatrixDevice.Tests，共14源文件；无ADB、接口、设备授权或真实应用数据修改。

## 验证与失败记录

- 主仓最终Build.ps1 -Verify：150/150，51.41秒，0警告/错误。最终DLL SHA256：79E3F633E6594DF811D5F6DB563A849CC534BA9569E470128A09B7CF515771BF。首轮51.83秒的程序集不冒充增量后的最终程序集。
- 实现者最终Build -Verify：150/150，52.02秒；final6-shell六尺寸每轮135断言，真实Loaded/Visible/HWND、请求与ActualWidth/Height相同。尺寸为390×640、390×844、768×1024、1000×700、1440×900、1920×1080。
- 保存路径包含SettingsEditor.Apply→Save→新Store.Load，390×640/844与768×1024保持不变；699/700、1099/1100断点、十二原导航、More、字段/工具栏、长页脚/错误、实际取消和Owner跟随均有独立断言。
- 实现者完整桌面三尺寸通过；最后原生构造顺序调整后1000×700完整回归、25主控守卫、12页idle捕获再次通过。未删原桌面业务断言来凑六尺寸通过。
- 原失败轮次保留：尺寸Normalize和固定侧栏是真实红测；Field初态未获焦点、页脚/notice取证错误属于夹具问题。选页加载抢焦点属于生产时序缺陷，修为Loaded后一次Input队列返还；断点后的焦点未被测试重新强制恢复。
- final5只渲染modal.Content导致透明黑底，已拒绝该视觉证据；final6渲染真实Window，root目视390壳层/抽屉及两种模态正文和按钮。未后处理刷白图片；WPF取证不包含系统非客户区标题栏像素。
- Patch SHA256：EF7D087D56AA2CA5AA825483214C715E6679AE48E2BB4322A96952A771432054；仅夹具增量5BB4928E12B68EB4AE9505784DC6DA1AC560F156983D9966DD442A80582849E9。git apply使两个新增文件由CRLF变LF，已单独核验，不混称原始字节摘要相同。
- 独立首轮150/150（54.74秒）及六尺寸shell通过，完整1000通过但1440的旧Toggle动效断言失败，已正式拒绝；同二进制仅一次1440-r2通过仍不抹除首败。旧夹具未记录焦点/clock前置，尚不足区分生产动效与焦点时序；后续仅补真实焦点初态、有界实际渲染帧及逐阶段诊断，未放宽原动效条件，不认定旧失败已确诊生产根因。
- 夹具增量后实现者150/150（51.00秒），完整三尺寸各209外观case（新增More名称及两条前置/帧断言）、25主控守卫及12页捕获通过；实际一次动效观察opacity=0.06且有clock。未对动画反复重试，也没有改生产动效来凑通过；首次独立失败仍不认定为已定位生产缺陷。
- 最终独立重建150/150（51.90秒）；六尺寸各135断言、实际HWND与请求尺寸一致；三桌面尺寸各209外观/25主控守卫/12 idle页，error空、恢复设备一致。三份Toggle JSON四阶段完整，单次观察真实focus、clock、opacity=0.06；独立目视390壳层/抽屉/两种生产模态，768/1920壳层及三桌面设备四行，tab0与最终PNG逐尺寸SHA相同。verifier程序集摘要76DDF4CFDA812BDAACAEF9241F6BC237615F1AB54B0142E3B0F91D75A21A647B，不冒充主仓二进制摘要。
- 主仓最终390×640 shell135与1000×700完整209外观/25守卫/12idle退出0；root目视设备四行与操作入口，tab0与最终PNG SHA相同。14源文件与实现者归一化逐字一致；用户profile摘要仍E4E33A8287BD931300D28976411897D7AFE8AA9932EEB3B7F5BC2036E5BB05F0。

原始日志/图像在实现者artifacts/apple-responsive、独立verifier的artifacts/apple-responsive-verifier/final-toggle及主仓artifacts/apple-experience/A04，均不上传。独立批准仅限本报告范围，不以实现者自验代替批准。

## 边界与下一步

仅PASS-OFFLINE共享层；六尺寸不代表每页业务布局通过，后续A05至A09仍需逐页处理。没有真实设备、ADB、scrcpy、安装或网络请求；Native只验证容器几何，未进行真实嵌入。原生文件选择器、MessageBox、真实副屏/混合DPI、系统标题栏像素未验；负坐标副屏只证明纯几何用例，不能扩张为物理环境结论。

独立源码审查保留两项P2：首次Owner-fit拒绝时事件订阅可能留存（AP-011），以及长页脚仅ellipsis/tooltip不能证明触屏完整信息可达（AP-012）。这两项纳入A10，不以本阶段几何/边界通过视为修复；完整目标在必需缺口解决前不完成。

A05将单独修复已确认的详情全局筛选/群控选择重复，隔离监控目录，并处理设备/主控/监控正文响应式。完整目标仍未完成。
