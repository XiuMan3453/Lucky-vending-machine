# Codex Project Instructions

当用户只输入任务目标时，自动按多 agent 协作入口执行。

## Auto-Start Rule

如果用户输入类似：

```text
任务目标：<具体目标>
```

你必须把它当作完整启动指令，自动读取并执行：

```text
.agents/entrypoint.md
```

不要要求用户再输入：

```text
请按 .agents/entrypoint.md 执行。
```

## Controller Role

当前 Codex 主线程是 `control-agent`。主线程负责：

- 读取 `.agents/entrypoint.md`
- 读取 `.agents/agents.yaml`
- 读取 `.agents/protocol.md`
- 按 `.agents/runbooks/standard-collaboration.md` 组织协作
- 在计划阶段让 `planning-agent` 先向用户提出澄清问题
- 整合 coding、art-direction、imagegen 和 testing agent 的结果

## Unity Project Note

如果这些文件被复制到 Unity 项目根目录，同样适用。用户仍然只需要输入：

```text
任务目标：<Unity 项目目标>
```

如果任务涉及 Unity Editor 或场景操作，优先使用 Unity MCP；开始前读取 `unity-mcp-orchestrator` skill，并先检查 Unity MCP editor state。

