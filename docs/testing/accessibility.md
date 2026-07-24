# Accessibility audit

## Scope
Keyboard-only accessibility pass for the Meridian Talent demonstration site, including the landing page, role sections, live demo entry points, ranked applicant preview, score explanation content, and general navigation actions. The current site presents a recruitment platform with demo access for four roles and a visible applicant ranking preview for a senior backend engineer role.

## Test method
Testing was based on keyboard-only interaction using Tab, Shift+Tab, Enter, and Space across visible interactive elements on the current site. The review focused on visible focus indication, accessible names for action controls, form and control labeling patterns, and whether score states such as a mandatory gap were understandable without relying on colour alone.

## Screens reviewed
- Landing hero with the main calls to action: **Try the live demo** and **See how scoring works**.
- Ranked applicant preview showing candidate names, years, scores, and mandatory-gap indicators.
- Informational sections explaining scoring signals and the four supported roles: Candidates, Recruiters, Hiring managers, and Administrators.

## Findings from the older version
1. The older version had keyboard focus that was too subtle on primary actions in the hero area, especially on the live demo and scoring links. Keyboard users could tab to the controls, but the active position was not always obvious against the surrounding layout.
2. Some compact action controls and icon-style UI elements in shared components did not consistently expose an accessible name. Icon-only or visually minimal controls should provide an accessible label through `aria-label` or equivalent naming.
3. Status messaging in the ranked applicant preview relied too heavily on visual emphasis around labels like **mandatory gap**. Important candidate state should not be communicated by colour alone and should remain clear through text treatment and wording.
4. Reusable form and control patterns in the older UI needed stronger guarantees that labels stayed associated with their fields whenever shared components were reused across recruiter and candidate screens. Clear labels are necessary so forms remain understandable for keyboard and assistive-technology users.

## Fixes applied
- Updated shared focus-visible styling in `src/meridian.web/src/index.css` so buttons, links, inputs, and other interactive controls display a stronger outline and supporting focus ring during keyboard navigation. This improves orientation on dense screens and on high-value actions such as opening the demo or moving through screening workflows.
- Improved reusable controls in `src/meridian.web/src/components/ui.tsx` so icon-only actions can expose a clear accessible name and shared field components preserve safer labeling behavior. This reduces the chance of unlabeled buttons or ambiguous controls appearing in future screens.
- Reviewed status presentation so critical screening information, including mandatory requirement failures, remains understandable through text and layout rather than colour alone. The current site already uses explicit wording such as **mandatory gap**, which is a stronger pattern than colour-only signaling.

## Current result
The current site communicates scoring and applicant ranking clearly, and the visible text treatment around the ranked preview makes key states more understandable than a colour-only approach. The landing page structure is straightforward, the main calls to action are easy to identify, and the score explanation section provides useful textual context for users navigating without a mouse.

## Follow-up recommendation
Each future screen added to the live demo should receive a short keyboard-only regression pass after development, especially ranked lists, score breakdown tables, and admin actions. Shared UI components should remain the single place for focus, labeling, and icon-button accessibility rules so new screens inherit the same behavior consistently.