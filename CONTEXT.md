# Coach Tracker Context

**Date Updated:** 2026-03-16

This file is intended to provide immediate context, architecture decisions, and error history for any subsequent AI agents working on this project.

## 1. What Errors Were Encountered?

- **SQLite Database Locking & Pending Changes:** When trying to update the EF core models with local seeded data, the system threw `Microsoft.EntityFrameworkCore.Migrations.PendingModelChangesWarning` and `SQLite Error 19: 'UNIQUE constraint failed: Exercises.Id'`. The database file was frequently locked by IDEs like DataGrip or running dotnet instances, making migration rollbacks impossible.
- **PrimeNG Bundle Size & Component Errors:** When first integrating PrimeNG v21, the angular compiler threw max bundle budget limits exceeded. Furthermore, legacy PrimeNG paths like `primeng/dropdown` were failing as version 21 changed its routing to `primeng/select`.
- **Backend Build Failures on Refactor:** Re-designing the Workout entities broke the frontend-facing API controllers heavily as outdated `WorkoutSession` models were missing.

## 2. What Were The Solutions?

- **Database Hard Reset:** We implemented forced process killing (`Stop-Process`), wiped the `coachtracker.db` and `-wal` files manually, deleted the old migrations via `dotnet ef migrations remove`, and ran a completely fresh `add InitialSchema` and `database update` to ensure EF generated a flawless map of the new entities.
- **Frontend Refactor:** Increased Angular budget limits inside `angular.json` to ~2MB. Replaced native tables and inputs across `plan.ts` and `today.ts` with `<p-table>`, `<p-select>`, `<p-card>`, and `<p-button>`, utilizing the Aura visual theme.
- **Controller Rewrites:** We deleted all instances of `WorkoutExerciseLog` and rewrote `WorkoutsController.cs` to use purely a `DailyWorkout` model paired with an in-memory batch payload route (`/finalize`).

## 3. What Has Been Changed So Far? (Current Architecture)

The system revolves around three core pillars for strength training:

### A. The Global Exercise Dictionary (`Models/Exercise.cs`)
A unified entity dictionary defining standard science-based lifts. It tracks metadata like `PrimaryMuscle`, `MovementPatternTag`, `Equipment`, and `DefaultPlannedSets`.

### B. The Setup "Shells" (`Models/SplitDay.cs`)
The user defines their macro cycle (e.g., Push / Pull / Legs). A Split Day simply holds relational order indices mapping to the Global Exercises. It does *not* contain set data yet.

### C. The Daily Draft & Finalize Workflow (`today.ts` & `WorkoutsController.cs`)
This is the most critical shift. 
- **The Local View:** When the user is on the `today.html` screen, they are interacting purely with a localized, in-memory "Soup" array (`this.todayWorkout.sets`). Every time they log a set or add a brand new Custom Exercise, it is **only** cached in their browser. 
- **The Finalize Hook:** When the user finishes their gym session, they click `Finalize & Save Workout`. This fires the `POST /api/workouts/finalize` endpoint, sending the full localized soup payload.
- **Auto-binding:** The API parses the payload, discovers any exercises that don't match the universal Dictionary, quietly registers them natively in the Dictionary, binds their core ID, and then persists the exact `WorkoutSet` entries to the SQLite DB under the `DailyWorkout` date structure.

## Where To Go Next
Everything currently functions end-to-end perfectly. You can spin up both `dotnet run` (port 5106) and `ng serve` (port 4200) simultaneously to verify. The user will likely want to build out Historical tracking charts, an interactive Calendar view on the frontend to visualize past `DailyWorkout` records, or Nutrition aggregations next.

---

## 4. Nutrition & Tailwind UI (March 2026)

- **USDA Search Flow:** Frontend `NutritionComponent` calls `FoodApiService.searchFoods()` → `POST /api/foods/search` → `FoodsController` → `UsdaFoodService.SearchFoodsAsync`, which proxies to USDA `foods/search`. The search DTO now includes **headline macros only** (`Calories`, `Protein`, `Carbs`, `Fat`) so the results list can show kcal/macros without a detail call; all other fields (fiber, sugar, sodium, potassium, micronutrients) live in `FoodDetailDto.Summary` and `FoodDetailDto.Micronutrients`.
- **Food Detail Flow:** Clicking a result calls `FoodApiService.getFoodDetail(fdcId)` → `GET /api/foods/{fdcId}` → `UsdaFoodService.GetFoodDetailAsync` / `MapFoodDetail`. This builds a `FoodDetailDto` with a `Summary` (including `Potassium`) and grouped micronutrients using `NutrientMapper`. The Nutrition page shows this in the right‑hand “Food details” tile.
- **Nutrition Logging & Usuals:** `NutritionStore` talks to `NutritionLogController` (`/api/nutrition/...`) to persist logged foods per `dayKey` and maintain a `NutritionUsuals` table. The Nutrition page can add the current USDA selection to today’s log or star it as a “Usual”; usuals are rendered as a grid of quick‑add pills under the details tile.
- **Tailwind UI:** PrimeNG is being phased out in favor of TailwindCSS 3. The Nutrition page is now a two‑column Tailwind layout (results on the left; food details and usuals stacked on the right) with a sharp, high‑contrast aesthetic that can be iterated toward a TempleOS‑style look across the app.
