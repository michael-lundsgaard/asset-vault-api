# Generate a Conventional Commit Message

Analyze the staged changes (or all unstaged changes if nothing is staged) and produce **one logical commit message** following Conventional Commits 1.0.

Your goal is to describe the **intent of the change**, not list every modification.

---

## Output Format

```
<type>(<scope>): <short summary>

<body explaining motivation and behavior>

<footer(s) if needed>
```

The blank lines separating subject, body, and footer are required.
Do not output markdown fences or commentary — only the commit message.

---

## Core Principles (IMPORTANT)

1. **Think in features, not files**
    - Multiple modified files often belong to one change.
    - Produce a single cohesive description.

2. **Always choose ONE primary purpose**
    - If multiple unrelated changes exist, describe the dominant one
      and treat others as supporting context.
    - Never write multiple independent changes in the same commit message.

3. **Prioritize externally visible impact**
    - API behavior
    - data model behavior
    - observable runtime behavior
    - developer usage

4. **De-emphasize mechanical refactors**
   Mention them only if they support the feature:
    - renames
    - repository plumbing
    - formatting / nullability cleanup

5. **Do NOT narrate the diff**
   Avoid phrases like:
    - "add file"
    - "update class"
    - "modify method"
    - "change property"

6. **The first line must answer:**
    > What capability does this introduce or fix?

---

## Choosing the Type

| Type       | Meaning                                                         |
| ---------- | --------------------------------------------------------------- |
| `feat`     | New capability or new API behavior                              |
| `fix`      | Corrects incorrect behavior                                     |
| `refactor` | Internal restructuring with identical runtime behavior          |
| `perf`     | Performance improvement                                         |
| `test`     | Tests only                                                      |
| `build`    | Dependencies / build system                                     |
| `ci`       | Pipeline changes                                                |
| `docs`     | Documentation only                                              |
| `chore`    | Tooling / config / structure without production behavior change |

**Rule:**  
If users of the API must read release notes → `feat` or `fix`  
If only developers care → `refactor` or `chore`

Prefer `feat` when a new endpoint, query capability, or response shape
becomes available — even if most code changes are internal plumbing.

---

## Choosing the Scope

Use the functional area, not the layer:

Good:

- `assets`
- `collections`
- `tags`
- `storage`

Avoid:

- `controllers`
- `handlers`
- `dtos`
- `repository`

---

## Subject Line Rules

- imperative mood
- lowercase
- ≤72 chars
- no trailing period
- describe behavior, not implementation

Good:

```
feat(assets): support expand for collection and tags
```

Bad:

```
feat(api): add GetAssetsQueryHandler and controller
```

---

## Body Rules

Separate from the subject with exactly one blank line
Wrap all lines at 72 characters

Explain **why the change exists** and clarify behavior.

Include:

- behavior details
- constraints
- compatibility notes

Avoid:

- listing files
- repeating the subject
- low-level implementation steps

The body must add new information not present in the subject line.  
If it only repeats the summary, omit the body entirely.

---

## Breaking Changes

If behavior changes incompatibly:

```
feat(api)!: rename asset url field to storageUrl

BREAKING CHANGE: url field removed from responses
```

---

## Examples

Good:

```
feat(assets): add listing endpoint with expand support

Allows clients to include related collection and tag data via the
expand query parameter. Ensures stable ordering and omits unused
fields from responses when not expanded.
```

Bad:

```
feat(assets): add query handler, controller, dto, mapping, repository
```

---

## Final instruction

Review the diff, infer the single primary intent, and write the
cleanest commit message a human maintainer would write after
refactoring the branch into one commit.

Output only the commit message.
