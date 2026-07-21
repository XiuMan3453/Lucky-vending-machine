# Multi-Agent Protocol

## 基本原则

1. 主控 agent 是唯一的协调入口，其他 agent 只接受边界清晰的任务。
2. 每个 agent 都要声明自己的写入范围，不改未分配文件。
3. 计划、美术风格、图片生成、代码实现、结果测试必须分离，避免职责混杂。
4. 任何 agent 发现需求冲突、文件冲突或验收标准不清楚，都先汇报给主控。
5. 交付必须包含：完成内容、改动文件、验证结果、剩余风险。

## 标准交接

每次派工使用 [.agents/templates/handoff.md](templates/handoff.md)。主控 agent 至少提供：

- 目标
- 输入上下文
- 期望输出
- 可写文件范围
- 不可做事项
- 验证方式

## 协作顺序

1. `control-agent` 接收用户目标。
2. `planning-agent` 输出计划、任务拆分、验收标准。
3. `control-agent` 审阅计划并创建并行任务。
4. `coding-agent` 实现工程部分。
5. `art-direction-agent` 输出风格设定、资产清单、图片提示词。
6. `imagegen-agent` 只基于已确认的提示词生成图片资源。
7. `testing-agent` 验证代码、资源和验收标准。
8. `control-agent` 整合代码、资源、测试结论和记录。

## 文件冲突规则

- 两个 agent 不应同时拥有同一个文件。
- 如果必须修改同一文件，先由主控 agent 规定修改顺序。
- agent 不允许还原自己不理解的改动。
- 合并时主控 agent 保留可验证的行为，而不是按 agent 优先级盲目覆盖。

## 图片资产规则

- `art-direction-agent` 负责决定风格和提示词。
- `imagegen-agent` 负责调用 AI Code With ImageGen skill。
- 默认输出目录为 `assets/generated/imagegen/`。
- 每次生成后必须记录模型、最终提示词、尺寸、质 量、保存路径和可见问题。
- API key 只能来自环境变量或 macOS Keychain，不能写入仓库或聊天内容。

## 测试结果规则

- `testing-agent` 负责独立验收结果，不负责默认修复。
- 测试结论必须区分 `pass`、`fail`、`partial` 和 `blocked`。
- 没有运行的检查必须标记为未验证，不能写成通过。
- 如果测试失败，主控 agent 决定是否把问题退回给 `coding-agent`、`art-direction-agent` 或 `imagegen-agent`。
- 最终交付前，主控 agent 必须汇总 testing-agent 的测试结论。
