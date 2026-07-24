# Responsive verification

## Scope
Responsive layout check for the Meridian Talent demo site at 360px (mobile), 768px (tablet) and 1280px (desktop) using Chrome DevTools device mode. Focus was on the landing hero, ranked applicant preview for the senior backend engineer role, and the score explanation section. [web:111][web:114][web:120]

## Method
The page was opened in Chrome, DevTools were activated, and the device toolbar was used in responsive mode. The viewport width was set to 360, 768 and 1280 pixels to simulate common mobile, tablet and desktop layouts while inspecting content and interactions. [web:111][web:114][web:120]

## 360px (mobile)
- The hero headline and primary call to action are readable and fit within the viewport without horizontal scrolling.
- The ranked applicant preview remains legible, and the mandatory-gap text is visible under each relevant row.
- Vertical spacing is tighter but still usable; touch targets remain reachable in a single scroll column.

## 768px (tablet)
- The layout expands, and card sections gain more breathing room without losing hierarchy.
- The ranked applicant list and score explanation section display side-by-side or stacked depending on the breakpoint, and text does not overlap.
- Tablet orientation presents the same information clearly, with readable body copy and headings.

## 1280px (desktop)
- The hero, ranked preview and explanation content align in a spacious grid with clear reading order.
- The score preview table has sufficient width for columns and labels without truncation.
- No horizontal scroll is required on standard desktop widths, and typography remains consistent.

## Notes
Screenshots for each breakpoint were captured into `docs/testing/responsive/` as a visual record of the layout at mobile, tablet and desktop widths. Future screens added to the demo should be checked at these widths using the same process to prevent regressions.
