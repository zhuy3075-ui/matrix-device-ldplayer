# 应用页的名称与连接身份

日期：2026-10-16。阶段A07a，基线`1acbf23`。完整目标仍在进行，本阶段不替代后续页面、真实设备或最终30分钟验收。

## 事实与目标

当前应用页使用独立模拟器勾选和按串号/包名的应用行。基线`ApplicationTarget`记录包含名称，而`RequireSame`实际只核验在线、串号、连接代次和transport；应用页的Contains、SetEquals和结果发布还使用完整记录相等。仅metadata改名时旧应用行保留，这些判断可能拒绝仍为同一连接的操作。该候选随后经原生产代码受控复现，见下方首次基线结果。

目标：同一连接仅改名不使已有应用选择和任务失效，列表显示名称同步更新；确认与在途结果仍遵守原设备身份和明确选择，不允许通过改名混入其它设备。保留应用页独立目标，不继承群控/当前/主控授权。

## 范围与取舍

推荐先复现，再在当前应用页明确区分显示名称与连接身份；同步既有应用行的名称，不删除行或勾选来规避问题。更简单的只忽略Name比较可能仍显示旧名，不能完整解决体验问题。改公共record默认相等影响所有消费者和语义，不作为首选；强制改名后重读或清选择会丢失合法原意图，也不选。

本阶段不修自动读取pending、小屏固定双栏、原生保存或其他页面；不改变Core的代次计算、API、授权、共享ADB、安装语义或确认策略。若发现必须改变这些边界，独立登记，不顺手扩大修复。

## 验收

- P0：实际当前应用批次UI读列表，metadata仓改名并完整VM同步，证明串号/代次/transport不变；旧应用行选择保留、显示新名称，原行启动和停止命令按预期到达正确Demo后端。
- P0：确认期间仅名称变化可继续原本相同连接和包名的任务；确认取消或明确选择变化不得写入设备。
- P0：在途读取期间仅名称变化，合法结果可发布且采用最新显示名称；身份变化结果不可归给新连接。
- P0：串号、代次、transport、离线、运行时或选择变化的安全门禁保持，错误不静默，任务排空后不误报成功。
- P1：当前/主控/群控勾选不受应用选择或metadata修复影响；原应用流程和原六套正式夹具不弱化。

## 顺序与证据

1. 三个新独立worktree `apple-app-identity`、`-tests`、`-verifier`均基于1acbf23。
2. 独立测试者设计新自有一致Demo Backend/Catalog/Runtime/WPF窗口用例，先运行原生产代码记录红；只使用现有smoke接线，不再新增CLI模式或依赖原生文件框。
3. 确认根因后最小生产修复，非实现者复跑正式测试与当前UI；不将逻辑WPF命令触发称为物理鼠标或Android实测。
4. 主仓代表复验、README/本报告/源码清单、独立交付检查；一个完整已验证缺陷单独提交并push原私有功能分支，不合并发布。

主要风险是错误地仅按串号匹配、确认后丢掉代次/transport核验、在途结果发布到新身份、名称刷新导致行/勾选丢失或假成功。原失败和每次审查均保留，最多三轮产品修复。不触碰原0、用户实例或真实配置。

## 当前状态

三个合法改名路径已在原生产代码实际复现产品红，七个身份/选择保护反例通过。生产r1仅两文件，独立f02限域离线验收通过；四份冻结源已机械导入主仓，主仓正式150项通过，但完整GUI复验在新增专项执行前的旧鼠标前置失败。下方保留f01与root-r02的失败，不倒填通过；非实现者已批准本阶段应用改名修复限域交付，未批准主仓完整回归或全目标。不重复执行A06d前置失败用例，原生保存仍另项未验。

## 首次基线结果

