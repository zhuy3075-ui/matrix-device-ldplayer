# AP-041 离线图像与报告诊断

## 背景与范围

基线为 `a23348ba78093b3fcde385c87dde3ddfd81a818d`。原 s01 在 `offline-reject` 的 OS/WPF 指针一致前置失败，随后报告因非有限 Double 保存失败；原件与 11 项 NOT-RUN 保留。本阶段不重跑原指针测试，不改生产缓存、输入门禁、主题、原负例或原 JSON 全局配置。

已知：原负例只改 State、不改 Generation；完成缓存的合法 Offline 历史要求 Generation 增一；Image 可见不保证有 Source；原 InPicture 不校验零像素，而生产 MapImage 拒绝无 Source。上述事实还不能证明 s01 的具体异常字段或与 AP-019/020/033 同因。

## 最小方案与取舍

推荐在独立 artifacts Demo 窗口增加明确的诊断入口，复用真实主控 Image 绑定、现有截图发布和 Info 通知，只观察同代 Offline 与增代 Offline 的不同结果。普通启动与原验收入口行为不变。所有诊断几何数据投影为有限数或 null，同时明确标记 NaN/正负无穷；不将 NaN 偷换为零。

更简单的纯算术测试只能证明零像素公式可产生非有限值，不能证明实际绑定撤图，因此仅作为补充反例。拒绝直接跳过原 Offline 定位或全局允许非标准 JSON 浮点值：前者缺因果证据，后者会掩盖无效坐标并改变所有日志合同。

## P0 任务与验收

1. 独立模式 `--diagnose-offline-geometry` 与严格参数、显式 Demo/独立 artifacts 绝对目录、单模式、有限尺寸验证。拒绝真实后端、未知参数与混合模式；禁 API/自动接入，窗口 ShowActivated=false，不加入 RunSmoke。
2. 不调用 Activate、Focus、SetCursorPos、GetCursorPos、SendInput、Mouse.Synchronize、鼠标捕获或鼠标路由，不触发真实 guest/ADB/应用权限/VPN。
3. 使用真实完成截图发布到主控 A，当前设备 B；记录基线 Source/row Screenshot 引用关系、像素、RenderSize、可见性、状态与代次、旧公式结果和生产 MapImage 结果。
4. 对同代 Offline 记录同步与固定 DataBind/Render/ApplicationIdle 后的相同字段；不循环等待假设、不主动撤图或写假图。恢复合法 Ready 并重新发布后，对 Generation+1 的 Offline 做正对照。诊断前置与安全断言未满足时保留 FAIL；观测与假设不同则标记假设不成立，不重试。快照前后比较真实状态以证明读操作不改状态；需要缓存身份时只读反射严格白名单，不修改或调用私有成员。
5. 用 nullable 有限数与分类 flag 投影所有 Point/Size/Rect/Double。用 NaN、正负无穷和有限值反例证明标准 JSON 保存/解析不丢分类；即使某一诊断断言失败，仍保存已获得观察与错误。
6. 准备、源哈希/原测试守卫、构建与原 158 项、单次诊断执行、原始 stdout/stderr 与 JSON、子进程终态/清理均记录；单 child 300 秒、清理 5 秒、流排空 10 秒，无重试。独立 reviewer 审核后才执行。

## 判定与风险

- 若新实验观测到同代 Offline 撤图、仍可见、零像素、旧公式非有限，而增代 Offline 保留图且公式有限，只能确认该受控 Demo 链，不倒填 s01 的具体字段。
- 若结果矛盾，停止实施预想修复并保留原日志；不通过修改缓存策略、激活窗口或重复测试获得绿。
- 新入口不是物理点击、原负例、六尺寸完整设备/监控、真实双实例、DPI 或长期稳定性验收。AP-041 原输入失败、AP-019/020/033 和全部实机缺口继续开放。
- 原 GUI 测试报告的局部有限数投影及原 Offline 负例处理须作为取得证据后的独立修复，不混进本诊断。

## 当前状态

独立 d01 与主仓 r01 已各完成一次构建及诊断，实际结果如下；原设备负例和失败报告出口尚未修复。仅新模式在创建目录前拒绝已有 data-dir 与任何 --screenshot 参数；本诊断只写 JSON，不输出图片。该模式不请求窗口激活并跳过后台窗口恢复；用真实 SetMainAsync 和完成截图发布建立 MainA/CurrentB。复用种子图后的原一次固定 Flush，在断言前记录请求/实际窗口尺寸与加载、可见、窗口状态，要求实际尺寸与请求相差小于 0.5，不激活或重试。清理恢复的是已准备的自有诊断基线，不声称通过反射清空启动前 Main=null；随后由 App 关闭释放整个自有窗口/runtime。同代和增代仅改变自有行 Info，catalog 仍 Ready，报告明确区分，不冒充真实 Catalog 断连。

快照反射只读白名单为 completedScreenshots、completedDebugImage、completedNodeTree 三个字段；只比较缓存条目引用、数量及行引用，不修改缓存或调用私有方法。所有后台原 Task 留存并在 finally 取消截止令牌、有界观察终态；截图/计数读取使用传入实际后端的关联截止令牌。所有 Double 的局部投影含 value/null、finite、kind，标准 JSON 反例重新解析；报告保存失败会向 stderr 留下明确 REPORT-FAIL 及已获局部内容。

## 实际验证

r1 仅经过静态准备，未构建或执行，原件保留。独审发现缺少窗口实际尺寸取证和可选截图可能碰撞自有输出后，r2 只补 4 行尺寸观察/断言并删除截图能力，原 await 数 21 不变。最终 App SHA `6999B4725C7638781CEBF34D6B46F23F4183A45FFDEBB60663ADB129FA6D1BC8`；新诊断 SHA `545A8CBFF9713F407D2560C0844B4CB1E8EC51C08DAE14265CAB106D1B979FA0`。原 Main、VM、Wall、API、主题和 21 份旧 Smoke 源未改。

