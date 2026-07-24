# Usability testing

## Scope
Informal usability feedback on the Meridian Talent live site
(`https://meridian-talent.pages.dev`), covering the candidate-facing job
search and application experience, the sign-in flow, the AI assistant, and
the ranked applicant table on the recruiter side.

## Test method
Feedback was gathered by having someone unfamiliar with the platform use
the live site and share their reactions and pain points directly, rather
than a formally timed multi-participant protocol. This session did not
capture per-task timing or pass/fail completion data, so those are not
reported here — the findings below are the qualitative friction points
that came up, which is a narrower but still genuine form of evidence than
the full stopwatch-based test originally planned.

## Findings

1. **Candidate-side UI feels dated next to the recruiter side.** The
   recruiter experience was described as well organized, but the candidate
   side was felt to need a more modern, user-friendly visual treatment by
   comparison.
2. **The application flow itself feels clunky.** General usability and
   "customer service" feel of the application process was flagged as
   needing improvement, without one single obvious blocker — more a
   cumulative friction across the flow.
3. **Sign-in requires a personal email account and password.** There is no
   option to create a platform-specific username/password separate from a
   personal email account. This was raised as a real limitation, not just
   a preference — some users may be uncomfortable using their personal
   email credentials to access a third-party platform.
4. **The AI assistant is single-language.** Multi-language support was
   raised as a gap that would broaden who can use the assistant
   comfortably.
5. **The ranked applicant table isn't mobile-friendly.** Details in the
   table are hard to view at a glance on a smaller screen, requiring extra
   effort (scrolling/zooming) to read what should be visible immediately.

## What you'd change

1. Refresh the candidate-side UI (spacing, typography, component styling)
   to bring it visually in line with the more polished recruiter side.
2. Walk through the application flow end-to-end and tighten copy, button
   placement, and step count to reduce the "clunky" feeling reported.
3. Add an optional platform-native sign-up (username + password) as an
   alternative to email-account sign-in, so users aren't required to
   reuse personal account credentials.
4. Scope multi-language support for the AI assistant as a future
   iteration, starting with the languages most relevant to the platform's
   user base.
5. Make the ranked applicant table responsive — collapse to a
   card-per-applicant layout or horizontal scroll with sticky key columns
   on narrow viewports, so all details remain visible without difficulty.

## Result
No single blocking failure was reported — the site is usable end-to-end —
but several friction points recurred across the candidate experience,
sign-in, and mobile table display. These are prioritized above roughly in
order of how directly they affect a first-time candidate completing an
application.

## Follow-up recommendation
A follow-up round with a second or third tester, run as a timed,
unassisted task (per the original protocol) would strengthen this
further by adding completion-time and success/failure data that this
round did not capture.
