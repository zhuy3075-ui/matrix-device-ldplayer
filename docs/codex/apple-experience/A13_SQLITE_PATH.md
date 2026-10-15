# A13b：数据库长路径修复与验收

日期：2026-10-16。问题AP-035。基线`1b5bac0cdb7d3d6976d2dbb9607f53a844da8e5a`。状态：独立及主仓离线修复验收通过，不代表完整GUI或目标通过。

## 目标与边界

当前程序在Windows普通数据库路径252字符、journal260字符时初始化抛SQLite14，影响正常运行与完整测试。修复应让同一原数据位置正确创建、读写、重开与迁移，不搬移或清空数据，不改SQLite事务/journal策略，也不要求用户改系统长路径设置。

仅产品`CatalogRepository.cs`与独立正式回归新增/接线属于代码范围。原有150项、全部桌面断言、用户配置、设备授权、ADB服务、真实应用与其他缺陷不改。UNC行为只有可以实际验证的层级才报告，不把本机磁盘用例写成网络共享验收。

## 事实、方案与失败模式

原始证据在本机`artifacts/p35a`：当前CLI空授权、无雷电路径/API初始化，db240/251字符成功，252/253字符均在BeginTransaction失败，journal分别260/261字符；原始流及退出码保留。`artifacts/p35b`只把调用的data目录设为Windows扩展路径，db逻辑长度252/300均成功，未修改产品。

推荐只在Windows规范SQLite原生路径为扩展路径，保留已经扩展的路径，正确区分本地盘与UNC；路径仍指向原文件。更简单的短测试目录只解决测试夹具，不修复正常用户长数据目录，因此不是本问题最终方案。拒绝禁用事务/journal、缩短/搬移数据库、改系统权限或全局注册表，避免丢失原行为与数据位置。

风险包括重复前缀、UNC规范错误、迁移备份后缀更长、已有库/未来schema拒绝路径泄漏句柄，以及非Windows回归。不能因一个新库创建成功就宣布迁移和保存均通过。

## 验收与顺序

1. 独立测试覆盖短路径与251/252/253/300边界、真实CRUD和重开、已有扩展路径、schema0旧库迁移与备份数据、未来schema拒绝及资源释放。生成长度按实际根目录计算，每个组件合法；只使用自有fresh目录。
2. 冻结新测试后，在原生产源码上取得失败；测试前置错误不算产品红，旧150项仍保留。实现仅改路径规范，独立审查冻结源后再导入独立验证树。
3. 同一测试与原断言在修复后通过，完整离线基线通过；使用真实CLI、默认空授权的普通长路径再次核验，不以手动传入扩展路径替代产品自动处理。
4. 主仓机械整合并复验，检查用户配置与源码范围，更新问题/报告/清单；独立交付审计通过后单独commit/push原私有功能分支。不合并发布、不清理旧worktree。

所有构建/子进程单次有界并记录原始输出、退出码、二进制摘要、源码前后摘要与清理结果。Build/GUI共享锁串行使用；不为此反复重跑已知AP-033完整GUI前置。AP-033、双实例同步、安装卸载和最终30分钟仍独立开放。

## 阶段记录

- 上一目标轮已完成四任务文档追加并推送`1b5bac0`，本轮不重复交付规划。
- 当前三隔离树`sqlite-path-fix`、`sqlite-path-tests`、`sqlite-path-verifier`均从该基线建立；实现者、测试作者和独立验证者职责分开。正式红/绿和交付结果随后按实际追加。

### 原代码正式红测

