# AP-043 官方纯文本观察对照

## 目标与已知事实

上一轮有实质进展：AP042实际组件红绿及主仓通过；新F18官方纯文本首读却为null，改变了下一步，不能按修复交付。请求false/true与原始返回已核，不是调用者遗漏include_text。没有可信helper日志接口，不搜索或逆向私有协议。

本阶段只区分“旧Desktop当前也不能被读取”和“同条件旧可读、新不可读”，不假定是工具或产品根因。不新增生产代码、不Build、不重跑旧指针套，不把SDK相同当实际runtime相同。

## 最小方案与取舍

- 使用当前Release完整46文件复制一次到fresh ignored probe-bin，共同44文件按路径/长度/SHA冻结；只在前一自有进程及原始流完全终态后，替换该目录的Desktop DLL/PDB。旧对为已保存E729/F923，新对为F18/D790；原Release和旧冻结件不改。
- 两次使用同一实际dotnet可执行文件、启动DLL路径/共同目录、1000×700配置原字节、同一官方Sky客户端；没有在进程内遥测Assembly.Location，不另称该属性已测。各自fresh profile避免沿用持久状态。配置关闭API、自动接入、Agent和同步，--demo不触真实设备。profile路径不同是必要隔离变量，须如实声明。
- 这是固定旧/新Desktop在共同当前依赖下的对照，不是恢复原版完整构建。不能据它证明整个历史环境相同，也不能证明两次之间桌面状态绝对不变。
- 通过精确自有PID/出生UTC ticks/可执行路径/DLL/profile/--demo检查后的Process.Modules，只读记录coreclr.dll与wpfgfx_cor3.dll实际模块路径、版本和对应磁盘SHA；缺失或枚举错误明确记录，不用安装目录补成已加载，更不把磁盘SHA说成内存映像验签。两轮实际模块记录一致才可排除这一运行时差异。
- 更简单的重读新版本不能区分回归，拒绝；两个应用目录增加AssemblyLocation变量；隐藏树、猜测修peer、外部UIA/Win32/SendInput、截图恢复或helper私有协议均拒绝。

## P0步骤与验收

1. 固定源与二进制摘要，准备同一probe-bin/变体副本/共同44清单；原配置E4E、两用户PNG及主仓四候选源保持。不核包、暂存或推送AP042候选。
2. 独立静审后只启动Old一次。官方Sky从新list_windows选择唯一返回对象，get_window后仅一次get_window_state(false,true)，保存实际完整state，不输入或激活。唯一性、锁屏、空树、超时或任何前置失败均停止。
3. Old为空即记录当前旧版也READ-BLOCKED，精确自有清理、流排空后结束本阶段；New严格NOT-RUN。不能从Old失败推新版本安全或AP042已完成。
4. 只有Old返回有效树、自有runtime实际记录完整、进程及流终态且共同文件保护通过，才允许New一次，同一初始纯文本读取。New为空立即停止；旧可读/新为空只提高回归相关性，尚不单凭它给根因结论。
5. 若New初读有效且实际runtime与Old记录一致，才允许额外的正式Sky验收：根据最新状态，设备→应用→设备每次一个导航动作并立即false/true重观测；不得复用索引。核正文与禁用状态，不做安装、删除、权限、账号或其它业务动作。失败即停，不自动重试。
6. 生命周期每child300秒、自有终止5秒、各原始流排空10秒。停止使用Sky输入后，可核精确自有身份再终止演示进程；必须记录externalKill/实际exit，不称自然Close。脚本或metadata采集失败也先保留原件，清理仅自有进程。
7. 独立实读原件后决定AP042下一步。不能把组件绿、旧版null、文件SHA或目录同一等价为全GUI通过；完整双实例/当前应用GUI/最终30分钟、A08-A16不缩减。

## 当前状态

受控对照已执行完毕，两个自有Demo及原始流均已终态；没有操作真实模拟器。准备稿经独审后才执行，未新增生产改动或Build。

| 阶段 | 实际观察 | 生命周期与边界 |
|---|---|---|
| Old | 唯一窗口76743122，一次false/true返回48个总节点0..47，止于空Tab，无设备正文 | PID62792；精确身份核后externalKill，session86146父exit0/child-1，双raw均0B且完整，无timeout/runnerKill/清理错 |
| New初读 | 唯一窗口80675104，一次false/true返回200个总节点0..199；47号为“工作区正文”Pane，包含DeviceStatusFilter、SelectAllDevices和设备表行；不是200个正文节点 | PID66348，出生UTC 07:15:05.8852427；实际coreclr/wpfgfx两模块的路径、版本、字节数、内存大小及对应磁盘SHA与Old完全相同 |
| New导航 | 按最新初读尝试应用入口21，官方工具isError=true：`coordinate input geometry is unavailable`；点击前已清除旧state，未获得后置state | 立即停止，不重试、激活、截图恢复或外部输入绕过；是否发生切页未知，应用正文、返回设备和正常Close均未验收 |
| New清理 | 仅精确自有PID/出生UTC ticks/路径/DLL/profile/--demo匹配后外部终止 | 清理exit0；同session11102父exit0/child-1，terminal=true，双raw0B且完整，无timeout/runnerKill/error/cleanupErrors；不称自然Close |

