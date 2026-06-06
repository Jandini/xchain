---
name: pr-from-diff
description: 'Generate a pull request title and description from current branch git diffs. Use when asked to draft PR text, summarize changes, or write release-ready PR metadata from branch history.'
argument-hint: 'Optional focus, audience, or PR template requirements'
user-invocable: true
disable-model-invocation: false
---

# PR Title and Description from Git Diff

## What This Skill Produces
- A PR title based on commits and diffs in the current branch since it diverged from `main`.
- A PR description based on git log and git diff evidence for the same range.

## Non-Negotiable Input Rule
- Do not read source files directly.
- Do not infer behavior from file contents outside git output.
- Use only `git` command output: branch name, commit messages, changed file list, and patch diffs.

## Scope Rule
- This skill is intentionally scoped to: "analyze changes in this branch all the way to main branch, then write PR title and description".
- Treat the current checked-out branch as the target head.
- Treat `main` as the base branch.
- Always include all commits from merge-base to current `HEAD`.

## When to Use
- User asks to analyze current branch changes up to `main`.
- User asks for a PR title and PR description for this branch.

## Procedure
1. Resolve branch and range.
- Run: `git rev-parse --abbrev-ref HEAD` to capture current branch.
- Run: `git merge-base main HEAD` and store as `<mergeBase>`.
- Set analysis range to `<mergeBase>..HEAD`.

2. Collect commit evidence from current branch to main divergence.
- Run: `git log --oneline --no-merges <mergeBase>..HEAD`.
- This commit list is the authoritative change history for the PR narrative.

3. Collect diff evidence for the same range.
- Run: `git diff --name-status <mergeBase>..HEAD`.
- Run: `git diff --stat <mergeBase>..HEAD`.
- Run: `git diff --numstat <mergeBase>..HEAD`.
- Run: `git diff --unified=3 <mergeBase>..HEAD`.

4. Draft PR title.
- Base title on dominant theme across commit messages and high-impact diff areas.
- Prefer concise imperative phrasing.
- Include ticket/scope from branch name if clearly present.

5. Draft PR description.
- Use this section order:
- Summary
- Why
- What Changed
- Risk and Impact
- Testing
- Every statement must be backed by commit or diff evidence from `<mergeBase>..HEAD`.

6. Validate completeness.
- Ensure all commits in `git log --oneline --no-merges <mergeBase>..HEAD` are reflected in the narrative.
- Ensure key changed areas from `--name-status` and `--stat` are explicitly mentioned.
- Do not include claims unsupported by git output.

## Decision Points
- If no commits are present in `<mergeBase>..HEAD`:
- Return a no-change PR title and a description that states no committed changes are detected.
- If diff is mostly renames/moves:
- Emphasize structural reorganization and avoid overstating behavioral changes.
- If branch has mixed topics:
- Use one unifying title and summarize changes in grouped bullets under What Changed.

## Output Format
Provide final answer in markdown with clearly separated copy/paste sections.

Required structure:
1. `PR Title (copy/paste)` section
- Include a fenced markdown code block containing only the final title text on one line.

2. `PR Description (copy/paste)` section
- Include a fenced markdown code block containing only the final PR description body.

Optional structure:
3. `Alternate Titles` section when ambiguity is high
- Include 2 alternatives as bullet points (outside code fences).

4. `Assumptions` section only when needed
- Keep assumptions concise and evidence-based.

Formatting constraints:
- Do not combine title and description in the same code block.
- Do not add explanatory prose inside either copy/paste code block.
- Ensure both blocks are valid markdown and ready to paste directly into PR UI fields.

## Example Invocation Prompts
- `/pr-from-diff`
- `/pr-from-diff analyze changes in git log in this branch all the way to main branch, write PR title and description`
