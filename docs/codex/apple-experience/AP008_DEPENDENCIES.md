# AP008 自有测试包候选依赖取证

日期：2026-10-15。仅用于A07后续准备，不是已通过构建、签名或Android安装的报告。原始工件在本机忽略目录`artifacts/dependency-inspect`，不随仓库或源码包分发。未读写CODEBRAIN。

## 已知事实

没有完整Android SDK和本轮自有APK。已有Temurin21 JDK启动器签名有效，但这不证明整个工具链来源或兼容性。独立只读调查取得以下固定工件，实际文件SHA1分别与发布地址的`.sha1`相符，另计算SHA256；未执行下载的EXE/JAR。

| 文件/字节数 | SHA256 |
|---|---|
| android-4.1.1.4.jar / 12,947,757 | 84072541CBB711EFF89F7277100FF854929A446DBA7CEB1B195C340E0B4FD3CB |
| aapt2-8.6.1-11315950-windows.jar / 1,947,069 | D7911DE3AFA8D5DE1F1213F5244DE7144093442D4FC20C8EB75D188EBE132EA5 |
| r8-8.6.27.jar / 17,059,952 | 9007144728D47B5F11338C9292D5B74899B6579AD9DDC6942D419E3600C3D6D7 |
| apksig-8.6.1.jar / 506,969 | C070ED1394629D74641AA0906F60B2FFA1EE77E6366A1F93437F59717B1AEB89 |

## 发布与许可证据

- Framework来自[Maven Central固定工件](https://repo.maven.apache.org/maven2/com/google/android/android/4.1.1.4/android-4.1.1.4.jar)，[发布POM](https://repo.maven.apache.org/maven2/com/google/android/android/4.1.1.4/android-4.1.1.4.pom)与内嵌POM一致，声明Apache2及源码重编译分发区别于SDK EULA二进制，注明API16/android-4.1.1_r4。实际有1698类、resources.arsc和Android资源包ID1；没有LICENSE/NOTICE/JAR签名。坐标不证明Google发布身份或可复现构建。
- AAPT2来自[Google Maven固定Windows工件](https://dl.google.com/dl/android/maven2/com/android/tools/build/aapt2/8.6.1-11315950/aapt2-8.6.1-11315950-windows.jar)，[发布POM](https://dl.google.com/dl/android/maven2/com/android/tools/build/aapt2/8.6.1-11315950/aapt2-8.6.1-11315950.pom)声明Apache2。归档包含Windows x64 PE、100,319字节NOTICE及Apache/BSD/MIT/Unicode等第三方文本；没有把EXE释放到磁盘或执行。[官方独立分发说明](https://developer.android.com/tools/aapt2)。
- R8/D8来自[Google Maven固定工件](https://dl.google.com/dl/android/maven2/com/android/tools/r8/8.6.27/r8-8.6.27.jar)，[发布POM](https://dl.google.com/dl/android/maven2/com/android/tools/r8/8.6.27/r8-8.6.27.pom)及31,313字节LICENSE列多种第三方许可，不概括为全部BSD。实际存在D8/R8类，D8 class major55；Manifest指定的SwissArmyKnife类缺失，不能假定`java -jar`可运行。[官方入口](https://r8.googlesource.com/r8/)使用`java -cp ... com.android.tools.r8.D8`。
- apksig来自[Google Maven固定工件](https://dl.google.com/dl/android/maven2/com/android/tools/build/apksig/8.6.1/apksig-8.6.1.jar)，[发布POM](https://dl.google.com/dl/android/maven2/com/android/tools/build/apksig/8.6.1/apksig-8.6.1.pom)及11,873字节LICENSE声明Apache2。签名/验证类major52及必要public方法存在，但没有ApkSignerTool CLI，不冒充已安装apksigner。[官方库与CLI区分](https://android.googlesource.com/platform/tools/apksig/+/refs/heads/main/README.md)。

## 下一阶段与限制

候选链是现有JDK21编译自有最小源码、D8、AAPT2/ZIP打包、自有apksig包装器进行v2签名和验证。Framework不含java.lang.Object/String，`javac --release 8`和D8 boot-library诊断仍需实测。签名后不得再次修改包。

[Android14最小target约束](https://developer.android.com/about/versions/14/behavior-changes-all#minimum-target-api-level)及[Android11 v2签名/资源要求](https://developer.android.com/about/versions/11/behavior-changes-11#apk-signature-scheme-v2-now-required)意味着拟target34不能仅使用jarsigner v1签名，resources.arsc需未压缩且4字节对齐。实际安装/精确版本/签名/启停/数据/卸载效果仍全部未验。

没有运行sdkmanager、接受SDK条款、修改JAVA_HOME/PATH或用户环境，没有生成密钥、编译APK、操作任何模拟器或ADB。这里记录分发文本与归档事实，不作法律意见。AP008继续是A07依赖，不标记已解决，也不使用用户抖音数据替代自有测试包。
