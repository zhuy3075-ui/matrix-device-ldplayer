# 当前执行任务

当前目标：全面功能验收、全页面去重、Apple HIG视觉和命名重设计、六种尺寸响应式；每个功能验证后push。完整计划见根plan.md，执行采用ai-coding-workflow，真实subagents交叉审查；superpowers不可见，按相同验收纪律fallback，不声称调用。

| ID | 优先级/状态 | 依赖 | 任务/责任范围 | 验证和完成标志 |
|---|---|---|---|---|
| A00 | P0/完成 | 无 | 主代理保存已验证列表精简基线和当前全目标计划；仅相关源码/测试/README/清单 | 基线8303cdc及完整计划c1be294已push，149/三尺寸证据核对、test备份不变；计划产物完成独立审查 |
| A01 | P0/初始盘点完成，逐页补齐执行证据 | A00文档部分 | 两名独立只读代理审UI结构与实测证据；103可见键+全局/源码及逐字段/模态细表已建立 | 每个控件/能力按N/E/B/C/F/T展开；未执行仍NOT-RUN，不拿清单当通过率 |
| A02 | P0/代码修复完成，真实路径纳入A11 | A01初始矩阵已建立 | 主控A/当前B投屏错目标红测后修复，冻结源/进程证明 | 独立149/三尺寸、25守卫与主仓最终1000通过，见A02_MAIN_TARGET；没有将守卫模拟当实机；单独push |
| A03 | P1/共享层验收完成 | A02 | 共享主题、字体、状态、焦点/动效和全12页显示名称；不动Tag/API/尺寸 | 149/独立三尺寸、每轮195外观/25守卫/12 idle捕获，主仓最终1000通过；见A03_APPEARANCE，独立push |
| A04 | P1/共享阶段验收完成 | A03已推送08c4f8c | 四契约、单一导航/公共Field/Toolbar/指定弹窗；仅测试Toggle焦点诊断增量，不改生产动效 | 独立150/六尺寸各135/三桌面各209外观+25守卫+12idle通过，主仓150/390shell/1000完整复验通过；见A04报告。AP-011/012属A10，固定正文仍由A05-A09验收；单独push |
| A05 | P1/阶段离线验收完成，真实路径仍属A11 | A04已推送2b1fe6e | 职责隔离15155a7、操作文案6c4adce、设备/主控76d0e43；[b2报告](codex/apple-experience/A05_MONITOR_RESPONSIVE.md)为监控正文，本阶段独立提交 | b2独立freeze4：150/六监控各952/308、六设备889或847、三桌面、六shell135、42CLI；主仓150/五代表首轮通过。AP019/020首败保留，真实视频/输入/ADB/稳定性不由离线替代 |
| A06 | P1/a、b1、b2a、b2b、c1及c2离线验收完成，余项开放 | A04 | [a发布隔离](codex/apple-experience/A06_DEBUG_PUBLICATION.md)e235657、[b1首载调度](codex/apple-experience/A06_AUTOMATIC_LOADING.md)ef74216；[b2a节点正文](codex/apple-experience/A06_NODE_RESPONSIVE.md)29ab03f；[b2b截屏正文](codex/apple-experience/A06_SCREENSHOT_RESPONSIVE.md)c7b6d24；[c1完成缓存](codex/apple-experience/A06_COMPLETED_CACHE.md)90815e8；[c2同行恢复首载](codex/apple-experience/A06_RECONNECT_LOADING.md)原19例11红保留 | c2独立150/0警告错误、三桌面各19/289/19清理及旧18/10/11/16/49全套/六shell135，主仓150/1000完整同19及旧五套/390shell135通过；生产C288/MainF243/夹具F1AC。依冻结10/核包/独审13路径后单独push，以实际Git记录为准。原生保存/在途裁剪、AP019/020/023/028及实机仍开放，不把自有1000窗口19例当六尺寸每控件实测 |
| A07 | P0/a、b1、b2及c/d有限域验过，实机/补验仍开放 | A01,A04 | 当前ApplicationsView批次（非旧VM捷径）；[改名身份](codex/apple-experience/A07_APPLICATION_IDENTITY.md)、[待办](codex/apple-experience/A07_APPLICATION_PENDING.md)、[API排空](codex/apple-experience/A07_API_DRAIN.md)、[正文适应](codex/apple-experience/A07_APPLICATION_RESPONSIVE.md)及[测量诊断](codex/apple-experience/A07_TOOLTIP_DIAGNOSTICS.md)保留原红与门禁；[CMFA首验](codex/apple-experience/A07_CMFA_APPLICATIONS.md)/[owned清理](codex/apple-experience/A07_OWNED_CLEANUP.md)不冒充GUI | c02与主仓r01各158/0警告错误、新12/2890/12清理、原17源Smoke链/12页过；当前505字体成员含344隐藏全1.5，非DPI/字体绑定保真。s01六壳层格各135过，但首设备指针前置及报告保存失败，11格NOT-RUN/AP041；20是冻结旧源数，不是20模式已跑。独审仅准原开发分支有限域交付，提交以Git记录为准，不合并发布。真实双实例/当前GUI安装卸载/最终30分钟及权限页缺口保持，不重装授权凑绿 |
| A08 | P1/a格式边界与b自动化正文有限域已验，余项开放 | A04 | [格式边界](codex/apple-experience/A08_SCRIPT_FORMAT.md)；[b工作区](codex/apple-experience/A08_SCRIPT_WORKSPACE.md)四视图、日志阅读及原控件；终端/文本其它路径保持 | b独立/主仓原209、VM9和十二组own-WPF/Demo通过；六尺寸含长字段/实际参数编辑Run/按任务取消/500日志尾窗，原真红及编码/测试包前置失败保留。四源已集成，核包/推送以Git为准；GUI选择器、实机、DPI及完整终端/文本仍必需，不以程序内控件替代完整GUI |
| A09 | P1/未开始 | A04 | 接口/工具/设置逐路径与视觉布局；依赖能力逐项登记 | 路由/端口/归档/保存重启/错误恢复/禁用解释有证据；每修复push |
| A10 | P2/AP-033已补诊断取证，修复和其余细节开放 | A05-A09 | 交互、长内容、键盘/触控、对比度/减少动画/DPI；AP-011/012/015/019/020/023及候选AP-017保持 | [AP-033](codex/apple-experience/AP033_INPUT_DIAGNOSTICS.md)单测试49+保持旧断言，独立150过、完整GUI旧坐标前置仍FAIL；三快照活动源null且OS后续移动，成因未明。非产品修复/全页验收。P2仍必交付，修复分别验证push，不把后续绿或未复现候选当完成 |
| A11 | P0/未开始 | 全部实现 | 冻结最终版本，独立单/双实例实机全路径和30分钟稳定性，完整回归 | 逐需求/控件/能力都有足够证据，失败/未验不计通过；环境不被误改 |
| A12 | P0/未开始 | A11 | 最终审计与报告、问题/修改清单、README/完整源码清单、推送；AP-021历史验收材料归档分类 | 用户目标逐条完成证据；不把路径/SHA检查称为原始证据净化，保留用户文件、不改写Git历史；存在必需缺口不标goal完成；无merge/release |
| A13 | P0/长路径和报告出口已修，完整执行未完成 | A01目录与既有版本 | 全功能逐控件测试、失败修复与问题清单；优先继续双实例同步、新版应用页安装卸载、完整GUI可靠性及最终30分钟 | [AP-035](codex/apple-experience/A13_SQLITE_PATH.md)独立/主仓158及四普通路径CLI通过；[AP-041诊断](codex/apple-experience/AP041_OFFLINE_DIAGNOSTICS.md)保持，[报告子项](codex/apple-experience/AP041_REPORT_RELIABILITY.md)实际红6PASS/8FAIL、同14类绿全过，独立/主仓各158及原390几何报告不变。原负例/S01/11NOTRUN、AP-033、实机与最终30分钟仍开放；每例步骤/预期/实际/证据齐全，每个修复独审后单独push |
| A14 | P1/a1及a2独立/主仓有限域通过；完整闭环开放 | A13a、A08；AI部分再依A15a/A15b与A16b | 复用原引擎；[a1参数绑定](codex/apple-experience/A14_SCRIPT_PARAMETER_BINDINGS.md)、[a2逐目标任务](codex/apple-experience/A14_SCRIPT_JOBS.md)、本地包上传/预览/分发执行；MCP脚本与指定应用动态分析 | a2独立/主仓各209/209含新16协议/业务用例、原4新5 VM全PASS/cleanup，1条测试警告/0错；JobId与实际队列/发送前pin、按任务取消/状态/真实输出具备有限证据。原失败/193保留，GUI/真实合法包全流程、完整RPA与AI采集仍必需，不执行未知代码或冒称沙箱 |
| A15 | P2/六工具Demo独立及主仓有限通过，待核包交付；完整业务及实机开放 | A13a；脚本工具复用A14a的包契约 | [A15a契约](codex/automation-expansion/MCP_DESIGN.md)及[A15b实施](codex/automation-expansion/MCP_STDIO.md)：SDK实际stdio、冻结包/逐任务/防重与退出；real先解决共同所有权，不另起不协调Runtime | 独立及主仓原209 exact；新官方StreamClientTransport接真实OS管道，主仓14顶层/14会话/12raw子例、227调用/13终态job、80原流及清理获独审有限通过。旧Schema/退出失败原件保留，旧Stdio兼容仍失败，13类NOTRUN不计通过。22源已纳主仓，核包push以实际Git为准；最终逐台授权集合/真实1与2/共同所有权仍必需，整个A15未完成 |
| A16 | P3/已追加，初步调研完成，POC未运行 | A13a、独立测试应用/服务及采集权限 | A16a比较代理/HTTP Toolkit/VPN/原始包；A16b先验证受控方案，再实现指定实例会话、停止清理、分析/导出并供A14c使用 | 有官方依据与明确选型；自有请求对应实际捕获记录、目标隔离及停止/容量/退出清理均过。HTTPS/应用级隔离各自验证，无明文能力不报成功 |