| 执行 | 实际结果 | 范围 |
|---|---|---|
| 独立 d01 Build / PID 30468 | exit0，76.709 秒；构建24.48秒，158项51.42秒，0警告错误 | 原158项名称及顺序保持，实际PDB27源同冻结文件 |
| d01 三参数反例 | 缺Demo、非artifacts、混合smoke均exit1，精确错误匹配，目标目录未创建 | 三份预期stderr分别476/476/229字节保留，不宣称全部stderr为空；已有目录/其它参数未在本轮动态穷举 |
| d01 新诊断 / PID 46996 | exit0，2.015秒；实际窗口390×640，27条已记录检查、22 Task全完成，cleanup=true/errors空 | 8条观察含6次同步快照；没有指针输入、原设备套或真实guest |
| 主仓 r01 Build / PID 22880 | exit0，72.003秒；构建20.09秒，158项51.12秒，0警告错误 | 独立实际执行输入与主仓149路径逐一canonical/完整规范化文本相同，147原raw保持 |
| 主仓 r01 新诊断 / PID 40764 | exit0，1.955秒；窗口/观察/Task/清理与d01相同，报告字节相同 | 没有重跑三参数反例、原Smoke或原指针测试；两进程四原流终态，无timeout/kill/owned残留 |

独立 d01 共5子进程/10原始流，11次149输入快照；主仓 r01 共2子进程/4原始流，5次149输入快照。原配置、两用户PNG、旧158原输出保持；独立阶段保留原主仓PDB，主仓构建有意生成新PDB，改由实际PDB27源校验及诊断前后DLL/三PDB四物稳定证明，不能错误要求旧编译物跨Build不变。

实际两份报告 SHA 均为 `3E0F0766BF6AF78A5DDBA9B469CA87CE4872B6F852AEC3FBBA1536C0D2D1DACE`。它们分别由各自新建工作区的真实进程写出；报告不含随机时间/路径，因此字节一致，不用旧报告补填新运行。

## 观测结论

- Ready：主控A、当前B；缓存1条，原绑定的360×640截图存在。六次同步观察的受监测身份、图引用、缓存和几何在入口/出口完全相同；不宣称全部系统状态无变化。
- 同代Offline：Info通知后即撤图且缓存清空，Image仍Visible。即时Render仍82×145，旧比例计算为正无穷、点为NaN；固定Flush后Render为0×0，比例与点仍非有限。生产MapImage使用有限中心点时实际返回null。
- 恢复Ready重新发布后：新图存在；仅row的Generation+1 Offline对照保留同一图/缓存，公式有限、生产映射为180,320。Catalog全程Ready/g1，不能称真实断线或目录接纳流程通过。
- 两个受控假设均成立。A/B实际Demo计数、文本和包名读前后相同；操作记录为1次准备Main、2次截图发布、4次Dump、4次Info赋值、6次快照，无额外输入派发/自动截图接纳。
- 标准JSON独审递归核454个实际number全有限；198个数值投影为169有限、17 NaN、7正无穷、5负无穷。所有非有限的value为null，finite/kind保留，Empty/null反例可回读。本证明只针对新诊断出口，不意味着旧device-layout.json已修。

## 证据与后续

原件只保留本机ignored artifacts，不上传新原始证据。独立目录为 `.worktrees/device-offline-verifier/artifacts/ap041/diagnostic-d01`，报告在其 `artifacts/od/d01/diagnose-390x640`；独审 SHA `7A903AF9E610F24333C5895FFE43A639BD86097C3D99D9F42EF8BECC29C30B10`，execution `9F675AC3AECDED27A178701A10C8837964D774594600ED8A97F629184D530CB3`，末快照 `FFFB4D1260C3889C361434F899809798076F42336469ACDB4919B20B2EB1C0E5`。

主仓原件在 `artifacts/ap041-main/root-r01`，报告在 `artifacts/od/root-r01/diagnose-390x640`；execution SHA `ACF6A83E83DD7118BB3E0CE26DC753584725BAA02BB5CE32DD0E8DB6B505AE14`，末快照 `9F49C71A4E9FF33A9F81EC57DC8BD9ACFE352A4A3CCAD513FE97A964A9770561`，DLL `95F0FF99D18652DA6A2A7191383E9D49851A4E18995D781DC63026438ACCFC21`。终态独审 SHA `80C101EFDD1A490A5C82DEC2E1BA1C793C135A44F54233FA39CE7205873B5FD3` 已通过，仅准原私有开发分支有限诊断源码交付；核包、暂存和推送以实际回执/Git记录为准，不提前宣称已交付，不合并发布。

下一独立修复先保留原有效Down、Up、释放捕获及A/B零派发断言，评估仅offline-reject不再对撤图状态重新OS定位；它验证的是组合拒绝，不是隔离单个Online守卫。原source-generation-reject也有撤图的静态候选，但尚未运行，不得顺手全部跳过。原报告出口须以局部有限数编码保留失败/阶段/异常，并用同一出口做无OS输入的标准JSON反例，不能只测平行toy函数、全局放行NaN或改原FAIL为PASS。

原s01的具体异常字段和OS/WPF前置成因仍无直接记录，AP-041输入/原报告修正、AP-019/020/033、11项NOT-RUN、真实GUI/双实例同步与安装卸载/最终30分钟及A08-A16全部继续；新诊断通过不是这些项目的完成凭据。
