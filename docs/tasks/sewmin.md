# Sewmin: demonstration data, accessibility and the video

**Your branch:** `work/sewmin`
**Your files:** `seed/`, `docs/video/`, and two named UI files listed below.

**Why this matters:** a ranking screen only proves something when the pool it
ranks has a real spread in it, and the demonstration video is a submission item
in its own right. A missing or unshared video is an automatic zero.

Read [CONTRIBUTING.md](../../CONTRIBUTING.md) first for the git workflow.

---

## Commit 1: More applicants

The demonstration pool currently has seven candidates. Ranking is more convincing
with more of them, and the analytics have more to plot.

Write `seed/candidates.md` describing **six more applicants** as prose, following
the pattern already in
`src/Meridian.Infrastructure/Persistence/DemoDataSeeder.cs`. For each: name,
headline, city, country, years of experience, education, and a **realistic
paragraph of CV text**.

Write real prose, not filler. The scoring engine reads this text with TF-IDF, so
"Lorem ipsum" produces meaningless similarity scores and an unconvincing demo.

Aim for a spread: two strong fits, two partial, one over-qualified from a
different technology stack, one clearly unsuitable.

```bash
git add seed/candidates.md
git commit -m "Add six more demonstration candidates with realistic CV text"
```

## Commit 2: More job postings

Write `seed/jobs.md` describing **four more postings** across the three offices
(Colombo, Singapore, London). For each: title, description, responsibilities,
city, country, work mode, seniority, minimum years, salary range and currency,
and the required skills marked mandatory or preferred.

Only use skills from the taxonomy. The list is in `DemoDataSeeder.SeedSkillsAsync`,
or call `/api/skills` while signed in.

```bash
git add seed/jobs.md
git commit -m "Add four more job postings across the three office locations"
```

## Commit 3: Accessibility pass

Sign in and work through every screen with the **keyboard only**, no mouse. Tab
through each page and note what breaks.

Check specifically:

- Every interactive element is reachable by Tab
- The focus outline is always visible, never invisible against the background
- Every form input has a label a screen reader can read
- Buttons that only show an icon have an `aria-label`
- Colour is never the only way information is conveyed

Record what you find in `docs/testing/accessibility.md` with a table of screen,
issue, and severity.

You may edit these two files to fix what you find, and only these two:

- `src/meridian.web/src/components/ui.tsx`
- `src/meridian.web/src/index.css`

```bash
git add docs/testing/accessibility.md src/meridian.web/src/components/ui.tsx src/meridian.web/src/index.css
git commit -m "Add accessibility audit and fix focus visibility issues"
```

## Commit 4: Responsive check

Open Chrome DevTools, toggle the device toolbar, and check every screen at
**360px, 768px and 1280px**. The brief requires desktop, tablet and mobile
support.

Screenshot each breakpoint into `docs/testing/responsive/` and record anything
broken in `docs/testing/responsive.md`.

The dense screens are the ones to watch: the ranked applicant list and the score
breakdown table.

```bash
git add docs/testing/responsive/ docs/testing/responsive.md
git commit -m "Add responsive verification across mobile, tablet and desktop"
```

## Commit 5: Video script

The video is **10 minutes total** and its structure is fixed by the guideline:

- **4 minutes**: the problem, the solution, objectives, the architecture and OOP
  concepts used, then a walk through the working system
- **1 minute each**: every member explains their own contribution *while
  demonstrating it in the running system*, and names a risk they managed

Write `docs/video/script.md` with a minute-by-minute running order, who speaks
when, and exactly which screen is on display at each point.

Everyone must state their **name and index number** at the start. Cameras on for
everyone, that is explicitly required.

Build the demonstration around the strongest moment: open the ranked applicant
pool, click "Why this score" on Arjun Mehta, and show that a principal engineer
with twelve years ranks fifth because he is missing a mandatory requirement.

```bash
git add docs/video/script.md
git commit -m "Add the demonstration video script and running order"
```

## Commit 6: Recording and upload notes

After the recording, write `docs/video/submission.md` with:

- The OneDrive link to the video
- **Confirmation that link sharing is set so any evaluator can open it.** Test it
  in a private browsing window while signed out. If an evaluator cannot open it
  the mark is **zero**, and this is the single most common way marks are lost
- The recording date and the final length

```bash
git add docs/video/submission.md
git commit -m "Add video submission details and sharing confirmation"
```

## When you are done

```bash
git push -u origin work/sewmin
```

Then tell Hasitha. **Do not merge it yourself.**

## Your minute in the presentation

Have the app open and demonstrate:

- Resize the browser live to show the interface working on a narrow screen
- Tab through a screen with the keyboard to show focus is visible and ordered
- The demonstration dataset, and why realistic CV prose matters: the engine reads
  it with TF-IDF, so placeholder text would produce meaningless scores
- The risk you managed: the dense screens, the ranked pool and the score
  breakdown table, are the hardest to keep readable on a narrow screen
