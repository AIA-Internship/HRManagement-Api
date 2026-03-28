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
- **Modular Assets**: Folder-in-Folder modular system for safe collaboration.

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
| **Core Identity** | - | `identity/brand-tokens.css` |
| **Global Logic** | - | **`core/shared.js`** |
| **Buttons** | `Buttons/_ButtonAia.cshtml` | `buttons/buttons.css` |
| **Layout** | `Layout/_PartialHeader.cshtml` | `layout/layout-components.css` |
| **Modals** | `Modals/_PartialModal.cshtml` | `modals/aia-modals.css` |
| **Badges / Toasts** | - | `badges/` & `notifications/` |
| **Forms / Dropdown** | - | `forms/dropdowns.css` |
| **Footers** | - | `footers/aia-footers.css` |
| **Validations** | `Layout/_ValidationScriptsPartial.cshtml` | - |

---

### 🛠️ How to use Global Buttons
Call the standardized Razor Partial:

```html
@await Html.PartialAsync("Shared/Buttons/_ButtonAia", new { 
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