# Ashan: getting your UAT work actually onto GitHub

Run these one at a time, in order, and look at what each one prints before
running the next. Don't paste them all at once blindly.

## 1. Make sure you're on your own branch

```bash
git branch --show-current
```

This should print `work/Ashan-Iduranga`. If it prints anything else,
run:

```bash
git checkout work/Ashan-Iduranga
```

## 2. See what you actually have

```bash
git status
```

Look for `docs/testing/uat.md` in the list. If it says
`nothing to commit, working tree clean` and you don't see `uat.md`
anywhere, **the file was never created**, go back and actually write it
per `docs/tasks/ashan-uat.md` before continuing.

If you do see `docs/testing/uat.md` (and maybe a `docs/testing/uat/`
folder with screenshots) listed as untracked or modified, continue.

## 3. Stage and commit it

```bash
git add docs/testing/uat.md docs/testing/uat/
git commit -m "Add end-to-end UAT covering application, screening and reporting workflows"
```

## 4. Check your commit is really there

```bash
git log --oneline -3
```

You should see your new commit at the top. If you don't, the commit
failed, stop and show this output before doing anything else.

## 5. Push it

```bash
git push origin work/Ashan-Iduranga
```

If this is rejected with something like "non-fast-forward" or
"updates were rejected", **don't force it**. Run:

```bash
git pull origin work/Ashan-Iduranga
```

then try `git push origin work/Ashan-Iduranga` again.

## 6. Confirm it's actually on GitHub

Go to `https://github.com/Dinuja-Ransara/Recruitment-Platform/tree/work/Ashan-Iduranga/docs/testing`
in a browser. You should see `uat.md` listed there. If you see it there,
you're done, tell Hasitha.
