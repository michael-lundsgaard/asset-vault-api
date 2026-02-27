---
name: full-codebase-review
description: Analyze the current codebase implementation and provide a structured code review.
argument-hint: "Review the codebase and provide feedback on what's done well and what needs improvement."
---

Analyze the current codebase implementation and provide a structured code review.

## What's Done Well

Identify implementations that are correct, smart, or exemplary. For each, briefly explain _why_ it's a good approach (e.g. performance, readability, scalability, security, maintainability).

## What Needs Improvement

Categorize every issue by severity:

### 🔴 Critical

Issues that must be fixed immediately. Includes: security vulnerabilities, data loss risks, broken functionality, race conditions, or anything that could cause production incidents.

### 🟠 High

Significant problems that will cause pain at scale or under edge cases. Includes: logic errors, poor error handling, missing input validation, tight coupling, or major performance bottlenecks.

### 🟡 Medium

Code quality issues that slow down development or increase risk over time. Includes: unclear abstractions, duplicated logic, missing tests for core paths, confusing naming, or tech debt accumulation.

### 🔵 Low

Minor improvements. Includes: style inconsistencies, minor naming issues, small refactor opportunities, or missing comments on complex logic.

### 💡 Suggestions (Optional)

Non-essential ideas worth considering — architectural improvements, library swaps, or patterns that could future-proof the code.

---

For each issue, provide:

- **Location**: file/function/line where relevant
- **Problem**: what's wrong and why it matters
- **Fix**: a concrete recommendation or code snippet
