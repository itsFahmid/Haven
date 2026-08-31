---
name: HAVEN
description: Bangladesh Youth Safety & Mental Health Sanctuary
colors:
  primary: "#0f766e"
  primary-hover: "#0d9488"
  primary-light: "#14b8a6"
  mint-tint: "#f0fdfa"
  aqua-highlight: "#ccfbf1"
  neutral-bg: "#fdfcf9"
  neutral-card: "#ffffff"
  text-primary: "#1e293b"
  text-dark: "#0f172a"
  alert-crisis: "#e11d48"
  crisis-bubble-border: "#fecdd3"
  verified-green: "#059669"
  verified-green-deep: "#047857"
  bkash: "#e2136e"
  nagad: "#f7941d"
  rocket: "#8c3494"
typography:
  display:
    fontFamily: "Outfit, Hind Siliguri, sans-serif"
    fontSize: "clamp(2rem, 5vw, 3.75rem)"
    fontWeight: 800
    lineHeight: 1.18
    letterSpacing: "-0.025em"
  headline:
    fontFamily: "Outfit, Hind Siliguri, sans-serif"
    fontSize: "clamp(1.5rem, 3vw, 2.25rem)"
    fontWeight: 800
    lineHeight: 1.25
    letterSpacing: "-0.02em"
  title:
    fontFamily: "Outfit, Hind Siliguri, sans-serif"
    fontSize: "1.25rem"
    fontWeight: 700
    lineHeight: 1.4
  body:
    fontFamily: "Outfit, Hind Siliguri, sans-serif"
    fontSize: "0.875rem"
    fontWeight: 400
    lineHeight: 1.6
  label:
    fontFamily: "Outfit, Hind Siliguri, sans-serif"
    fontSize: "0.75rem"
    fontWeight: 700
    letterSpacing: "0.05em"
rounded:
  sm: "8px"
  md: "12px"
  lg: "16px"
  xl: "24px"
  full: "9999px"
spacing:
  xs: "4px"
  sm: "8px"
  md: "16px"
  lg: "24px"
  xl: "32px"
  2xl: "48px"
components:
  button-primary:
    backgroundColor: "{colors.primary}"
    textColor: "#ffffff"
    rounded: "{rounded.lg}"
    padding: "16px 28px"
  button-primary-hover:
    backgroundColor: "{colors.primary-hover}"
  button-secondary:
    backgroundColor: "{colors.neutral-card}"
    textColor: "{colors.text-primary}"
    rounded: "{rounded.lg}"
    padding: "16px 28px"
  button-crisis:
    backgroundColor: "{colors.alert-crisis}"
    textColor: "#ffffff"
    rounded: "{rounded.md}"
    padding: "8px 16px"
  chat-bubble-user:
    backgroundColor: "{colors.primary-hover}"
    textColor: "#ffffff"
    rounded: "18px 18px 4px 18px"
    padding: "12px 18px"
  chat-bubble-bot:
    backgroundColor: "#f8fafc"
    textColor: "{colors.text-primary}"
    rounded: "18px 18px 18px 4px"
    padding: "12px 18px"
  chat-bubble-crisis:
    backgroundColor: "#fff1f2"
    textColor: "#881337"
    rounded: "18px 18px 18px 4px"
    padding: "12px 18px"
---

# Design System: HAVEN

## Overview

**Creative North Star: "The Digital Healing Sanctuary"**

HAVEN is an anonymous, trauma-informed child & adult safety education, mental health, and emergency recovery platform for Bangladesh. The visual interface is designed to evoke absolute safety, emotional calm, and immediate clarity. Built around soft warm paper backgrounds, deep organic sanctuary teal accents, and protective glassmorphic layers, HAVEN balances immediate crisis response with reassuring mental health sanctuary aesthetics.

The visual tone is **Empathetic, Protective, Calming & Modern**. It rejects cold, clinical hospital vibes and aggressive neon aesthetics in favor of welcoming warm surfaces (`#fdfcf9`), organic rounded geometries (16px–24px radii), fluid somatic animations (19-second breathing loops), and dual-script (Bengali & English) typographic harmony.

