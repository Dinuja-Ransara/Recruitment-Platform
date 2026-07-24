# Responsive check

Three fixed breakpoints per the brief: **360px** (mobile), **768px** (tablet),
**1280px** (desktop). Checked with Chrome DevTools' device toolbar, not by
freehand dragging, so the widths are exact.

## What the code already does

Confirmed directly in the source: the two densest screens already use
responsive utility classes rather than a fixed desktop-only layout.

- `RankedApplicants.tsx` stat row: `grid gap-3 sm:grid-cols-3`, stacks to one
  column below the `sm` breakpoint instead of squeezing three columns into a
  phone width.
- `RankedApplicants.tsx` applicant card row: `flex flex-wrap items-center
  gap-4 p-5 sm:flex-nowrap`, wraps onto multiple lines on a narrow screen
  instead of overflowing horizontally.

That means the riskiest screen was already built with narrow widths in mind,
this pass is confirming it actually holds up, not building it from scratch.

## Screens to check at all three widths

| Screen | 360px | 768px | 1280px | Notes |
|---|---|---|---|---|
| Public homepage | | | | |
| Job search results | | | | |
| Job detail / apply | | | | |
| Recruiter: My postings | | | | |
| Recruiter: Ranked applicants | | | | |
| Recruiter: Score breakdown panel | | | | |
| Candidate: My applications | | | | |
| Analytics (funnel + tables) | | | | |

Fill in Pass / Fail per cell. The two to look at hardest are Ranked
Applicants and the score breakdown panel, and the Analytics posting
performance table, that one has six columns and is the one most likely to
force horizontal scroll on a phone.

## Screenshots

Save one screenshot per breakpoint for the two or three screens that matter
most (Ranked Applicants at minimum) into `docs/testing/responsive/`, named
`<screen>-360.png`, `<screen>-768.png`, `<screen>-1280.png`.

## Summary

_Fill in after the pass: which screens needed a fix, whether it was made,
and where, `src/meridian.web/src/index.css` or a Tailwind class change in
the component itself._
