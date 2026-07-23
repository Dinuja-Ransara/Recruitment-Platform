# Working on Meridian

Four people, one repository, a deadline. This document exists so nobody has to
guess, and so no one action can break the main branch.

## The two rules that matter

1. **Never push to `main`.** Work on your own branch and push that. Hasitha
   merges. If `main` breaks the whole group is blocked.
2. **Never use `--force`.** If a push is rejected, stop and ask. A force push can
   delete someone else's work permanently and cannot be undone from your machine.

Everything else below is detail.

## Who owns which files

Conflicts happen when two people edit the same file. They have been avoided by
giving each person their own area. **Stay inside yours** and merges stay trivial.

| Person | Owns | Task list |
|---|---|---|
| Ashan | `postman/`, `docs/testing/` | [docs/tasks/ashan.md](docs/tasks/ashan.md) |
| Dinuja | `docs/diagrams/`, `docs/report/` | [docs/tasks/dinuja.md](docs/tasks/dinuja.md) |
| Sewmin | `seed/`, `docs/video/`, plus named UI files | [docs/tasks/sewmin.md](docs/tasks/sewmin.md) |
| Hasitha | `src/`, `tests/`, `deploy/`, `docs/adr/` | architecture, backend, engine, integration |

If you genuinely need to change a file outside your area, say so in the group
chat first rather than editing it quietly.

## One-time setup

```bash
git clone https://github.com/Dinuja-Ransara/Recruitment-Platform.git
cd Recruitment-Platform

# Identify yourself, so your commits are actually attributed to you.
git config user.name  "Your Name"
git config user.email "the-email-on-your-github-account@example.com"
```

The email **must** be the one on your GitHub account, otherwise GitHub will not
link the commits to you and the contribution history will not show your work.

## Every working session

```bash
# 1. Start from the latest main.
git checkout main
git pull origin main

# 2. Create your branch, once. Use your own name.
git checkout -b work/ashan          # or work/dinuja, work/sewmin

# 3. Do one task. Commit it. Repeat.
git add <the files you changed>
git commit -m "Short summary of what this commit does"

# 4. Push your branch.
git push -u origin work/ashan       # first push
git push                            # every push after that
```

After the first push, `git push` on its own is enough.

## Committing well

Make **one commit per completed piece of work**, not one commit for everything at
the end. Six small commits tell the story of how the work was done; one giant
commit tells nobody anything.

Write the message as a sentence describing what changed:

```
Add authentication requests to the Postman collection
Add the use case diagram source
Fix keyboard focus outlines on the applicant list
```

Not `update`, `fix`, `changes`, or `asdf`.

## If something goes wrong

**You committed to `main` by mistake, but have not pushed:**

```bash
git branch work/yourname      # save your work onto a branch
git reset --hard origin/main  # put main back
git checkout work/yourname    # carry on there
```

**Your push was rejected because main moved on:**

```bash
git pull --rebase origin main
# resolve any conflicts, then
git push
```

**You are lost.** Stop. Do not run anything with `--force`, `reset --hard` or
`clean`. Send a screenshot of `git status` to the group. Nothing is lost as long
as you have committed, and it can be recovered.

## Checking your work is committed

```bash
git status          # should say "nothing to commit, working tree clean"
git log --oneline   # your commits should be listed at the top
```

## Merging (Hasitha only)

```bash
git checkout main
git pull origin main
git merge work/ashan --no-ff -m "Merge Ashan's API testing evidence"
git push origin main
```

`--no-ff` keeps each person's branch visible as its own line in the history,
which is what makes the individual contribution readable in the final report.
