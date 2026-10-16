# A07：接口排空后恢复应用读取

日期：2026-10-16；基线ebda58de1a7ee1e3e6137646f22e820a46ca4c54。承接[应用待办恢复](A07_APPLICATION_PENDING.md)的API释放未验项；本阶段不替代完整GUI、实机双实例或最终30分钟。

## 事实与目标

准备时的原350ms应用读取Tick会在API活动时停止timer并保留pending。ApiServiceManager对三个HTTP入口计数，但最后请求结束只减计数，没有结束通知；原应用页仅由选择、可见性及VM通知再次排队。该静态候选随后取得8条产品红，修复版14条独立专项通过，执行过程分段保留如下。

根问题候选是暂时阻塞解除后没有唤醒，不是需要用户反复刷新。目标是在请求实际排空后，自动完成已有的最终明确选择List意图一次；不创建新用户意图、不自动安装/启停/清数据/卸载。全部身份、保护、在线、脚本、传输与Busy门禁保持。

## 推荐方案与取舍

推荐先用真实受控HTTP复现，再在Host最后请求计数归零时提供轻量提示事件；应用窗口通过自身Dispatcher补排原350ms debounce。提示不是原子空闲锁，UI回调执行时必须再次检查当前API/runtime引用、关闭及pending，原CanManage通过后才消费意图。换runtime与Close需退订，已排队旧来源回调也不得越过身份/关闭检查；监听者异常不能改变HTTP结果，必须隔离并记日志。

更简单的持续polling可恢复读取，但增加后台工作并掩盖缺少状态通知，不推荐。拒绝移除API门禁、将API加入全局HasActiveOperations、直接在Kestrel线程触碰WPF集合或直接List：这些会改变其他页面/关闭语义或带来并发风险。生产范围仅Host计数通知与应用页生命周期，不做公共状态重构；如静态方案经真实反例不成立，以证据推翻，不强行修改。

## 验收与边界

- P0：原生产真实只读HTTP请求进入公共ServiceInfoProvider的TCS门，证明HasActiveRequests=true、VM非Busy；可用CheckBox经原Binding勾选，同一原Tick观察API仍活跃且零List、pending仍在。只释放HTTP后应自动读取一次，不由夹具额外制造VM通知或手工刷新；自动读取本身的Busy通知是合法原行为。
- P0：两个并发请求分别交换释放顺序，首个释放仍零List，最后释放一次；API期间改选仅最终目标，清空后零读，全局Current/Main/群控保持。
- P0：API与Push/脚本交叠，先释放一项不能越过剩余门禁；全部实际终态后恢复，最大List并发1。读取失败不循环，新用户意图可恢复。
- P1：隐藏期间不读、返回恢复；关闭和已排队通知不得新增List，当前runtime/API变化后旧通知无效。HTTP服务失败/取消与请求成功分别报告，计数归零不能称请求成功。
- P1：现有公开接入流程拒绝Demo热换runtime，不反射替换私有字段制造通过；旧来源守卫先独立静审，真实接入换代仍须后续专用实例验证并明确记未验，不与本阶段纯API窗口关闭混淆。
- P1：新增专项只在自有一致Demo/STA窗口、随机可用loopback端口、stub后端运行；复用原逻辑WPF CheckBox/TwoWay与Tick观察，不冒称物理输入。不开真实实例、ADB、VPN、系统代理或外部服务，不读写CODEBRAIN。
- P1：原158正式回归、全部旧夹具原字节/调用相对次序保持；新专项加入完整smoke最前，失败保留，后续未执行记NOT-RUN。旧AP-033指针前置失败不改、不跳过、不因新专项通过就关闭。
- P1：逐case记录原请求/响应、TCS、Tick、pending、活动/Task终态、List目标与次数、清理；原始双流、实际PDB/冻结输入与配置核对。客户端响应/provider门退出不等于Host finally已排空，另观察真实计数归零。关闭专项需独立拥有纯API窗口生命周期，不为复用旧脚本Close前置而制造活动，也不二次Shutdown。内部等待与外层child300秒均有界，超时仅终结本轮owned进程，不盲重启；所有自有Task/进程结束才归锁。

## 执行计划

1. P0：隔离app-api-idle-tests仅新增正式专项及Main唯一hook；独立app-api-idle-verifier冻结输入并静审后，以原生产单次Build/完整1000 smoke取红。专门区分产品红、前置、夹具及清理失败。
2. P0：仅确认产品红后，在app-api-idle-fix修复Host及应用页；测试断言不弱化，独立相同专项与原基线复验，最多三轮review-fix。
3. P1：主仓按冻结字节整合，运行同源回归与页面代表验证；更新README、状态、源码包，经独审后单功能push原私有feature，不合并发布。原两PNG和原始证据/配置/依赖不上传。