**Key Characteristics:**
- **Trauma-Informed Warmth:** Soft warm-cream background (`#fdfcf9`) eliminating stark white glare.
- **Sanctuary Teal Hierarchy:** Deep teal (`#0f766e`) anchors primary actions and trust badges; soft mint (`#f0fdfa`) provides calming container tints.
- **Glassmorphic Security:** Translucent frosted glass headers (`backdrop-filter: blur(12px)`) and floating quick-exit emergency controls.
- **Dual-Script Typographic Excellence:** Seamless side-by-side rendering of English (*Plus Jakarta Sans*) and Bengali (*Hind Siliguri*).
- **Fluid Somatic Motion:** Interactive 19s breathing reset rings and pulsing red emergency SOS indicators.

## Colors

The HAVEN color system is built around soothing organic teals, warm cream neutral surfaces, and high-visibility emergency rose accents.

### Primary
- **Sanctuary Teal** (`#0f766e` / `rgb(15, 118, 110)`): The core identity color representing sanctuary, growth, and trust. Used for primary call-to-action buttons, key brand badges, active navigation indicators, and primary header elements.
- **Teal Hover Accent** (`#0d9488`): Deeper hover state for interactive teal buttons and active user chat bubbles.
- **Vibrant Mint Light** (`#14b8a6`): Used for interactive somatic exercise rings and glowing status indicators.

### Secondary
- **Mint Foam Tint** (`#f0fdfa`): Calming background tint for feature callouts, active payment gateway selections, and badge fills.
- **Aqua Highlight** (`#ccfbf1`): Subtle border and tag highlight for youth safety badges and trust indicators.

### Neutral
- **Warm Cream Surface** (`#fdfcf9`): Global page background providing an organic, paper-like warmth that reduces cognitive load and eye strain.
- **Pure Card White** (`#ffffff`): Elevated card surface for content modules, course cards, and modal dialogs.
- **Slate Navy Body** (`#1e293b`): High-legibility text color for body paragraphs, menu links, and labels.
- **Midnight Slate** (`#0f172a`): Dark slate accent for emergency notice banners, dark glass panels, and footer backgrounds.

### Crisis Alert
- **Emergency Rose** (`#e11d48`): High-priority alert color strictly reserved for life-critical SOS triggers, national emergency hotline callouts (999/1098/109), and Quick Exit controls.

### Tertiary — Domain Brand Colors
These specialist colors serve specific, non-repeatable contexts. Use them only in those exact contexts.

- **BMDC Verified Green** (`#059669` → `#047857`): Gradient fill for "BMDC Verified" trust badges on therapist cards. Not used elsewhere.
- **bKash Magenta** (`#e2136e`): Selected state border and glow for the bKash payment gateway card. Strictly the bKash brand color — do not repurpose.
- **Nagad Orange** (`#f7941d`): Selected state for the Nagad gateway card.
- **Rocket Purple** (`#8c3494`): Selected state for the Rocket gateway card.

### Named Rules
**The One Voice Rule.** Primary Sanctuary Teal (`#0f766e`) is used on ≤15% of any given viewport surface to preserve its authority as a trust anchor. Emergency Rose (`#e11d48`) is reserved strictly for immediate life safety triggers and Quick Exit affordances. Gateway brand colors (`bkash`, `nagad`, `rocket`) appear only in their branded component contexts and nowhere else.

## Typography

**Display Font:** Plus Jakarta Sans (with fallback `-apple-system, BlinkMacSystemFont, Segoe UI, Roboto, sans-serif`)
**Bengali Body & Display Font:** Hind Siliguri (with fallback `Plus Jakarta Sans, sans-serif`)

**Character:** Modern, clean, highly legible geometric sans-serif paired with optimized Bengali letterforms. Designed for effortless reading under emotional stress and seamless bilingual toggle.

