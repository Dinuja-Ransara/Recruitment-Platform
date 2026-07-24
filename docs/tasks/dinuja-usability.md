# Dinuja: extra task, usability testing

**Why this matters:** Task 1 of the brief explicitly says "Conduct usability
testing and improve the user experience based on collected feedback." Right
now there is no evidence of this anywhere in the submission. This is the
whole of that, and it is genuinely fast to produce for real.

This is on top of your diagrams and report chapters, not instead of them.
Only takes an hour or so including the write-up.

## What to do

Find **2 or 3 people** (coursemates, friends, family, anyone who has never
used the app before). Sit with them, one at a time, and watch them use the
**live site**: `https://meridian-talent.pages.dev`

Pick **one** of these two workflows to test (don't do both, one done
properly beats two done thin):

- **Job search → apply**: starting from the homepage, find a job and submit
  an application as a candidate.
- **Create posting → rank applicants**: signed in as recruiter, create a job
  posting, publish it, then open the ranked applicants screen.

## Rules for the session

- **Do not help them.** Give them the goal in one sentence ("find a backend
  engineering job and apply to it") and then go quiet. Watching where they
  get stuck is the entire point.
- **Time it.** Start a stopwatch when they begin, stop it when they finish
  or give up.
- **Write down every moment of hesitation**, not just outright failures.
  Someone pausing for 10 seconds looking for a button is a real finding.

## What to record, per person

For each of your 2-3 testers, note:

- How long the task took
- Whether they completed it unassisted
- Every point they hesitated, clicked the wrong thing, or asked "wait, how
  do I—"
- One thing they said out loud, if anything (people narrate while testing,
  write down the useful bits verbatim)

## Write it up

Create `docs/testing/usability.md` with:

1. **Method** — which workflow, how many testers, how you ran it (one
   paragraph, matches the style of `docs/testing/accessibility.md` already
   in the repo, read that one first for tone).
2. **A table**, one row per tester: time taken, completed (yes/no), what
   tripped them up.
3. **Findings** — the 2-4 friction points that came up more than once. These
   are your real findings, not invented ones. If everyone breezed through,
   say that honestly too, that is still evidence.
4. **What you'd change** — for each friction point, one sentence on the fix.
   You don't have to build the fix, the brief asks for the testing and the
   improvement *plan*, not necessarily a shipped fix, given the time left.
   If there's time and it's a one-line CSS/copy change, feel free to just
   make it (matches how Sewmin's accessibility pass fixed small things
   directly in `ui.tsx` / `index.css`).

## Commit it

```bash
git checkout main
git pull origin main
git checkout -b work/dinuja-usability
git add docs/testing/usability.md
git commit -m "Add usability testing results for the job application workflow"
git push -u origin work/dinuja-usability
```

Then tell Hasitha it's pushed, same as always. Don't merge it yourself.