正式夹具仅新增`ApplicationIdentitySmoke.cs`与Main的单行前置调用，旧六套及原ApplicationManagementSmoke不变。r0的确认同步前置独立检查防止异常被生产错误呈现吞掉；r0a另记录并拒绝所有非本专项允许的写入口，不能只观察启停/卸载就宣称零写。改名后、启停前的真实DataGrid单元格名称也在聚合断言中。原r0保留，未先执行过它。

r0a新夹具36858字节，SHA256 `03FD1C95A2C073E8F50E536F9311E091227C5865AC792BA315BA38C92723A279`；Main46928字节，SHA256 `70055C6D31C65D5ACD5105D154EA6E3D110A06E8D08F4276D42BAA6110E5F54D`。root与独立verifier均完整读审，按冻结字节机械导入。未新增App CLI模式、测试许可或真实服务。

独立baseline-b01首次Build exit0，73.223秒，正式150/150用时51.54秒，实际0警告0错误；首次1000窗口smoke exit1，3.502秒。runner exit0仅说明预期退出被观察到，不代表产品通过。十例为3FAIL、7PASS、10清理完成：

| 用例 | 实际结果 |
|---|---|
| 完成后A改为B同名 | 串号/g1/t11、原行引用/选择均未变；绑定名称仍旧，真实菜单启停均因“应用行不属于本次目标”拒绝，无写 |
| 在途List仅改名 | 原后端List payload与TCS已接纳，完整metadata同步后身份未变，后续校验错误拒绝；Start第二阶段NOT-RUN |
| 确认内仅改名 | 确认同步前置实际成功，但完整record集合比较错误拒绝，零卸载 |
| 确认内选择/代次/transport/离线变化 | 四例正确拒绝，零写，反馈非成功 |
| 在途List代次/transport/离线变化 | 三例拒绝旧结果，后续真实有效读取恢复，无写 |

逐例原JSON位于verifier树`artifacts/i/b01/artifacts/ai/ccbda4c1/application-identity.json`，SHA256 `52EEA41654546D3166D67B6F66ACCFE82250E55071D87C3DE20AA83BCC37DBED`；实际进程/保全`artifacts/app-identity-verifier/baseline-b01/acceptance.json`，SHA256 `6CAD86E6D0071AAC1A500B42EEAD7AC28FD6FC8920956599104B1FC3AB69E557`。root实读10例、红路径的身份/单元格/原命令对象/错误反馈/调用/原Task清理及实际Build原字节输出。两child无timeout/kill，均terminal、ownedAfter0；127源前后同、旧七份raw及Git blob同、实际配置E4E33不变。

新专项前置失败后旧六套、原应用smoke及12页检查均NOT-RUN，不能用正式150通过代替这些路径。本阶段逻辑WPF命令和Demo确认回调不等于物理输入、原生模态或Android实机；后者继续属于完整目标未验范围。

## 首轮修复与独立执行

生产r1仅`ApplicationsView.cs`、`ApplicationRows.cs`，20增/10删：统一按Serial/Generation/Transport比较，保留原在线、Runtime、保护及活动校验；旧行只更新独立展示名称，冻结Target与勾选不变；计划采用当前展示名，结果先接纳同身份再替换相同行并使用最新名称。公共record、Core、旧七套和新10例断言均未改。

两生产源SHA256为`9941BFA26383A70DF3007F9970F9107FF7D4D7EF26D8E6AFAE5BD523EB0EDFB2`、`F57BC529EB9F227E3739956814F198E95369E5EB11B5AAFA6D8BFB0262232F6E`；补丁`3C54BD5B616A9BB189BAE1B96A80B1F1A7B8B12827574A328BE7442D8E835C73`。root与非实现者静审后按冻结字节导入。

