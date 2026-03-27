# 🏢 AIA Timesheet Management System (v1.2) - Final Modular Edition

Professional enterprise solution with a **Modular Architectural Framework** designed for high scalability and effortless maintenance.

---

## 📂 Project Architecture

### 1. Backend (HRManagement.Api)
- **Framework**: .NET 8 Web API.
- **Pattern**: Clean Architecture (MediatR/CQRS).

### 2. Frontend (HRManagement.Web)
- **Framework**: ASP.NET Core Razor Pages.
- **UI Engine**: Metronic v8.3.3.

---

## 🎨 UI Component Architecture (COMPLETE STRUCTURE)

To ensure zero conflicts across multiple teams, we follow a strict **"Folder-in-Folder"** modular system.

### 📂 File Locations (The Global Kit)

| Category | HTML Path (`Pages/Shared/`) | Asset Path (`wwwroot/apps/shared/`) |
| :--- | :--- | :--- |
| **Core Identity** | - | `identity/brand-tokens.css` |
| **Global Logic** | - | **`core/shared.js`** |
| **Buttons** | `Buttons/_ButtonAia.cshtml` | `buttons/buttons.css` |
| **Layout** | `Layout/_PartialHeader.cshtml` | `layout/layout-components.css` |
| **Modals** | `Modals/_PartialModal.cshtml` | - |
| **Badges / Toasts** | - | `badges/` & `notifications/` |
| **Forms / Dropdown** | - | `forms/dropdowns.css` |
| **Validations** | `Layout/_ValidationScriptsPartial.cshtml` | - |

---

### 🛠️ How to use Global Buttons
Call the standardized Razor Partial:

```html
<partial name="Buttons/_ButtonAia" model='new { Type = "primary", Title = "SAVE", Icon = "bi-save" }' />
```

**Variants:** `primary` (Red), `ghost` (White/Border), `info` (Soft Red), `nav` (Circle).

---

## 🛠️ Maintenance & Safety Guidelines
1.  ** डिजाइन सुरक्षित (Design Safety)**: Do not change visual CSS properties (colors, radius, shadows) to maintain verified brand identity.
2.  **No Direct Modification**: Do not add logic to `core/shared.js` unless it's global; use module-specific files.
3.  **Strict Path Consistency**: Feature-specific logic remains in **`apps/modules/[feature]/[js/css]/`**.