原件位于本机ignored `artifacts/sky-ab-a01`。manifest SHA `95E7794788B5CC4928740DFED3B75EDDF62592654E16A3BE4D32587C3E07D671`；Old observation `400DB75AC2C587804902694F60602E411B6511F3A3CD601775B3DAE97C886F75`；New observation `AED9FBCAD1D873D1D07109EF2B2BCDBF541F4A25C03BF72A4251DABB79D18979`；原失败tool回包 `F6F35585DAA6AB122625E38D7766EFF0AF4CEB817F08DE1FA1F27EC1B868AA5E`；New terminal `4C4E2F3EF8738271AE802AA908359659B4CA7A3B2A52250E927FF1C056B83579`。

独立实读确认共同44、原Release46、七项源/配置/用户PNG保护全同；guards SHA `4D3367CBB647BADF7BD8C5206AA8E6D42409C03E5DAE615805DE2B57E05BE0BE`。实际模块磁盘SHA为coreclr `18EED8C40D05FC455BE95D12CC77EACF379265063B15CA84BD7A11CF29078A22`、wpfgfx `AD1C9DBBC2FCC2B274145727E8F04D5ADF49A0C9AE4D1A3C33EC969FCD5B4BF1`，不将其称为进程内存验签。

结论只支持“同共同依赖与已观测运行时下，新构建初始设备正文可被官方Sky读取”。先前F18整树null仍保留，未证明其根因；此次点击错误也不能归因于新peer或纯环境。同一窗口的应用导航门仍未通过，AP042整体验收尚未获批准。独审允许考虑仅正文可达的私有开发分支源码交付，仍以文档/核包/index审核与实际Git为准，不外推官方控制已恢复。下一安全工作转A08脚本包边界，不重复本轮探测凑绿；全GUI、双实例同步、应用GUI安装卸载与最终30分钟等必需项保持开放。

## AP045显式激活诊断

在已交付9117fd2/Desktop D123上，按公开Sky文档与独审计划进行一次新的受控诊断：fresh 1000×700 Demo、同配置字段、关闭API/自动接入/Agent/同步，不触真实设备。新构建、路径、依赖及观察时间与原AP043不同，不是历史因果A/B。公开guidance本就支持false/true之后的索引点击，并称激活可选，因此未预设截图或激活是硬前提。

唯一返回窗口111871806，显式activate工具成功；随后false/true读到200个总节点和真实设备正文，应用入口21唯一且未标禁用。activation原envelope SHA `5910ED0D4429F4C2F73C8BD407E1BE74660E53E43776E3068EFDB47FAFCE6C71`，state `8F42BC4E57F65AA92C7EB0370EC376F1291833F30B1DFDF87BFFEA748D4C0D3F`。这只证明工具调用返回成功，不另称已独立核验OS前台状态。

经独审后只尝试一次最新21入口，先清空旧state、拟立即false/true重观测；click再次返回isError=true、`coordinate input geometry is unavailable`，tool原件 SHA `F6F35585DAA6AB122625E38D7766EFF0AF4CEB817F08DE1FA1F27EC1B868AA5E`。没有后置state或apps-after-click.json，切页效果未知；不能说未导航、零输入或无副作用。立即停，不截图、键盘、坐标、外部UIA、重试或生产猜测修补。

精确PID36520/出生UTC 08:01:47.2727815Z/可执行文件/DLL/profile/--demo匹配后外部终止，owned-cleanup SHA `42927A3A14B4E9AFBFBDD39E27F8DAA0782AEA33F11E0C58A264D47B9D470D94`。同session66646父exit0/child-1、无timeout/error/cleanupErrors，双raw0B且完整；terminal SHA `5D83384B199D84BD795ED02F4224F89CBAFE2E4D0AB73DB302AD7702785255DF`。不称自然Close，原件在ignored `artifacts/activated-observation/New`。

独审结论：显式激活成功及正文可读不足以使本次索引输入可用；不证明产品、工具或环境根因，不倒填原null/几何错误已解决。停止该GUI输入分支，等待受支持诊断能力或外部条件变化，不自动反复试验。当前真实GUI、应用导航/安装卸载及原完整验收仍开放；继续其它安全实现与验证，不把分支受阻当整个目标完成。