首次final-f01的Build exit0/63.562秒，150/150用时51.32秒、0警告错误。新应用10/10、114检查、10清理全部通过：改名后即真实单元格新名且原行/勾选保持；A启停各1；在途List与Start两阶段都过、无额外读取或错目标写；确认内同身份改名可继续唯一A卸载；七项身份/选择保护及三项读取恢复仍通过。新JSON位于verifier树`artifacts/i/f01/artifacts/ai/9191be07/application-identity.json`，SHA256 `3A1AC1B892211214802D7817326F4B310AA0179CF7C1B40830275FEC17B5F82A`。root直接实读逐例状态、实际写调用、任务与清理，独立verifier另读全分量。

完整smoke仍为FAIL/exit1，39.022秒：旧重连19/19、缓存18/18通过；旧截屏9PASS/1FAIL、10清理。失败在coordinate-route的原OS/WPF坐标命中前置，尚未RaiseEvent：zoom0.1、系统请求及实际均(911,573)，但WPF局部(-8310,-5280)、Shell(-248,-101)、owned=false，IsActive=true；colors0不当成功。原JSON`artifacts/i/f01/artifacts/sl/5792cec9/screenshot-layout.json`保留。不能据此归因用户、认定新夹具泄露输入源或宣布旧截屏产品回归；需独立只读诊断，不能靠盲重跑、改原断言或抢前台取绿。

该轮后续节点/自动首载/发布隔离、旧应用smoke与12页均NOT-RUN。进程证据SHA256 `0676C12DF6BE15B2D5124561FC3FA85CCF9025D26166371A61209099FBEABBFA`；session89496已实际终结，无timeout/kill/残留，127源/旧七套/配置均未变。当时只获新应用专项限域绿证据，未批准完整交付；产品r1与首次失败保持冻结。

## 受控新轮f02与测试隔离

独立静审未能从原记录确定WPF鼠标坐标异常的根因。新夹具实际打开过自有菜单，但不能据此证明泄露或排除影响；不加入抢焦点、鼠标同步、释放捕获或反射缓存的生产修补。推荐将新增的唯一smoke调用从最前面移到原应用管理验收之前，恢复原六套的执行顺序。更简单的原样盲重跑不能提供隔离价值，改旧坐标断言或禁用新夹具则会损失验证，所以不选。

测试r0b只移动该行，Main SHA256 `5864CBF91416F90CEC921C03CD707965DE636BA023F3EAEC31FC5EAFC71D94C0`。生产两源、新夹具03FD、原七套、所有旧调用顺序/断言/等待不变。双静审后以fresh profile运行f02；这是有明确测试隔离变化的新轮，不把后续通过写成首次通过或已修鼠标根因。

f02实际Build exit0/64.704秒，正式150/150用时51.58秒、0警告错误；完整1000×700 smoke exit0/106.981秒。新应用10/10、114检查、10清理；原重连19、缓存18、截屏10、节点11、自动首载16、发布隔离49全通过并清理。原应用管理smoke实际到达，截图已目视；12页均有idle记录，脚本完成且error为空。外层colors=0、nodes=0如实保留，不能冒称该外层已直接取色或读节点成功。旧节点观察器记录11条被原catch处理的Visual诊断异常，不隐藏成stderr为空，不扩大本次生产修复。

新专项报告`artifacts/i/f02/artifacts/ai/b1f27dc1/application-identity.json`，SHA256 `934610777CFB47E5E605D22155359FF739340543335A2BAF473E661FE5C008CE`。独立verifier逐例审读实际名称、命令、调用、确认、身份拒绝和任务清理后批准本阶段离线范围。两child无timeout/kill/清理错误，127源前后同、旧七套固定、实际配置E4E33未变，无残留进程。

## 主仓导入与预检

主仓机械导入Main r0b、新夹具03FD、ApplicationsView 9941、ApplicationRows F57四源，与冻结版本逐字节同；未导入A06d原生模式或更改其保存逻辑。

