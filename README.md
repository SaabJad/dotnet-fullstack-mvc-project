# MVCIDENTITYDEMO — Secure E‑Commerce Demo (Razor Pages + MVC)

This repository is a .NET 8 Razor Pages / MVC sample e‑commerce application with an emphasis on secure design and hardening. The codebase demonstrates authentication, role based authorization, secure file uploads, auditing, request validation and other defensive measures implemented during today's work session.

## Project Overview

- Project: `MVCIDENTITYDEMO`
- Target: .NET 8
- Patterns: Razor Pages + MVC controllers, EF Core (Pomelo MySQL provider), ASP.NET Core Identity
- Purpose: demo e‑commerce features with security-focused enhancements for authentication, authorization, file handling and auditing.

## Major Security and Functionality Changes (what we did today)

1. Authorization & Rate Limiting
   - Added authorization policies (`AdminOnly`, `RequireAuth`, `CanManageUsers`) and applied role restrictions to relevant controllers (Products, admin area).
   - Added basic IP rate limiting configuration (memory cache + general rule) to mitigate brute force attempts.

2. Audit Logging
   - Created `Models/AuditLog` and registered it in the EF Core model.
   - Implemented `IAuditLogService` / `AuditLogService` to persist important security events (user management actions, file uploads/downloads, etc.).
   - Admin area includes UI to view audit logs under `Areas/Admin/Views/UserManagement/AuditLogs.cshtml`.

3. Secure File Uploads
   - Added `UploadedFile` entity and `UploadedFiles` DbSet.
   - Implemented `IFileUploadService` and `FileUploadService`:
     - Validates extension and reported MIME type.
     - Uses ImageSharp to validate/process images (EXIF removal, resizing to max 1920x1080).
     - Saves files to `Uploads/{FileType}` with GUID stored filename to prevent collisions.
     - Basic heuristic malware/unsafe-content checks (header patterns). In production integrate a real AV engine (ClamAV/Defender/VirusTotal API).
   - Exposed `FileUploadController` and a Razor view at `Views/FileUpload/Index.cshtml` for authenticated users.
   - Service registered in DI (`builder.Services.AddScoped<IFileUploadService, FileUploadService>()`).

4. Request Validation & Input Hardening
   - Added `RequestValidationMiddleware` to inspect query and form inputs for common SQL injection / XSS / path traversal patterns. Middleware returns 400 when suspicious content is detected.
   - Added request size limits and form configuration (Kestrel limits + `FormOptions`) to prevent large payload attacks.
   - Ensured anti‑forgery protection on POST endpoints (`[ValidateAntiForgeryToken]`) and HTML forms include `@Html.AntiForgeryToken()`.

5. Model Validation
   - Added data annotations to models (`Product`, `Category`, `Order`, `ApplicationUser`) to enforce server side validation and reduce malformed data entering the system.

6. Admin Area
   - Created an `Areas/Admin` with `UserManagementController` and views for listing users, managing roles and viewing audit logs.
   - Administrative actions are audited.

7. Misc
   - Registered routes including area routing.
   - Seeded initial roles and users (`admin@example.com` / `Admin123!` seeded as Admin).

## How to run

Prerequisites
- .NET 8 SDK
- MySQL server (or adjust connection string to your DB)

Quick start
1. Update connection string in `appsettings.json` if needed (`ConnectionStrings:Conx`).
2. Apply EF migrations and update DB:
   - `dotnet ef database update` (or use Package Manager Console: `Update-Database`).
   - Migrations created during this session include audit and uploaded files tables.
3. Run the app:
   - `dotnet run` or use Visual Studio (F5).


## Upload tests (test cases implemented / recommended)

These tests reflect the test scenarios used today. Use the `/FileUpload` page (authenticated) to exercise them.

- TEST 1: Valid Image Upload
  - Upload a `.jpg` or `.png` under 5MB and `FileType=ProductImage`.
  - Expect success; file stored at `Uploads/ProductImage/{GUID}.ext`; DB row in `UploadedFiles`; audit entry in `AuditLogs`.

- TEST 2: Invalid Extension
  - Attempt to upload `.exe` / `.bat` / `.sh` — service rejects with "File type is not allowed".

- TEST 3: MIME Type Spoofing
  - Rename an executable to `.jpg` and upload — ImageSharp validation or MIME checks should reject it.

- TEST 4: File Too Large
  - Upload a file > 10MB — request will be rejected by size checks.

- TEST 5: Malicious Content
  - Upload a file containing executable headers (e.g. `MZ` at start); the heuristic scanner flags it as unsafe. (Production: integrate AV solution.)

- TEST 6: Unauthorized Download
  - User A uploads file; User B attempts download — should be rejected (authorization check) and produce 404/Unauthorized.

- TEST 7: Image Processing
  - Upload image larger than 1920x1080 and verify it is resized; EXIF metadata should be removed.

## Important notes & production considerations

- Malware scanning in the sample is a simple heuristic and not suitable for production. Integrate a real scanner (ClamAV, Defender or third‑party API) and block/queue suspicious files.
- Content detection: the sample uses ImageSharp and `IFormFile.ContentType`. For stronger content‑based mime detection, add a robust signature library.
- Anti‑virus & sandboxing: consider storing uploads in an isolated service or object storage and scanning asynchronously before marking files as available.
- Rate limiting: current setup is in-memory for demo. For production use a distributed rate‑limit store (Redis) and fine tune rules per endpoint.
- Logging & retention: secure audit logs (immutable storage, restricted access) and implement retention policies.
- Validation middleware: tune patterns to reduce false positives and log blocked events for analysis.


## Where to look in the codebase

- `Program.cs` — DI, middleware, routing, limits
- `Data/ApplicationDbContext.cs` — DbSets and EF configuration
- `Models/UploadedFile.cs`, `AuditLog.cs`, `ApplicationUser.cs` — relevant models
- `Services/FileUploadService.cs`, `IFileUploadService.cs` — upload logic
- `Services/AuditLogService.cs` — audit persistence
- `Middleware/RequestValidationMiddleware.cs` — input inspection
- `Controllers/FileUploadController.cs` — upload/download/delete endpoints
- `Areas/Admin/Controllers/UserManagementController.cs` and `Areas/Admin/Views` — admin UI

---