### Hierarchy
- **Display** (ExtraBold 800, `clamp(2rem, 5vw, 3.75rem)`, line-height 1.18): Hero headlines; high-impact landing messages.
- **Headline** (ExtraBold 800, `clamp(1.5rem, 3vw, 2.25rem)`, line-height 1.25): Section headers (e.g. Hotline alert bar, Pillars of protection).
- **Title** (Bold 700, `1.25rem` / `20px`, line-height 1.4): Card titles, modal headers, course titles.
- **Body** (Regular 400 / Medium 500, `0.875rem` / `14px`, line-height 1.6): Paragraph copy, descriptions, case study text.
- **Label** (Bold 700 / ExtraBold 800, `0.75rem` / `12px`, letter-spacing `0.05em`, uppercase): Category tags, emergency hotline badges, status pills.

### Named Rules
**The Dual-Script Harmony Rule.** Every UI element containing text must support both English (*Plus Jakarta Sans*) and Bengali (*Hind Siliguri*) fonts with matching line-heights (`line-height: 1.5–1.6`) to prevent text jump or overflow during language toggling.

## Layout

HAVEN uses a responsive 12-column grid layout with a max container width of `72rem` (`1152px` / `max-w-7xl`).

- **Grid Columns:** 12 columns on desktop (`lg:grid-cols-12`), 2–3 columns on tablet (`md:grid-cols-3`), 1 column on mobile.
- **Margins & Container Padding:** `px-4 sm:px-6 lg:px-8` (16px mobile, 24px tablet, 32px desktop).
- **Vertical Spacing Rhythm:** `py-12` to `py-24` (48px to 96px) between major sections; `gap-6` to `gap-8` (24px to 32px) between grid items.
- **Sticky Navigation:** `h-20` (80px) header bar with `sticky top-0 z-40 backdrop-blur-md`.

## Elevation & Depth

HAVEN uses flat warm surfaces at rest with soft, ambient teal-tinted hover elevations and translucent glassmorphism for overlays.

### Shadow Vocabulary
- **Card Hover Elevation** (`box-shadow: 0 16px 32px -8px rgba(15, 118, 110, 0.08), 0 8px 16px -4px rgba(0, 0, 0, 0.03)`): Soft ambient glow applied on hover with `translateY(-4px)` lift.
- **Light Glass Panel** (`background: rgba(255, 255, 255, 0.85)`, `backdrop-filter: blur(12px)`, `border: 1px solid rgba(226, 232, 240, 0.8)`): Used for floating headers, quick exit bars, and interactive widgets.
- **Dark Glass Panel** (`background: rgba(15, 23, 42, 0.88)`, `backdrop-filter: blur(14px)`, `border: 1px solid rgba(51, 65, 85, 0.6)`): Used for emergency footer banners and dark crisis cards.
- **Emergency Red Pulse** (`animation: pulse-emergency 2s infinite`): Expanding red shadow ring `0 0 0 12px rgba(225, 29, 72, 0)` for immediate emergency SOS buttons.

### Named Rules
**The Sanctuary Layering Rule.** Surfaces rest flat on the warm cream background (`#fdfcf9`). Elevation appears dynamically on hover via teal-tinted ambient glow (`rgba(15, 118, 110, 0.08)`), reserving frosted glass blur (`backdrop-filter: blur(12px)`) for fixed navigation headers and floating crisis overlays.

## Shapes

HAVEN features organic, protective form language with generous border radii and smooth curves.

- **Cards & Modules:** `rounded-3xl` (24px radius) with thin slate borders (`border-slate-200/90`).
- **Primary Buttons & Form Inputs:** `rounded-2xl` (16px radius) or `rounded-xl` (12px radius).
- **Status Pills & Trust Badges:** `rounded-full` (9999px radius).
- **Icons & Brand Containers:** `rounded-2xl` (16px radius) with soft background gradients.

