# Migrating This Multi-Agent Setup

这套多 agent 配置是项目级配置。迁移到其他项目时，把配置文件复制到目标项目根目录即可。

## 必须复制

```text
AGENTS.md
.agents/
```

复制后，目标项目中就可以只输入：

```text
任务目标：<你的目标>
```

主控 agent 会自动读取 `.agents/entrypoint.md` 并启动协作流程。

## 建议复制

如果你希望保留实验记录、文档模板和图片输出目录，也复制：

```text
docs/
experiments/
assets/generated/imagegen/
```

这些不是启动必需项，但有助于记录计划、美术 brief、测试报告和生成资源。

## 推荐命令

从当前仓库迁移到目标项目：

```bash
cp -R AGENTS.md .agents /path/to/target-project/
mkdir -p /path/to/target-project/docs/plans
mkdir -p /path/to/target-project/docs/art
mkdir -p /path/to/target-project/docs/testing
mkdir -p /path/to/target-project/assets/generated/imagegen
```

如果目标项目已有 `AGENTS.md`，不要直接覆盖。把本仓库 `AGENTS.md` 的 Auto-Start Rule 和 Controller Role 合并进去。

## 迁移后检查

在目标项目根目录运行：

```bash
test -f AGENTS.md
test -f .agents/entrypoint.md
test -f .agents/agents.yaml
test -f .agents/protocol.md
```

可选检查：

```bash
ruby -e "require 'yaml'; YAML.load_file('.agents/agents.yaml'); puts 'agents.yaml ok'"
```

## Unity 项目

迁移到 Unity 项目时，目标项目根目录应包含：

```text
Assets/
Packages/
ProjectSettings/
AGENTS.md
.agents/
```

如果要使用 Unity MCP，确保 `Packages/manifest.json` 里包含 MCP for Unity 包，例如：

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#v10.0.0"
```

然后用 Unity Editor 打开该项目，等待编译完成。之后在 Codex 的 Unity 项目 task 中输入：

```text
任务目标：<Unity 项目目标>
```

如果 Unity MCP 工具已连接，主控 agent 会优先检查 editor state，再进行场景、脚本、测试或截图操作。

## 迁移注意事项

- `.agents/entrypoint.md` 使用相对路径，适合直接复制。
- `imagegen-agent` 依赖本机的 `aicodewith-imagegen` skill，不要复制 API key。
- 如果目标机器没有 `aicodewith-imagegen` skill，需要先安装或改用别的图片生成流程。
- 每个项目可以按需要修改 `.agents/prompts/*.md`，但建议保留角色边界。