- 独立验证树只新增`CatalogPathTests.cs`及Program唯一注册，原生产与原150项逐字保留。冻结测试SHA为`B82594AB0A3933DFAA6B968ED853CB127FE8CF14964AE3145634CED2C02C21BC`，Program为`9B0A72B0B3D1DE49B72FD87E2172036868D6FF4FDC38FD64257C231B2F3DCDD7`。
- `tools/Build.ps1 -Verify`的b01实际PID66980，正常exit1、76.970秒，无超时或kill；构建0警告/0错误，测试152/158、51.73秒。原150全过，新短路径及251字符两项通过；252、253、300、扩展输入后普通重开、旧库迁移、未来schema拒绝六项均真实SQLite14。未来schema例尚未到预期IOException，corrupt后半未执行，不将其写成保护成功。
- 每例自有目录清理未抛，结束后该前缀目录剩余0。145项实际构建输入前后原字节相同、配置仍原SHA、自有进程0；原stdout22381字节和stderr271字节均保留。不同writer输出混合UTF8/CP936，原始二进制流为依据，不把替换字符的解码文本当无损原件。
- 唯一产品r1只向SQLite原生主库/备份路径传入Windows扩展路径，原.NET文件路径、事务、schema和数据位置不变；产品冻结SHA为`41C2BB51E01F671FF2758C3059C619AEA5C42CC5C073A387D49A3DD57267515F`。本机CLI扩展路径可行性与[Windows路径约定](https://learn.microsoft.com/en-us/windows/win32/fileio/maximum-file-path-limitation)、固定版本Microsoft.Data.Sqlite路径传递行为支持该方案；UNC只静审，未验证真实网络共享。

### 首轮修复验证的旧产物问题

f01实际PID69384、exit1、54.187秒，正式152/158、51.63秒，原六失败仍在；不计修复通过。复制文件保留的r1时间早于b01的bin/obj，构建只1.69秒；更关键的是实际Host portable PDB的CatalogRepository文档checksum仍为原生产`5591689D6CAE4EBD2438768D95CD0A51096BCCE495C5A7DB06CA4FD8145670FB`，运行的是旧编译输入。旧Host及Tests拷贝DLL同为`81F21D6B47BBE8129831CEEA36131CA0FE10EFBA0ED11EDA66BD9BB3056C8FE4`。源文件仍r1、测试未变、145输入和配置前后同，自有进程与临时目录均0。

保留f01全部原件；仅更新自有验证树该源mtime促使现有构建重新编译，原字节/SHA、产品补丁和测试不变。下一轮必须核PDB文档checksum与r1相等，不能只看源码SHA和构建exit0就认定测试到了修复。

### 独立有效绿测

f02 PID14412正常exit0、65.394秒，Build13.26秒、0警告/0错误，正式158/158、51.43秒；原150及新8的名称/顺序与b01相同。Host PDB文档checksum精确等于r1，Tests PDB的fixture与Program也等于冻结源；Host和Tests拷贝DLL同为`E3056270E4359E1C128DCD4266263843EB692235377F6109065200FE58EEB6A3`，已不再是旧产物。

新8例实际通过普通长路径CRUD/两次重开、扩展与普通输入读写同一库、v0非空迁移/原备份内容和完整性、future2重复拒绝与corrupt拒绝/原字节保持/独占再开。结束后自有临时目录和进程均0，145输入/配置前后不变；stdout13533字节（本轮严格UTF8解码成功）、stderr0，原始流保留。报告摘要`2C3B6AE9F53441C1D5E9B03B5A3EFAF1F9BA81C7D06DF2F5D997DADCAA0EAECD`。

主仓按冻结三源机械整合，另以主仓自身原144份raw/HEAD建立基线（不能沿用独立树的不同换行原字节）。UNC、非Windows、完整GUI和真实模拟器均未由本轮独立离线结果覆盖。

### 主仓复验与交付边界

- 主仓f01 PID15364正常exit0、77.338秒，Build24.63秒、0警告/0错误，158/158、51.76秒。PDB的CatalogRepository/新fixture/Program均精确等于冻结三源，实际Host/Tests/CLI/Desktop四处Host DLL同为`D191E79E5105236F346666A26C35CEBBC364021ACF76B41E33FD2804EFFD23E5`。Desktop DLL为`54BA17BD34E0161B41C3B1C5F3F8A61BE9F806CAA1307266BE44F497AE7D360D`。
- 真实CLI无授权串号、无雷电路径/无API，四个fresh普通输入db251/252/253/300字符，PID9532/31948/75980/7300，全部exit0、0.347/0.165/0.168/0.166秒；各库102400字节、子进程退出后独占再开成功，stdout各154字节/有效JSON/无目标，stderr全0。不是手工传入扩展前缀代替产品处理。
- 主仓145构建输入前后相同、原配置SHA保持，自有Build与CLI均正常终结，无超时/kill。Build摘要`B670690991F6BEDDF7CEEC6F8568C29096B28B98F9F28F121BA424709FFDCC52`，CLI摘要`AD598FE68D298D2C20A2C09F1CBEB1D74E9B5245DC3D9069E1872DB2E3803D36`；原始证据在`artifacts/sqlite-path-main`，不上传。
- 产品只改`src/MatrixDevice.Host/Persistence/CatalogRepository.cs`，正式测试为`tests/MatrixDevice.Tests/CatalogPathTests.cs`和Program唯一接线；同步README、SPEC、TASKS、PROGRESS、HANDOFF和DEFECTS。Windows追加8项；非Windows只注册通用短路径1项，未实际运行，不将其打印为平台通过。
- 本项独审/核包后独立commit/push原私有功能分支；原始证据、配置、SDK/二进制和两个用户PNG不提交。包保留既有tracked历史AP-021材料，不声称清洗历史。无合并或发布，后续仍须完成AP-033、真实双实例同步、当前应用页安装卸载及最终双实例30分钟。
