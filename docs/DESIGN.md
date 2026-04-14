# Design System Document: High-End Editorial PWA

## 1. Overview & Creative North Star
**The Creative North Star: "The Digital Sanctuary"**

This design system moves away from the clinical, rapid-fire nature of modern social apps to create a "Digital Sanctuary." It is an editorial-first experience designed for the contemplative nature of Islamic lectures. We break the "template" look by treating the mobile screen like a high-end physical journal. 

Key to this identity is **intentional asymmetry** and **tonal depth**. Instead of centering everything, use generous, purposeful white space to let the content breathe. By shifting focus away from rigid grids and toward organic, layered compositions, we create an experience that feels human-made, grounded, and tactile.

---

## 2. Colors
Our palette is rooted in the natural world—sage, earth, and sand—to foster a sense of spiritual calm.

### The Palette (Material Design Tokens)
*   **Primary (`#416355`):** Reserved for core brand moments and primary actions.
*   **Surface (`#fdf9f2`):** The foundation. Warm, not cold.
*   **Secondary (`#685c53`):** Used for supporting elements and soft accents.
*   **Tertiary (`#7b514e`):** Subtle contrasting tones to add warmth.
*   **On-Surface (`#1c1c18`):** High-contrast charcoal for maximum legibility without the harshness of pure black.

### The "No-Line" Rule
To maintain the "Sanctuary" aesthetic, **1px solid borders are strictly prohibited for sectioning.** Boundaries must be defined solely through:
1.  **Background Color Shifts:** Use `surface-container-low` (#f7f3ec) against a `surface` (#fdf9f2) background.
2.  **Generous Spacing:** Use the empty space as a structural element.

### Surface Hierarchy & Nesting
Treat the UI as a series of stacked sheets of fine paper. 
*   **Level 0:** `background` (#fdf9f2) — The base of the app.
*   **Level 1:** `surface-container-low` (#f7f3ec) — Secondary sections or content groups.
*   **Level 2:** `surface-container-lowest` (#ffffff) — Actionable cards or focused content blocks.

### The "Glass & Grain" Rule
To add "soul," apply a global **2-5% opacity grain texture** over the entire background. For floating elements (like a sticky audio player), use **Glassmorphism**: 
*   **Color:** `surface` at 80% opacity.
*   **Effect:** `backdrop-blur: 12px`. 
This softens the interface and makes the PWA feel like a premium, integrated experience.

---

## 3. Typography
We utilize a pairing of **Manrope** for impact and **Plus Jakarta Sans** for clarity.

*   **Display & Headlines (Manrope):** Use heavy weights (Bold/ExtraBold). These should feel authoritative and editorial. Use `display-md` (2.75rem) for section intros to create a clear "starting point."
*   **Titles & Body (Plus Jakarta Sans):** Use `title-md` (1.125rem) for lecture titles. 
*   **Metadata & Labels (Plus Jakarta Sans):** Use `label-md` (0.75rem) with Light or Regular weights. Metadata should have a wider letter-spacing (0.05em) to feel "airy" and premium.

**Editorial Tip:** Don't be afraid of oversized typography. A large, bold headline next to a small, light metadata string creates the "High-End Editorial" tension that defines this system.

---

## 4. Elevation & Depth
We eschew traditional "drop shadows" in favor of **Tonal Layering**.

*   **The Layering Principle:** Depth is achieved by "stacking." A white card (`surface-container-lowest`) placed on a warm beige background (`surface-container-low`) provides a natural, soft lift without a single pixel of shadow.
*   **Ambient Shadows:** If a floating action button (FAB) or a high-priority modal requires a shadow, use an **Ambient Shadow**:
    *   **Color:** `on-surface` at 6% opacity.
    *   **Blur:** 24px - 32px.
    *   **Y-Offset:** 8px.
*   **The Ghost Border Fallback:** If accessibility requires a border, use `outline-variant` (#c1c8c3) at 15% opacity. It should be felt, not seen.

---

## 5. Components

### Cards & Lists
*   **Rule:** Forbid divider lines. 
*   **Implementation:** Separate list items using `1.5rem` (xl) vertical padding. To group items, wrap them in a `surface-container-high` (#ece8e1) container with `md` (0.75rem) rounded corners.
*   **Tactile Touch:** Cards should use the `xl` (1.5rem) corner radius for a soft, friendly feel.

### Buttons
*   **Primary:** Solid `primary` fill, `on-primary` text. No shadow. `full` (pill) roundedness.
*   **Secondary:** `secondary-container` (#f0e0d3) fill with `on-secondary-container` text.
*   **Tertiary:** No fill. `primary` text. Used for "Cancel" or "Back" actions.

### Audio Player (Context Specific)
*   **Surface:** Use the Glassmorphism rule.
*   **Controls:** Use `primary` for the Play/Pause toggle. Use `outline-variant` for the progress bar background, with a `primary` fill for the progress.

### Input Fields
*   **Style:** Minimalist. A subtle `surface-container-highest` (#e6e2db) background. No bottom line or box border. 
*   **Focus:** Transition the background to `primary-fixed` (#c5ebd9) at 10% opacity.

---

## 6. Do’s and Don’ts

### Do:
*   **Do** use asymmetrical layouts. Place a title on the left and metadata on the far right.
*   **Do** leverage the grain texture to give the flat colors a "paper" feel.
*   **Do** use "Body-Large" for lecture descriptions to encourage reading.
*   **Do** allow elements to overlap slightly (e.g., a card bleeding off the edge of the screen) to break the "contained" mobile feel.

### Don’t:
*   **Don’t** use pure black (#000000). It breaks the "Natural" style. Use `on-surface` (#1c1c18).
*   **Don’t** use standard 1px dividers. If you need a break, use a `2px` wide, 24px tall vertical line in `primary` as a decorative accent instead.
*   **Don’t** use "springy" or flashy animations. Use "Ease-in-out" transitions with 300ms–500ms durations to mimic a calm, deliberate movement.
*   **Don’t** crowd the edges. Maintain a minimum global padding of `1.5rem`.

---

**Director's Note:** This system is about the *pause* between the notes. Use the generous white space and the muted sage tones to tell the user that they are in a place of reflection. Every pixel should feel intentional, quiet, and premium.