当前A13补验：[AP-042](codex/apple-experience/AP042_WORKSPACE_ACCESSIBILITY.md)历史组件红绿与主仓曾通过，最新[A14a1旧链](codex/apple-experience/A14_SCRIPT_PARAMETER_BINDINGS.md)却146→146→146/应用owner缺3/旧残留111，后继NOT-RUN，当前PRODUCT-FAIL开放；[AP-043/AP045](codex/apple-experience/AP043_SKY_AB.md)初始正文可读但官方导航几何错误/无后置state亦保留。原null与根因未定，不重跑凑绿、不用独立无窗VM替代全GUI。真实GUI、双实例同步、应用GUI安装卸载和最终双实例30分钟、A08-A16仍必交付。

最新进展：A14a2逐目标任务已推7736630，A08b工作区已推6cb127d，不重做既有交付。[A13真实同步补验](codex/apple-experience/A13_DUAL_INSTANCE_SYNC.md)专用1/2的四态Tap/Swipe/Home及未勾选反例共14真实VM-SDK效果已独审通过；主仓两源精确集成，唯一原209/11fake早拒/旧VM9/十二own-WPF及清理通过，14child28raw/源PDB/用户保护均获独审。原实机outer1由AP052报告读取缺省null造成，独立离线2正例/6拒例和158原件核对通过，旧失败保持、不重跑设备；不是物理GUI/文字/30分钟通过。本阶段交付仅相关源码、正式验收、README/报告/清单，核包与推送以实际回执/Git为准，不合并发布。

