# User Task Template

最小输入：

```text
任务目标：<你想完成的项目或功能>
```

可选增强输入：

```text
任务目标：<你想完成的项目或功能>

偏好：
- 技术栈：
- 美术方向：
- 是否允许 AI Code With ImageGen：
- 图片数量或成本限制：
- 验收标准：
```

如果你想跳过计划阶段提问：

```text
任务目标：<你想完成的项目或功能>

按默认方案执行。
```

默认情况下，planning-agent 会先通过主控 agent 向你提出 3-7 个澄清问题，再制定正式计划。

根目录的 `AGENTS.md` 已经保存自动启动规则，因此不需要再输入：

```text
请按 .agents/entrypoint.md 执行。
```
