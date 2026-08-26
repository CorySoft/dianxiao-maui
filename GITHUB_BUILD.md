# GitHub Actions 自动构建 APK

本项目已包含两个 GitHub Actions 工作流，推送到 GitHub 即可自动编译出 APK。

## 快速开始

### 1. 推送到 GitHub
```bash
cd /mnt/data/work/projects/dianxiao-maui
git init
git add .
git commit -m "Initial commit: MAUI auto-dialer"
git branch -M main
git remote add origin https://github.com/YOUR_USERNAME/dianxiao-maui.git
git push -u origin main
```

### 2. 自动触发构建
- 推送到 `main`/`master` 分支 → 自动构建
- 手动触发：GitHub 仓库 → Actions → Build MAUI Android APK → Run workflow
- 打 Tag 发布：`git tag v1.0.0 && git push origin v1.0.0` → 自动创建 Release 并附带 APK

### 3. 下载 APK
- **Actions 页面**：点击构建任务 → Artifacts → 下载 `dianxiao-maui-apk.zip`
- **Releases 页面**：Tag 触发的构建会自动生成 Release，直接下载 `.apk`

---

## 两个工作流对比

| 工作流文件 | 方式 | 优点 | 缺点 |
|-----------|------|------|------|
| `build.yml` | 安装 MAUI workload | 官方标准方式，功能最全 | 需下载 ~2GB workload，耗时 3-5 分钟 |
| `build-no-workload.yml` | 仅 NuGet 还原 | **无需 workload，构建快 2-3 倍** | 依赖项目已显式引用所有 NuGet 包（本项目已配置好） |

**推荐使用 `build-no-workload.yml`**（本项目 csproj 已直接引用 `Microsoft.Maui.Controls` 等包，无需 workload）。

---

## 关键配置说明

### .NET 版本
```yaml
dotnet-version: '10.0.x'
dotnet-quality: 'preview'  # .NET 10 目前为预览版
```

### Android SDK
```yaml
api-level: 36       # 对应 net10.0-android36.0
ndk-version: r27c   # 可选，原生库需要时用到
build-tools: 36.0.0
```

### 构建命令
```bash
dotnet build DianxiaoMaui.csproj \
  -c Release \
  -f net10.0-android36.0 \
  -p:AndroidSdkDirectory=$ANDROID_HOME
```
- `-f net10.0-android36.0`：显式指定目标框架（单项目多 TFM 时必需）
- `$ANDROID_HOME`：`android-actions/setup-android` 自动设置

---

## 常见问题

### Q: 构建失败 "Could not find android.jar for API level 36"
**A**: 确保 `api-level: 36` 已在 `setup-android` 中指定，且 csproj 用 `net10.0-android36.0`。

### Q: 签名 APK
在仓库 Settings → Secrets 添加：
- `ANDROID_KEYSTORE_BASE64`：keystore 文件 base64 编码
- `ANDROID_KEY_ALIAS`：别名
- `ANDROID_KEY_PASSWORD`：密钥密码
- `ANDROID_STORE_PASSWORD`：库密码

然后在构建步骤添加：
```yaml
-p:AndroidSigningKeyStore=$(KEYSTORE_PATH) \
-p:AndroidSigningKeyAlias=${{ secrets.ANDROID_KEY_ALIAS }} \
-p:AndroidSigningKeyPass=${{ secrets.ANDROID_KEY_PASSWORD }} \
-p:AndroidSigningStorePass=${{ secrets.ANDROID_STORE_PASSWORD }}
```

### Q: 缓存加速
`setup-android` 已内置缓存 (`cache: true`)，二次构建会复用 SDK。

### Q: ARM64 设备测试
GitHub Actions 跑的是 x86_64 模拟器/真机，产出的 APK 为 `arm64-v8a` + `x86_64`（默认多架构），可直接安装到 ARM64 手机。

---

## 文件结构
```
.github/
└── workflows/
    ├── build.yml                 # 标准 workload 方式
    └── build-no-workload.yml     # 推荐：仅 NuGet，更快
```

直接推送即可使用，无需额外配置。