原AP042/AP045、应用GUI安装卸载、完整GUI/DPI及最终双实例30分钟，A08-A16余项与全RPA/MCP/抓包仍必交付；MCP已完成独立Demo实际stdio有限验收并进入主仓复验，不代表真实接入或整个A15完成，不混前一同步交付。不用受限JSON或组件局部绿替代用户全流程。

执行规则：一项先复现/定义预期，再小改、全基线、页面/安全实机、独立verifier、文档/清单、commit/push；独立实现worktree不共享写同一文件。单个拒绝点最多3轮review-fix；需要额外权限或不明风险时停止该动作，记录并继续其他安全工作，不向用户反复提问。

## 剩余验收分组

双实例VM-SDK同步块已获本阶段有限验收后，当前按用户可验收的剩余工作块约为13项，归为7类；这是原A任务的统计口径，不新增需求或改写编号，也不是13项全部从零开发。实际14条双实例动作是已验同步块内的用例数，不能与工作块数量混用；其物理GUI路径仍归完整GUI项，不漏验。新发现可能产生修复子项，不用离线用例数估算完成率。

| 类别 | 剩余块数 | 内容 |
|---|---:|---|
| 真实模拟器主流程 | 1 | 当前应用页安装、读取、启停与卸载；双实例GUI操作仍在下方GUI项 |
| 其他页面业务 | 3 | 终端/文本/文件；接口/工具/设置；节点/截屏原生保存、导出与中断交互 |
| GUI与兼容体验 | 2 | 官方GUI可靠性及12页真实操作；DPI/键盘触控/焦点及其余P2细节 |
| 自动化与AI闭环 | 2 | 真实合法包上传、分发至结果回传；指定应用脚本、采集分析至结果回传 |
| MCP实际接入 | 1 | 已有设计基础上的stdio客户端、权限/错误及目标隔离验证 |
| 抓包 | 2 | 受控POC和选型验证；会话隔离、分析导出及停止清理 |
| 最终收口 | 2 | 最终版本全路径及双实例30分钟；最终审计、源码包和交付 |

