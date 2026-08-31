# HAVEN Codebase Audit & Implementation Action Plan
**Compliance & Requirements Mapping based on CSE 3224 (Report 03) & CSE 3200 (Project Proposal)**

---

## Executive Summary

This document provides an in-depth audit of the **HAVEN** platform against the academic specifications set forth in:
1. **CSE 3224 (Report 03)**: Information System Design & Software Engineering Lab Report (Subsystems S1–S7, Use Cases UC-01 to UC-25, Cost-Benefit Analysis).
2. **CSE 3200 (Project Proposal)**: Software Development - V Report (Functional Requirements FR-1 to FR-12, 13 Entity ER Diagram, SDG Alignments).

---

## Part 1: Audit of Functional Requirements (FR-1 – FR-12)

| ID | Specification Description | Implementation Status | What to Do / What to Change |
| :--- | :--- | :--- | :--- |
| **FR-1** | Account registration/login under Individual or Parent/Guardian modes with age-aware consent checks. | **PARTIAL** | Add `UserType` (Individual vs Parent) and `Age` fields to `User.cs` entity and `RegisterViewModel.cs`. Update `Register.cshtml` to offer account type selection. |
| **FR-2** | Parent/Guardian account creates and manages linked Child Profiles without collecting unnecessary personal data. | **MISSING** | Create `ChildProfile.cs` entity linked to `User`. Add parent dashboard actions in `AccountController.cs` for managing child aliases & age brackets. |
| **FR-3** | Quick-Exit / Hide-Mode safety control on sensitive pages immediately redirecting browser and clearing storage. | **IMPLEMENTED** | Implemented via `window.havenQuickExit()` in `site.js` and `ESC` key global event handler. |
| **FR-4** | Self-paced enrollment in free and low-cost paid courses with per-module progress tracking. | **PARTIAL** | Move course mock data from `HavenDataStore.cs` into EF Core database entities (`Course`, `CourseModule`, `Enrollment`). Persist completion status. |
| **FR-5** | Enable booking, rescheduling, and cancellation of appointments with verified clinicians. | **PARTIAL** | Add `Appointment.cs` EF Core entity. Build appointment booking, slot locking, rescheduling, and cancellation actions in `TherapyController.cs`. |
| **FR-6** | AI safety chatbot for psychoeducation and triage in Bangla and English. | **IMPLEMENTED** | Gemini API integration in `HotlineController.cs` supporting bilingual conversation. |
| **FR-7** | Risk-classification layer detecting acute danger with automatic emergency contact surfacing (109/999/Haven Hotline) and human handoff. | **IMPLEMENTED** | Keyword risk classification in `HotlineController.cs` triggering `_CrisisEscalationModal.cshtml`. |
| **FR-8** | Support anonymous, low-data hotline chat via SignalR without mandatory account creation. | **MISSING** | Add `HotlineHub.cs` SignalR hub for real-time WebSocket communication between citizens and hotline operators. |
| **FR-9** | Allow clinicians and authors to create articles and safety courses. | **PARTIAL** | Created `Article.cs` EF Core entity. Add `ArticlesController.cs`, article reader view, and clinician publishing editor (FR-9). |
| **FR-10** | Admin credential verification tools to review professional licence/ID uploads before elevating account status. | **MISSING** | Create `ProfessionalProfile.cs` entity with `LicenseNo`, `LicenseDocumentUrl`, `ApprovalStatus` and Admin review UI in `AdminController.cs`. |
| **FR-11** | Process payments and charity donations through bKash, Nagad with optional Hall of Fame opt-in. | **IMPLEMENTED** | `DonateController.cs`, `_PaymentModal.cshtml`, and `PaymentViewModel.cs`. Connect to `Payment.cs` entity. |
| **FR-12** | Enforce server-side role-based access control (RBAC) & audit logs for every access to sensitive records. | **PARTIAL** | Add `AdminAuditLog.cs` entity and decorate admin/professional endpoints with `[Authorize(Roles = "Admin,Professional")]`. |

---

## Part 2: Subsystems Audit (S1 – S7) & Use Case Mapping (UC-01 – UC-25)

### Subsystem 1: Authentication & Account Management
- **UC-01 (Register Account)**: Needs Individual vs Parent mode toggle.
- **UC-02 (Init Safety Prefs)**: Needs guest session initial privacy level & panic button customization.
- **UC-03 (Login) & UC-04 (Verify Credentials)**: Fully implemented via `AuthService.cs` and `AccountController.cs`.
- **UC-05 (Logout)**: Implemented.
- **UC-06 (Reset Password)**: Missing secure password reset token generation & email/SMS dispatch.
- **UC-07 (Manage Profile)**: Needs linked child profiles management (FR-2).

### Subsystem 2: Educational Courses & Learning Engine
- **UC-08 (Browse Courses) & UC-09 (Filter by Topic)**: Implemented in `CoursesController.cs`.
- **UC-10 (View Safety Content)**: Implemented in `Courses/Details.cshtml`.
- **UC-11 (Enroll in Course) & UC-12 (Track Learning Progress)**: Currently client-side. Must connect to database `Enrollment` & `CourseModule` tables.

