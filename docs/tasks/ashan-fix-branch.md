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

**Nothing you've done is at risk, and nothing gets deleted below.** Your
real task commits, the Postman environment, the auth requests, the security
tests, the API test results, are already merged into `main` and permanently
in its history under your name. That happened when your branch was merged
earlier. This fix doesn't touch `main` at all, only your own personal
branch pointer, and even that just gets renamed, not removed.

The one commit your last push added ("Fixed issues and etc") is older than
what's already on `main`, it accidentally reverts the Postman collection to
a version missing things that are already there properly (recruiter token
capture, the security tests). It's not new work, so there's nothing worth
keeping from it, but if you want to keep it around anyway just in case,
that's exactly what renaming instead of deleting gives you.

## The fix

This renames your old branch out of the way and starts a clean one from
the current `main`. Your old branch still exists afterwards, under a new
name, you can look at it any time, it's just not the one you work on.

```bash
git checkout main
git pull origin main

# Rename your old branch instead of deleting it, so it's kept, just out of the way
git branch -m work/Ashan-Iduranga work/Ashan-Iduranga-old
git push origin work/Ashan-Iduranga-old
git push origin --delete work/Ashan-Iduranga

# Start a clean branch with your real name, from the current, up-to-date main
git checkout -b work/Ashan-Iduranga
```

Run `git log --oneline -5` afterwards. You should see the report chapters,
the diagrams, and everything else in recent history. If you see that,
you're on the right base. Your renamed old branch is still on GitHub if
you ever want to check it, it's just no longer the one anyone merges.

## Then do the UAT task

Your actual task is already written up and waiting on `main`:
[`docs/tasks/ashan-uat.md`](ashan-uat.md). Read that file fresh (pull it
from the branch you just recreated, not an old copy), do the three
workflows it describes, and commit and push exactly as it says.

## If you're not sure

Before pushing anything, run `git status` and `git log --oneline -5` and
send a screenshot to the group chat. Don't guess, and don't use `--force`
on anything, ask first.
