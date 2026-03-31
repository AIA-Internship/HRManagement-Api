## 📂 Project Architecture

### 1. Backend (`HRManagement.Api`)
- **Framework**: .NET 10.0 (Core)
- **Pattern**: Clean Architecture using **MediatR / CQRS**.
- **Validation**: Strict domain validation with **FluentValidation**.
- **Persistence**: **Entity Framework Core** with Repository Pattern.
- **Database**: SQL Server/LocalDB with automated schema generation (`EnsureCreated`).
- **Data Seeding**: Automated master data and user accounts on initial run.

### 2. Frontend (`HRManagement.Web`)
- **Framework**: ASP.NET Core Razor Pages.
- **UI Engine**: Metronic v8.3.3 + AIA Design System.
- **Project Structure**:
    - `Pages/Account/`: Core Authentication & Security.
    - `Pages/Dashboard/`: Central Application Hub.
    - `Pages/Modules/`: Independent Business Modules (Timesheet, Profile).
    - `Pages/General/`: Supporting System Pages (Error, Privacy).
    - `Pages/Shared/`: Global UI Components.

---

## 🛠️ Backend Implementation Details

To maintain a clean and scalable backend, the API follows a strict layer separation:

| Layer | Responsibility | Key Folder |
| :--- | :--- | :--- |
| **Domain** | Core entities, Enums, and Business Models. | `Domain/Models/Tables` |
| **Application** | Commands, Queries, DTOs, and Request Handlers. | `Application/` |
| **Repositories** | DB Context, Migrations/Seeders, and Base Logic. | `Repositories/` |
| **Controllers** | API Endpoints (Restful). | `Controllers/` |

---

## 🎨 UI Component Architecture (FRONTEND CORE)

To ensure zero conflicts across multiple teams, we follow a strict **"Folder-in-Folder"** modular system.

### 📂 File Locations (The Global Kit)

| Category | HTML Path (`Pages/Shared/`) | Asset Path (`wwwroot/apps/shared/`) |
| :--- | :--- | :--- |
| **Core Identity** | - | `identity/brand-identity-tokens.css` |
| **Global Logic** | - | **`core/shared-global-logic.js`** |
| **Buttons** | `Buttons/_StandardButton.cshtml` | `buttons/standard-action-buttons.css` |
| **Layout** | `Layout/_PartialHeader.cshtml` | `layout/base-layout-styling.css` |
| **Modals** | `Modals/_ActionModal.cshtml` | `modals/confirmation-action-modals.css` |
| **Badges / Toasts** | - | `badges/status-badges.css` & `notifications/` |
| **Forms / Dropdown** | - | `forms/common-input-dropdowns.css` |
| **Footers** | - | `footers/standard-corporate-footers.css` |
| **Validations** | `Layout/_ValidationScriptsPartial.cshtml` | - |

---

### 🛠️ How to use Global Buttons
Call the standardized Razor Partial:

```html
@await Html.PartialAsync("Shared/Buttons/_StandardButton", new { 
    Type = "primary",  // primary, ghost, info, nav
    Title = "Save Record", 
    Icon = "check2-circle" 
})
```

**Variants:** `primary` (Red), `ghost` (White/Border), `info` (Soft Red), `nav` (Circle).

---

## 🔑 Default Credentials (Seeder)
Testing accounts for Brandon and Owen:
- **Supervisor**: `brandon@aia.com` / `AdminPass123!`
- **Intern**: `owen@aia.com` / `WorkerPass123!`

---

## 🛠️ Guidelines
1.  **Design Safety**: Do not use `text-transform: uppercase` on buttons to maintain the new modern aesthetic.
2.  **Icon Visibility**: Ensure icons on red buttons prioritize white coloring (`color: #fff !important`).
3.  **Strict Path Consistency**: Feature-specific logic MUST remain in **`apps/modules/[feature]/[js/css]/`**.
4.  **No Direct Modification**: Do not add logic to `core/shared.js` unless it's truly global.
5.  **CORS Issues Fix, (didalem Program.cs Backend ganti withOrigin jadi local kalian dulu).
