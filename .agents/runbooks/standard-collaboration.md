# Standard Collaboration Runbook

这份流程给主控 agent 使用，用于组织 6 个 agent 协同完成一次项目任务。

## Phase 0: Intake

主控 agent 记录：

- 用户目标
- 期望交付物
- 时间和范围限制
- 是否需要图片资源
- 是否允许调用 AI Code With ImageGen

## Phase 1: Planning

派给 `planning-agent`：

```text
请基于以下目标先进入交互式需求澄清。不要直接写最终计划，不写代码，不生成图片。

目标：
<user-goal>

请先提出 3-7 个关键问题。每个问题都要包含：
- 为什么重要
- 推荐默认值
- 用户如何快速确认

问题应覆盖：产品目标、功能范围、技术偏好、美术方向、是否允许调用 AI Code With ImageGen、验收标准、时间或成本限制。
```

主控 agent 将问题转交给用户。用户回答后，再派给 `planning-agent`：

```text
用户已经回答了计划阶段问题。请基于目标、回答和默认值制定正式计划。

目标：
<user-goal>

用户回答：
<user-answers>

请包含：已确认假设、需求拆解、任务依赖、agent 分工、验收标准、风险。
```

主控 agent 审阅正式计划，并把任务切成 coding、美术、imagegen 和 testing 四类。

## Phase 2: Parallel Work

派给 `coding-agent`：

```text
你负责工程实现。只修改以下文件范围：
<owned-files>

计划摘要：
<plan-summary>

验收标准：
<acceptance-criteria>

完成后汇报改动文件、验证命令和风险。
```

派给 `art-direction-agent`：

```text
你负责美术方向和图片提示词。不要生成图片。

产品/功能目标：
<goal>

请输出美术风格、资产清单和每个资产的 ImageGen-ready prompt。
```

## Phase 3: Image Generation

当主控 agent 确认美术提示词后，派给 `imagegen-agent`：

```text
请使用 aicodewith-imagegen skill 生成以下资产。不要自行改变核心风格。

输出目录：
assets/generated/imagegen/

资产提示词：
<approved-prompts>

默认使用 gpt-image-2、2K、medium、n=1。完成后汇报保存路径和检查结论。
```

## Phase 4: Testing

当 coding、art-direction 和 imagegen 任务完成后，派给 `testing-agent`：

```text
你负责独立验证结果。默认不修改生产代码或生成图片。

用户目标：
<user-goal>

正式计划和验收标准：
<plan-and-acceptance-criteria>

代码改动摘要：
<coding-result>

生成资源摘要：
<imagegen-result>

请运行可用测试和构建命令，检查资源路径和用户可见功能。完成后汇报 pass/fail/partial/blocked、命令输出摘要、问题和未验证项。
```

如果 `testing-agent` 返回 `fail` 或 `partial`，主控 agent 决定是否返工：

- 代码问题退回 `coding-agent`。
- 视觉风格或提示词问题退回 `art-direction-agent`。
- 生成质量或文件问题退回 `imagegen-agent`。
- 验收标准不清楚则回到 `planning-agent` 或询问用户。

## Phase 5: Integration

主控 agent 检查：

- 代码是否引用正确资源路径。
- 生成图片是否存在并符合命名。
- `testing-agent` 的测试结论是否为 `pass`，或是否已明确记录未解决风险。
- 文档是否记录关键决策。

## Phase 6: Final Report

```text
Final:
- Delivered:
- Files changed:
- Generated assets:
- Validation:
- Testing result:
- Known risks:
- Suggested next experiment:
```