当前优先应用GUI生命周期、完整GUI回归和最终双实例30分钟；最后一项须在最终版本冻结后执行，不能为先扣数字而用当前短测替代。共享主题、多个页面响应式、参数绑定、JobId及自动化工作区已有有限域交付，不重复计算为从未开始。

## 新增任务执行顺序

本次真实14效果和主仓代表已获独审有限通过，全部原失败保持；详细证据及交付口径见上方当前进展与A13报告。这里只继续约定A13-A16的完整任务顺序，不把同步局部验收扩成全功能完成。

2026-10-16追加。A13-A16按用户指定的优先顺序排列，不改写A00-A12历史；任务优先级不同于缺陷严重性，四项均为必交付。A11的“全部实现”自此包含原未完成项及A13-A16，A12最终审计不得在新增项仍未完成时关闭。

1. A13a核定当前源码/二进制、逐控件目录、目标及依赖；登记深路径SQLite14、AP-033和单实例限域Live结果。已有一次150项通过与实例1八项通过，不等于本任务完成。
2. A13b继续双实例同步的四种开关状态、新版ApplicationsView独立测试包安装/执行/卸载和完整GUI失败定位；沿用AP编号和原断言，逐修复验证并push。最终版本双实例30分钟保持必验，不以旧单实例长测代替。
3. A14a冻结现有脚本兼容、扩展格式/参数及安全边界；A15a可同时完成独立工具契约审查。A14b实现上传、预览、分发/执行和逐目标作业回传，再提供给A15b真实客户端接入测试。
4. A16a先完成方案调查和安全选型；A16b只对专用实例与自有应用做POC，通过后实现会话并接入MCP。A14c再完成“AI指定目标→执行脚本→采集/分析→结果回传”的受控全链路，不以模拟HTTP响应充数。
5. A13c将新增界面和能力加入原矩阵，完成六尺寸/异常/取消/目标变化回归；A11冻结最终版本完成真实全路径和30分钟，A12汇总问题与修改内容。实现失败时回到对应任务，不形成A14等待最终A13或A13等待最终A11的环。

问题清单继续使用`codex/apple-experience/DEFECTS.md`的AP编号；每例记录caseId、前置、步骤、预期、实际、状态、严重性、影响范围、证据、修复提交与独立复验。抓包方案与依据见[调研记录](codex/automation-expansion/CAPTURE_RESEARCH.md)，最近实机范围见[实机复核](codex/list-console/LIVE_RECHECK_2026-10-16.md)。
