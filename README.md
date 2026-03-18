## Coach Tracker – overview

This repo is a small full‑stack app for tracking training, nutrition, and daily programs.

### Backend (ASP.NET + SQLite)

- `CoachTracker.api` – single Web API project
- Storage: `coachtracker.db` (SQLite, EF Core)
- Main domains:
  - **Exercises** – library of lifts with default sets
  - **Splits** – training splits and days, plannable onto calendar (`PlannedCalendarDays`)
  - **Workouts** – finalized daily workouts + sets (`DailyWorkouts`, `WorkoutSets`)
  - **Nutrition** – per‑day food logs (`NutritionFoodLogs`) and usual foods
  - **Admin DB** – simple endpoints for browsing tables / running SQL (`AdminDbController`)

### Frontend (Angular standalone + Tailwind)

- Entry: `frontend/src/app/app.html` + `app.routes.ts`
- Pages:
  - `Today` – log sets, see today’s program, session notes, diet summary, micronutrient panel
  - `Plan` – pick split/day, see day exercises, add extra exercises, draft program → send to Today
  - `Nutrition` – USDA food search, micronutrient details, add foods to today & manage usual foods
  - `Database` – introspect SQLite tables, quick exercises editor, ad‑hoc SQL runner

### Data flows (high level)

- **Today ↔ Workouts** – `Finalize & Save Workout` posts the current local draft to `/api/workouts/finalize`.
- **Plan → Today** – choosing a split/day + extras builds a draft program, stored in `localStorage` and surfaced on Today.
- **Calendar ↔ Today** – calendar assignments (planned split for a calendar date) feed Today’s “schedule” when no custom program is selected; can be cleared from Today.
- **Nutrition → Today** – foods added via Nutrition page are logged to `NutritionFoodLogs` and aggregated on Today for macros + micronutrient progress.

This README is intentionally short; the code is the source of truth for detailed behavior.