r01在启动Build/GUI之前拒绝：调用参数错误地使用verifier中未修改ApplicationBatch的CRLF raw EDDA摘要，主仓实际LF raw为`CB94788757997E845760D59DC539EE5C28D62509285F60FF89516FCE42EEE055`。两树归一化文本相同，主仓canonical blob与HEAD均为`41363675c28f827fda90e79b479aaa91c6e87d63`，该文件无diff。原预检记录保留，不当产品红；独立复核后只纠正调用参数，以fresh r02复验，runner与断言未改。执行预算为Build/Smoke各300秒，另有有界自有进程清理，不是合计300秒。

root-r02实际Build exit0/75.095秒，正式150/150用时54.18秒、0警告错误；DLL SHA256 `493B6C51E073CD05A3E3B5C17CE4626368FCC35CECAD98FF2C0D3667FAAF7204`。完整GUI smoke exit1/34.718秒，旧重连19与完成缓存18通过并清理；旧截屏9PASS/1FAIL、10清理。失败仍是coordinate-route原前置，未触发合成取色事件：系统请求(911,573)，实际OS(1105,575)，WPF Shell(-248,-101)、局部(-8310,-5280)、owned=false、IsActive=true。与f01的OS实际值不同，不擅自归因同一个根因或操作者。

该轮在Main第346行失败，新应用专项调用在第381行，尚未执行，故它不是该轮失败的必要触发；这也不能排除其它WPF状态或输入环境因素。主仓新增10例、后续四套、原应用smoke、完整12页均NOT-RUN，不能借用独立结果冒充主仓通过。原`artifacts/i/r02/artifacts/sl/3a65d6bc/screenshot-layout.json` SHA256 `AD6CE1D3F93E159B539AD010C98F88EA2C735864661F0789C289E89EEF742670`，进程验收`artifacts/app-identity-root/final-r02/acceptance.json` SHA256 `0523E14F31E4142450C00B4FB5AC501E7CCF294B739ACA4A84920AD4345F39F8`。raw stderr严格936解码，未用替换字符掩盖错误；全部child已终结，无timeout/kill/清理错误、127源前后同、旧七套固定、配置E4E33未变、ownedAfter0。

该前置缺口单列AP-033，保持开放，不与AP-028的Catalog历史边界混同。当前未再次执行完整GUI，也未更改旧断言或生产输入逻辑以取绿。

额外独立L1审查建议下一轮只在原定位前、定位后和现有循环结束记录有界primitive快照：前后OS位置、同一OS点经PointFromScreen的直接转换/命中、Mouse.GetPosition/PointToScreen、PrimaryDevice.ActiveSource与目标PresentationSource、捕获/悬停/焦点及自有窗口生命周期。现数值与WPF屏幕零点相容只是推断；[官方MouseDevice源码](https://source.dot.net/PresentationCore/System/Windows/Input/MouseDevice.cs.html)存在该路径，不证明当前安装runtime实际走过它。[PointFromScreen定义](https://learn.microsoft.com/en-us/dotnet/api/system.windows.media.visual.pointfromscreen?view=windowsdesktop-10.0)可作旁路对照，不改变原事件/等待/断言。不增加Activate、Synchronize、反射或重试；本阶段尚未实施这些诊断。

## 独立交付判定

非实现者再次实读root-r02，并逐项比较root与独立f02全部127个src/tools：路径集合完全相同，52份raw同、75份仅LF/CRLF，规范化文本全同；新增四源raw精确相同。因此批准将A07作为已有完整独立实证的限域修复提交原私有功能分支，不要求以无意义盲重跑覆盖本次前置失败。此判定不是root完整GUI通过或AP-033根因修复。

交付包含两生产源、新10例和单Main接线、README/状态/正式报告与生成源码清单；A06d报告保留其未验事实，不引入其未验测试入口。原始证据、APK、SDK、本机授权与配置、两用户PNG不提交，历史已tracked验收材料仍按AP-021后续分类，不冒称本次做了历史隐私净化。源码包逐项路径/字节核验和最终暂存独审后提交，版本与推送状态以实际Git为准，不合并发布。