### Named Rules
**The Protective Curve Rule.** Corners are kept soft and rounded (minimum 12px, default 16px–24px) to reinforce a safe, approachable aesthetic. Sharp square corners (`border-radius: 0`) are prohibited on interactive surfaces.

## Components

### Buttons
- **Shape:** Generously rounded `rounded-2xl` (16px radius) or `rounded-xl` (12px radius).
- **Primary Action:** Background `#0f766e` (Sanctuary Teal), text `#ffffff`, padding `16px 28px`, shadow `0 10px 25px -5px rgba(15, 118, 110, 0.25)`. Hover: `#0d9488` with `translateY(-2px)`.
- **Secondary / Ghost:** Background `#ffffff`, text `#1e293b`, border `1px solid rgba(203, 213, 225, 0.8)`, padding `16px 28px`. Hover: `#f8fafc`.
- **Crisis SOS Button:** Background `#e11d48` (Emergency Rose), text `#ffffff`, padding `14px 20px`, `pulse-red` keyframe animation ring.

### Cards / Containers
- **Corner Style:** `rounded-3xl` (24px radius).
- **Background:** `#ffffff` (Pure White) with `border: 1px solid rgba(226, 232, 240, 0.9)`.
- **Hover Treatment:** `translateY(-4px)` upward lift, `box-shadow: 0 16px 32px -8px rgba(15, 118, 110, 0.08)`.
- **Internal Padding:** `32px` desktop (`p-8`), `24px` mobile (`p-6`).

### Emergency Direct-Dial Rows
- **Shape:** `rounded-xl` (12px radius).
- **Background:** Soft rose (`#fff1f2` for 1098), soft slate (`#f1f5f9` for 999), soft amber (`#fffbeb` for 109).
- **Border:** `1px solid` matching hotline severity tint.

### Chat Bubbles
- **User Bubble:** Background `linear-gradient(135deg, #0d9488 0%, #0f766e 100%)`, text `#ffffff`, radius `18px 18px 4px 18px`.
- **Bot Bubble:** Background `#f8fafc`, text `#1e293b`, border `1px solid #e2e8f0`, radius `18px 18px 18px 4px`.
- **Crisis Trigger Bubble:** Background `#fff1f2`, text `#881337`, border `1px solid #fecdd3`, radius `18px 18px 18px 4px`.

### Navigation Header
- **Background:** `rgba(255, 255, 255, 0.9)` with `backdrop-filter: blur(12px)`.
- **Border:** Bottom border `1px solid rgba(226, 232, 240, 0.8)`.
- **Quick Exit Button:** Red gradient `from-rose-600 to-rose-700`, text `#ffffff`, keyframe pulse, `ESC` shortcut handler.

## Do's and Don'ts

### Do:
- **Do** use warm cream background (`#fdfcf9`) for primary content pages to maintain trauma-informed visual comfort.
- **Do** keep card border-radius generous at `24px` (`rounded-3xl`) and button border-radius at `16px` (`rounded-2xl`).
- **Do** ensure every interactive card features smooth hover elevation (`translateY(-4px)` with soft teal shadow `rgba(15, 118, 110, 0.08)`).
- **Do** test all copy layout changes in both English and Bengali (`data-bn` and `data-en` bilingual attributes).
- **Do** maintain the Quick Exit (ESC) button in a high-visibility, fixed/sticky location across all pages.

### Don't:
- **Don't** use stark pure black (`#000000`) for text or backgrounds; use Slate Navy (`#1e293b`) or Midnight Slate (`#0f172a`).
- **Don't** use sharp rectangular corners (`border-radius: 0px`) on cards, buttons, or dialog containers.
- **Don't** use Emergency Rose (`#e11d48`) for non-critical interface elements; reserve it strictly for SOS and hotline emergency triggers.
- **Don't** remove frosted glass backdrop filters (`backdrop-filter: blur(12px)`) from sticky navigation headers.
- **Don't** use cold, clinical grey-blue colors that mimic clinical hospital management software.