当前只有定点调查和规格/隔离树准备，尚未执行本阶段Build、HTTP或WPF专项，未确认产品红或完成修复。Windows Computer Use此前两次抓取超时的限制仍保留；本阶段受控逻辑WPF不替代真实GUI操作验收。

## 原生产真实基线

以上“尚未执行”为准备时快照。r1a冻结后，独立baseline-b01只运行一次：Build子进程1836正常exit0/81.410秒，其中实际构建27.89秒/0警告错误、原158项52.53秒；158名称/顺序保持，PDB精确核23源。Smoke66428实际exit1/55.727秒，12条用例8 PRODUCT-FAIL/4 PASS，无前置、夹具或清理失败。

8条红为单API、双请求两种释放顺序、API内最终改选B、Push先释放、真实HTTP503、List错误例的首个List之前，以及真实客户端取消。每条先证明原350ms Tick时API仍活跃、pending保留、零List；实际server计数归零后5秒仍无List。客户端取消及503仍保留各自原结局，不算HTTP成功。原List错误例尚未到“错误后新意图”子路径，明确NOT-RUN。

4条绿为清空选择、离页释放后返回、先释放API后脚本终态、纯API在途Close；不要求原生产所有用例均红。14个HTTP门与64个可跟踪Task均终态、12清理完成，无超时kill或owned残留，5次147输入/19旧Smoke/Main剩余全文及配置E4E33保持。新聚合失败后全部旧专项与12页未执行，不冒称全GUI通过。

报告位于工作区`.worktrees/app-api-idle-verifier/artifacts/a/b01/artifacts/aa/d3cc3b81/application-api-pending.json`，SHA `EFB84B1B02BB79324FBD3F3F42B27C1D69666F2047B1A73BCC507B20F19AFAB1`；独审SHA `B652D717398F09F018991D06D11D427519A8C5CDB20D7763591740388560FF37`。独审脚本首次访问被忽略的空JSON属性触发StrictMode，仅本地添加属性守卫后审同一原件，未重跑产品。所有原始结果保留，不上传raw。

## 首轮修复准备

root在另一隔离树仅两生产源`32+/2-`：Host归零后逐observer发RequestsDrained，应用页在自身Dispatcher补排原debounce，已排队时重核source/runtime/关闭，换runtime退旧订新、Closed退订。原350ms、CanManage/RequireContext、身份/保护及全局HasActiveOperations不改。Host冻结SHA `7851505D510558256E58845F69A667D415469DFDCA4A50CAAC73CEFEDB4FC6AC`；ApplicationsView冻结SHA `3FDEB924EB114C409F23C975831DF855DC3A5B1950640077C30842D3358FE8F0`，独立静审通过，尚未运行修复版。

上述“尚未运行”为首轮准备快照。隔离通知异常的结论限当前可用日志依赖；生产没有RollingLog.EntryAdded订阅，后续专项仅增加正常的只读marker观察者，假设该日志observer自身再抛异常的双故障并未验证，不修改日志模块。两个真实红之后新增的事件合同用例（已排队后Close、throwing subscriber）与原12条分开标记；不把新增case倒填基线。

## 冻结测试与独立复验

原生产运行使用r1a夹具，SHA `FB6AA2317386AB29892261F0CFDD3763E1FCE63F20066780A7A6D937EA508BC9`。执行前仅纠正ProviderTask先发布再发Entered信号，及坏HTTP结构的产品失败分类；r1原件与delta保留，没有运行原r1。原生产首次12例红绿记录保持。

修复版使用r2夹具，SHA `363694C78D1CD6D7AFAE5F5268712BF3B222D63FD30F7DB393C682AC448D2394`；Main仅在完整smoke首部新增`VerifyApplicationApiPendingAsync`，SHA `2669087FAD9D5C245D639C19033002373C89C16BF522729801AEE0D5C50E1C30`。r2保留原12路径，仅加强双请求的通知次数断言、提取相同释放验证；新增两个公共事件合同，不声称同一14例夹具在原生产运行过。19份旧Smoke原件与调用相对次序保持。

独立final-f01仅执行一次，session74125已终态：Build16312 exit0/69.458秒，其中实际构建17.84秒、0警告错误，原158项50.85秒，名称和顺序完全相同；完整1000×700 Demo Smoke45760 exit0/148.325秒。14 PASS、496检查、100条可取得Task记录全终态、16个真实HTTP门及14清理完成。

单请求、并发两顺序、最新选择B、双门、真实503与真实客户端取消均在实际计数排空后只补一次List；首个并发请求结束时零通知/零List。List故障例此轮实际进入首List失败、不循环，新的明确选择再成功，共两次List。清空与关闭零List，离页不读、返回恢复；全局Current/Main/群控选择不变。HTTP失败或取消并未改写为成功。

