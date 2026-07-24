# Ashan: your branch needs a reset before you push again

**Read this before doing anything else on `work/Ashan-Iduranga`.**

## What happened

Your branch was created a while ago, before a lot of work landed on `main`:
the report chapters, the diagrams, the AI assistant, cloud resume storage,
the new homepage, the unit tests. Your machine never pulled that in. When
you committed "Fixed issues and etc" on top of your old copy and pushed it,
your branch ended up **behind** `main`, not ahead of it.

If that branch gets merged as it currently stands, it would **delete**
everything listed above, dozens of files, because as far as your branch
knows those files were never added. Nobody has merged it, so nothing is
lost yet, but don't push anything further to it until it's fixed.

Nothing in your work is actually lost either. Your UAT task
(`docs/tasks/ashan-uat.md`) was never started on that branch, and the
Postman edits your last commit made are older than what's already on
`main`, not new work worth keeping.

## The fix

This resets **only your own branch**, not `main`, not anyone else's work.

```bash
git checkout main
git pull origin main

# Delete your stale local branch
git branch -D work/Ashan-Iduranga

# Delete your stale branch on GitHub too
git push origin --delete work/Ashan-Iduranga

# Recreate it fresh, from the current, up-to-date main
git checkout -b work/Ashan-Iduranga
```

Run `git log --oneline -5` afterwards. You should see the report chapters,
the diagrams, and everything else in your recent history. If you see that,
you're on the right base.

## Then do the UAT task

Your actual task is already written up and waiting on `main`:
[`docs/tasks/ashan-uat.md`](ashan-uat.md). Read that file fresh (pull it
from the branch you just recreated, not an old copy), do the three
workflows it describes, and commit and push exactly as it says.

## If you're not sure

Before pushing anything, run `git status` and `git log --oneline -5` and
send a screenshot to the group chat. Don't guess, and don't use `--force`
on anything, ask first.
