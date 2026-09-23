---
name: ponytail
description: "Use when: reducing over-engineering, trimming unnecessary code, choosing native browser/stdlib features over custom abstractions, auditing diffs for scope creep, applying YAGNI, or following the lazy-senior-dev 'best code is the code you never wrote' principle from the Ponytail repo."
---

# Ponytail

Use Ponytail as a minimalism-first engineering rule for this project.

## Core rules

- Prefer the smallest correct solution.
- Reuse existing code before creating new abstractions.
- Prefer native platform features, browser APIs, and standard library solutions over custom wrappers.
- Do not add dependencies, services, or files without a clear need.
- Cut unnecessary polish, scaffolding, and speculative features.
- Keep the change easy to review, test, and maintain.
- Never sacrifice correctness, validation, security, accessibility, or reliability to save lines.

## Decision ladder

1. Does this need to exist at all?
2. Is it already present in this codebase?
3. Can the platform or standard library do it?
4. Can a native solution do the job in a single clear step?
5. If not, add the minimum code required to satisfy the requirement.

## Working style

- Read the exact code path before editing.
- Trace the real flow rather than guessing.
- Avoid redundant wrappers, duplication, and abstraction churn.
- Prefer direct implementation over creating a framework around a simple task.
- Keep the diff surgical and review-friendly.

## Reference

- Repo: https://github.com/DietrichGebert/ponytail
- Principle: "The best code is the code you never wrote."

Use this skill whenever the goal is to keep the solution lean, practical, and free from accidental over-engineering.