异常订阅用例实际顺序为View→Scope→throw→normal，throw、normal及相同marker日志各一次，HTTP200及原待办恢复；原Shutdown后在实际日志文件核到已落盘marker，不仅检查内存观察者。队列用例在同UI栈暂禁消息处理，只等待纯后置TCS不超过3秒；只读Hooks在前后observer之间捕获唯一Background Pending操作。close请求时刻8487097179425早于原回调Started8487097213131，随后同一原操作Completed、零List，Hooks/observer全部退订。此证据是关闭请求之后，不宣称一定在Closed事件之后；未Abort队列或伪造回调。

旧12待办、19重连、18缓存、10截屏、11节点、16首载、49发布、10应用身份、原应用smoke及12页流程全部实际到达通过；desktop.error为空、apps1，nodes0/colors0照旧，不冒充外层节点/取色效果。stderr非空16026字节，为保留的旧节点/外观已捕获诊断，不称零输出。AP-033本次通过不能证明其间歇输入源成因已修。

五次147输入快照、其余143原件、19旧Smoke、Main剥除唯一hook后全文及配置E4E33均保持；PDB精确对应23源。无timeout/kill/清理错误，自有进程0后显式归锁。独立DLL SHA `092CB57D360922B8DAA34D9F0BBC61C3D1FD212456719E024610B3C7B76A0579`；新报告本机`.worktrees/app-api-idle-verifier/artifacts/a/f01/artifacts/aa/c4d39b16/application-api-pending.json` SHA `68133AB9A03BAA104833C02225C00AD11099D45243BAFE6620D28049FF5EC10D`，独审SHA `AE17F17F8CDE7C84A07E398AA993641F1AFF450CF221E224807433AE3E28B53D`，限域批准四源整合。

## 主仓与剩余验收

已按冻结字节机械导入Host、ApplicationsView、新fixture及Main；主仓预导入基线SHA `EB9E7FD14D0151AA29247C1B4BCF7B0134E208726AE991B522C846B822D97569`。其余143保留主仓原raw，所有147 canonical与规范化全文逐项匹配独立实际执行输入；PDB按root自身raw精确核23源，不把换行差异当未编译的通过证明。

单次主仓r01 session4546已真正终态：Build30508 exit0/72.155秒，其中实际构建20.24秒、0警告错误、原158项51.08秒；Smoke37868 exit0/144.498秒。新14 PASS/496检查/100任务记录终态/16 HTTP门/14清理，原八旧专项及原应用/12页全部实际通过。异常observer/normal与已flush日志marker各一次；队列原操作closeAt8492284197511早于UI回调Started8492284221368，随后Completed、零List及完整退订。五次147/19旧源/Main剩余全文/原配置保持，无timeout/kill/清理错误或owned进程残留；非空stderr15948字节为旧已捕获节点/外观诊断，原流保留。desktop.error为空，nodes0/colors0不外推效果；最终1000截图仅设备页代表，不称应用页六尺寸截图证据。

主仓报告`artifacts/ai/r01/artifacts/aa/00d82563/application-api-pending.json` SHA `352AB302BB14877A1C8FEEFC2400F38742129C6D32C9EB859F60F00C43FE875B`，acceptance SHA `02758ADAFA93FF6270DF05047060FC4BE5DF85E6B400BF8D44060777648B76D8`；独审`artifacts/app-api-idle-main/root-r01/independent-audit.json` SHA `1916917D8607A1BDA3A6FF09B75BF7C3DCC331486E9F728A365E6A0A68283C6A`。root DLL SHA `6947B8266E6784A084248B37D0F7545D8054C3F115EBD71546DF5E532D64A509`；实际最后快照SHA `C7845604021EB18B6D060F0E551C09DDA75BD4932DD3ABC8973DAF033B7C4247`，非实现者核原报告/四流/实际PDB后批准本功能限域交付。

正式源文件、八份相关文档和三清单经核包/独审后单功能push原私有feature，不合并发布；具体commit/远端refs和包摘要以Git及本机`artifacts/app-api-idle-main/push-complete.json`为准，不从准备流程推断已push。原两PNG、APK、本机配置、二进制依赖和本轮raw不上传，既有AP-021历史材料不清理，不称整个仓库原始材料净化。

本阶段只用自有Demo窗口、真实loopback HTTP和逻辑WPF控件；不是物理输入、Android、原生文件框/确认、真实runtime热换、双实例同步、应用正文六尺寸或最终30分钟验收。当前CMFA本轮安装已在上一阶段停止并卸载，旧权限页主页FAIL不改；AP-003/AP-033/AP-036及A07余项、A08-A16和完整目标继续开放。
