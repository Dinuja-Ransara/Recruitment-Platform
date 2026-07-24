# Accessibility audit

Keyboard-only pass across every screen, checking focus reachability, focus
visibility, form labelling, icon-only controls, and colour-only signalling.

**Method:** signed in as each role in turn, no mouse, Tab / Shift+Tab /
Enter / Space only.

## What the code already guarantees

These three were checked directly against the source rather than guessed,
and hold true on every screen because they are implemented once, centrally,
not per page:

| Check | Where it's enforced | Result |
|---|---|---|
| Focus outline always visible | `src/index.css`, global `:focus-visible { outline: 2px solid var(--color-accent); outline-offset: 2px; }` applies site-wide | Pass |
| Icon-only controls have a label | The one icon-only pattern (`PlusIcon` on "New posting") pairs the icon with visible text and marks the icon `aria-hidden="true"`, so nothing depends on the icon alone | Pass |
| Every form input has a label | The shared `Field` component wraps its input in a real `<label>` element, so the association is native, not bolted on with a mismatched `id`/`for` | Pass |

## What still needs a live keyboard pass

The three checks above hold by construction, but a Tab-through of the actual
running app is still the only way to catch things static reading can't:
focus order that jumps around, a modal that traps focus wrong, a control
that's reachable but does nothing on Enter.

Screens to check, Tab only, no mouse:

| Screen | Reachable by Tab | Focus order sane | Notes |
|---|---|---|---|
| Sign in | | | |
| Job search (public) | | | |
| Job detail / apply | | | |
| Recruiter: My postings | | | |
| Recruiter: Ranked applicants (dense) | | | |
| Recruiter: Score breakdown panel | | | |
| Candidate: My applications | | | |
| Hiring manager: Shortlists | | | |
| Admin: System health | | | |

Fill in Pass / Fail plus a one-line note per screen, delete any row for a
screen that doesn't exist in the build being demoed.

## Colour-only signalling

Status badges (Draft / Published / Closed, application status) use colour
plus a text label together, never colour alone. Confirm this holds on the
Ranked Applicants and My Postings screens specifically, those are the
densest and most likely place a colour-only shortcut would sneak in.

## Summary

_Fill in after the live pass: how many screens passed outright, what (if
anything) needed a fix, and confirm the fix, if any, is committed in
`src/meridian.web/src/components/ui.tsx` or `src/meridian.web/src/index.css`._