### Subsystem 3: Crisis Triage & Emergency Response
- **UC-13 (Access Hotlines)**: Implemented in `HotlineController.cs` & `_CrisisEscalationModal.cshtml`.
- **UC-14 (Trigger Quick Exit)**: Implemented in `site.js`.

### Subsystem 4: Professional Care & Appointment Booking
- **UC-15 (Search Therapists)**: Implemented in `TherapyController.cs`.
- **UC-16 (Book Appointment) & UC-17 (Confirm Schedule)**: Needs `Appointment.cs` database persistence, slot locking, and masked notification.

### Subsystem 5: AI Safety Assistant
- **UC-18 (AI Safety Chat) & UC-19 (Triage Assessment)**: Implemented via Gemini API integration in `HotlineController.cs`.

### Subsystem 6: Anonymous Community & Reporting
- **UC-20 (Post Anonymous Query)**: Missing peer support forum with randomized aliases.
- **UC-21 (Report Safety Incident)**: Missing encrypted safety incident report submission form for grooming or abuse.

### Subsystem 7: Admin & System Oversight
- **UC-22 (Manage Course Catalog)**: Missing Admin CRUD for course modules.
- **UC-23 (Review Reports)**: Missing Admin audit dashboard for reported safety incidents.
- **UC-24 (Verify Credentials)**: Missing Admin license review portal for therapists.
- **UC-25 (Monitor System Health)**: Missing `AdminAuditLog.cs` background log tracking.

---

## Part 3: Database Entity Expansion Plan (13 Entities)

Currently, `HavenDbContext.cs` only contains `DbSet<User>`. To satisfy the Project Proposal's 13-Entity ER Diagram, the following model classes and DbSets must be added:

1. **`User`** *(Identity & Access)* - Extend with `Age`, `UserType` (Parent/Individual).
2. **`ChildProfile`** *(Identity & Access)* - Parent-linked child profiles.
3. **`ProfessionalProfile`** *(Identity & Access)* - License number, document URL, approval status (`Pending`, `Approved`, `Rejected`), hourly rate.
4. **`Course`** *(Content & Learning)* - Category, price, PWYW flag, approval status.
5. **`CourseModule`** *(Content & Learning)* - Step number, title, duration.
6. **`Enrollment`** *(Content & Learning)* - User ID, Course ID, completed modules count, progress percentage.
7. **`Appointment`** *(Care Management)* - User ID, Professional ID, scheduled date/time, status (`Scheduled`, `Completed`, `Cancelled`), channel.
8. **`SupportSession`** *(Care Management)* - User ID, responder ID, session type, escalation status.
9. **`EmergencyResource`** *(Care Management)* - Hotline name, district, contact number, type.
10. **`Article`** *(Content & Learning)* - Author ID, title, content, approval status.
11. **`Payment`** *(Payments & Feedback)* - User ID, amount, gateway (bKash/Nagad/Rocket), transaction ID, Hall of Fame opt-in, display name.
12. **`CrisisAlert`** *(Trust & Safety)* - User ID, trigger keyword, severity level, timestamp.
13. **`AdminAuditLog`** *(Trust & Safety)* - Admin User ID, action type, target resource, timestamp.

---

## Part 4: Step-by-Step Implementation Roadmap

### Step 1: EF Core Models & Migration
- Create entity model files under `haven/Models/`.
- Update `HavenDbContext.cs` with all 13 DbSets and fluent configuration.
- Run EF Core Migration (`dotnet ef migrations add ExpandHavenEntities`).

### Step 2: SignalR Real-Time Hotline Hub (FR-8)
- Create `haven/Hubs/HotlineHub.cs` handling anonymous WebSocket chat connections.
- Map SignalR Hub endpoint `/hubs/hotline` in `Program.cs`.

### Step 3: Controller & View Extensions
- **`AccountController.cs`**: Add Parent mode & Child Profile management views.
- **`CoursesController.cs`**: Connect enrollment & progress checkmarks to `Enrollment` database records.
- **`ArticlesController.cs` (FR-9)**: Build Clinician Article Publishing Portal & Psychoeducation Reader Hub (`Views/Articles/Index.cshtml` & `Details.cshtml`).
- **`TherapyController.cs`**: Connect appointment booking modal to `Appointment` database records with cancellation & rescheduling logic.
- **`AdminController.cs`**: Build Admin portal at `/Admin` for credential verification (FR-10), course management (UC-22), incident report audit (UC-23), and system logs (FR-12).
- **`CommunityController.cs`**: Build anonymous peer forum (UC-20) and safety incident reporting form (UC-21).

### Step 4: Verification & Handoff
- Run `dotnet build` to ensure 0 compilation errors.
- Test parent/child registration, database-backed course progress, appointment scheduling, and SignalR live chat.
