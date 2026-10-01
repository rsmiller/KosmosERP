# Known bugs

App bugs found while building the Playwright e2e suite (`UI/e2e`) and the DB seeder. Each entry stands alone, so it can be handed to a separate agent or developer: where the bug is, how it shows, the root cause, evidence, a suggested fix and when it counts as done.

**Conventions for whoever picks one up**
- Keep each fix scoped to its bug. Don't refactor neighbouring code.
- The e2e suite runs from `UI/`: `npm run test:e2e` (build and test), `npx playwright test <path>` (one spec), `npm run test:e2e:typecheck`. The API is mocked in the browser, so no backend is needed. See `UI/README.md` → "End-to-end tests".
- Where a bug is pinned by a test (`knownBug(...)` in `UI/e2e/tests/known-bugs.spec.ts`, `fixme` on a catalog entry, or an allowlist entry), the fix isn't done until that marker is removed and the test passes. To see a pinned bug's real failure, run `E2E_SHOW_KNOWN_BUGS=1 npx playwright test tests/known-bugs`.
- Run `npx playwright test --repeat-each=2 --retries=0` before calling a UI fix done.

## Index

| ID | Title | Area | Severity | Status |
| --- | --- | --- | --- | --- |
| [BUG-001](#bug-001) | Admin → Settings and Adjustments redirect every user away | UI / permissions | High | Fixed (uncommitted) |
| [BUG-002](#bug-002) | Sales orders can't be created: `order_headers` schema defects | DB / EF migrations | High | Fixed (uncommitted) |
| [BUG-003](#bug-003) | Production status dropdowns use a lookup id the backend never populates | UI ↔ API data | Medium | Fixed (uncommitted) |
| [BUG-004](#bug-004) | Freight carrier dropdown uses a lookup id the seeder never populates | UI ↔ seeder data | Medium | Fixed (uncommitted) |
| [BUG-005](#bug-005) | React hydration error #418 on most production page loads | UI / SSR styling | Medium | Fixed (uncommitted) |
| [BUG-006](#bug-006) | "Delete Record" on new-record forms crashes (no handler) | UI | Medium | Fixed (uncommitted) |
| [BUG-007](#bug-007) | Lookup comboboxes filter on the hidden key, not the visible label | UI / components | Low | Open |
| [BUG-008](#bug-008) | Login reports "incorrect password" when the API is unreachable | UI / auth | Low | Open |
| [BUG-009](#bug-009) | Seeded users can't log in (unhashable seed password) | Dev tooling / seeder | Medium | Open |
| [BUG-010](#bug-010) | `CreditMemoHeaderDto` doesn't declare the `guid` it carries | UI models | Low | Open |
| [BUG-011](#bug-011) | Date-only values show one day early in US time zones | UI | Medium | Fixed (uncommitted) |
| [BUG-012](#bug-012) | Shipment pages show the raw shipping-method key | UI | Low | Open |
| [BUG-013](#bug-013) | `/erp/ar/new` is a stub with hard-coded sample data | UI | Low | Open, needs decision |
| [BUG-014](#bug-014) | Form controls with no accessible name | UI / accessibility | Low | Open |
| [BUG-015](#bug-015) | Credit memo edit page is titled "New Credit Memo" | UI | Low | Open |
| [BUG-016](#bug-016) | Combobox options inside modal dialogs are hidden from assistive tech | UI / accessibility | Medium | Fixed (uncommitted) |
| [BUG-017](#bug-017) | Shipment line "Delete" button marks the line shipped | UI | High | Fixed (uncommitted) |
| [BUG-018](#bug-018) | Non-taxable customers' invoice lines default to taxable | UI | High | Fixed (uncommitted) |
| [BUG-019](#bug-019) | Invoicing crashes unless payment terms are named "NETxx" | UI | High | Fixed (uncommitted) |
| [BUG-020](#bug-020) | Journal entry and chart of accounts pages pass props PageActions ignores | UI | Medium | Open |
| [BUG-021](#bug-021) | Journal entry "Account" column is a free-text internal id | UI / UX | Medium | Open |
| [BUG-022](#bug-022) | BOM item dialog is titled "Add Address" | UI | Low | Open |
| [BUG-023](#bug-023) | New Product can't be saved after a direct load or refresh | UI | High | Fixed (uncommitted) |
| [BUG-024](#bug-024) | Production order edits are never saved | UI | High | Fixed (uncommitted) |
| [BUG-025](#bug-025) | Completed production orders stay editable | UI | Low | Open |
| [BUG-026](#bug-026) | API endpoints with no authorization (Settings writable anonymously) | API / security | High | Fixed (uncommitted) |
| [BUG-027](#bug-027) | Grid buttons send an empty token after a direct load | UI | Medium | Fixed (uncommitted) |

Also see [Needs verification](#needs-verification) for suspected issues that haven't been confirmed.

A few bugs are already fixed in the working tree but not committed yet. They're listed under [Fixed (uncommitted)](#fixed-uncommitted) at the end.

---

<a id="bug-001"></a>
## BUG-001: Admin → Settings and Adjustments redirect every user away

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** High. Nobody, including admins, can reach company settings or inventory adjustments.
- **Where:**
  - `UI/app/erp/admin/settings/page.tsx` (~line 58)
  - `UI/app/erp/admin/adjustments/page.tsx` (~line 55)
- **Symptom:** open `/erp/admin/settings` or `/erp/admin/adjustments` as any user. The page flashes briefly, then redirects to `/erp`.
- **Root cause:** both pages call

  ```ts
  permissionsService.HasPermission(ERPModules.Admin, '', realmRoles)
  ```

  with an **empty** permission. `HasPermission` builds `module + "_" + permission` = `"admin_"` (`UI/services/permissions-service.tsx`). The role mapping (`UI/lib/auth/role-mapping.ts`) only produces `admin_read`, `admin_write`, `admin_edit` and `admin_delete`, so the check never passes. The admin landing page (`app/erp/admin/page.tsx`) correctly uses `ERPModulePermission.Read`.
- **Evidence:** `UI/e2e/catalog.ts` entries `admin/settings` and `admin/adjustments` are marked `fixme` with this reason, and `tests/pages/render.spec.ts` skips them.
- **Suggested fix:** use `ERPModulePermission.Read` for access. Gate the save and adjust actions on `ERPModulePermission.Edit` / `Write` the way other pages do (e.g. `setHasEditPermission` on Settings is currently always `true`). Then grep `app/erp` for other `HasPermission(` calls with `''` (only these two at time of writing).
- **Done when:**
  - The `fixme` fields are removed from both catalog entries.
  - `npx playwright test tests/pages` passes: the pages render and stay on their URL.

<a id="bug-002"></a>
## BUG-002: Sales orders can't be created: `order_headers` schema defects

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference; its first root cause turned out to be wrong (`order_number` wasn't the culprit).

- **Severity:** High. The live app can't insert a sales order against a database built from the migrations.
- **Where:**
  - `Shared/KosmosERP.Database/Configurations/OrderHeaderConfiguration.cs` (line 14)
  - The Payment ↔ OrderHeader relationship configuration
  - `Shared/KosmosERP.Database/Migrations/20260918152029_InitialDatabase.cs`
- **Symptom:** inserting into `order_headers` fails:
  - first with `Field 'id' doesn't have a default value`
  - then on the foreign key `FK_order_headers_payments_id`
- **Root cause:**
  1. `order_number` is configured `.HasDefaultValue(10000).ValueGeneratedOnAdd()`. MySQL allows one AUTO_INCREMENT column per table, so the migration emitted `order_headers.id` **without** identity. It's the only table whose primary key lost AUTO_INCREMENT.
  2. `FK_order_headers_payments_id` points the wrong way (`order_headers.id` → `payments.order_header_id`). An order can't exist until a payment referencing it exists, which is impossible.
- **Evidence:** found 2026-09-22 while building `Tools/KosmosERP.Seeder`. The seeder works around it by running with `FOREIGN_KEY_CHECKS=0` and `ALTER`ing `order_headers.id` to AUTO_INCREMENT (see `Tools/KosmosERP.Seeder/Program.cs`). The configuration line is unchanged at time of writing.
- **Suggested fix:**
  - Drop `.ValueGeneratedOnAdd()` from `order_number`, and generate order numbers in the business layer or by sequence.
  - Fix the Payment ↔ OrderHeader relationship so the FK is `payments.order_header_id → order_headers.id`.
  - Add a corrective migration (`dotnet ef migrations add ... --project Shared/KosmosERP.Database`).
  - Once applied, remove the seeder workarounds.
- **Done when:**
  - On a fresh DB (`dotnet ef database update`), a sales order can be created through the API with FK checks on.
  - The seeder runs without its `ALTER` / `FOREIGN_KEY_CHECKS=0` workaround.
  - `dotnet test KosmosERP.sln` passes.

<a id="bug-003"></a>
## BUG-003: Production status dropdowns use a lookup id the backend never populates

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** Medium. Against real data, production status dropdowns and cell editors are probably empty.
- **Where:**
  - The UI asks for key-value module `f157469e-5e5c-4a5b-b071-89a28b2a0310` in:
    - `UI/components/production-status-combobox.tsx`
    - `UI/components/ag-grid/production-status-cell-editor.tsx`
    - `UI/components/ag-grid/production-status-cell-renderer.tsx`
  - The backend stores production statuses under `KeyValueIds.ProductionStatuses = "97dd4b13-ff15-47ff-955d-5e957644cffd"` (`Shared/KosmosERP.Models/KeyValueIds.cs`), and the seeder populates that id (`Tools/KosmosERP.Seeder/DatabaseSeeder.Sales.cs`).
- **Symptom:** on production order pages the status options are empty, and statuses render as raw keys or blanks.
- **Root cause:** there are two ids for one concept. `UI/services/keyvalue-service.tsx` (`getModuleAndKeyValueTypes`) lists both `f157469e… "Production Status"` and `97dd4b13… "Production Order Status"`. The backend and seeder use `97dd4b13…`, which is also `ERPModulesId.ProductionOrderModule`.
- **Evidence:** the e2e mocks follow the UI's id (`UI/e2e/factories/lookups.ts`, `KeyValueModules.ProductionStatus`), so tests pass without exercising real data.
- **Decision needed:** which id is canonical? Probably `97dd4b13…`, to match the backend. Check the API and any DB rows before choosing.
- **Suggested fix:**
  - Point the three UI components at the canonical id, preferably through a shared constant instead of inline GUIDs.
  - Remove the duplicate from the admin Lists module menu.
  - Update `KeyValueModules.ProductionStatus` in `UI/e2e/factories/lookups.ts` to match.
- **Done when:**
  - Against seeded data (`dotnet run --project Tools/KosmosERP.Seeder`), the production order status dropdown lists Submitted, Parts Pulled, …
  - The e2e suite passes with the updated lookup id.

<a id="bug-004"></a>
## BUG-004: Freight carrier dropdown uses a lookup id the seeder never populates

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted) for what changed. The original report follows for reference.

- **Severity:** Medium. The shipment freight carrier dropdown is probably empty against seeded data.
- **Where:**
  - `UI/components/freight-combobox.tsx` asks for module `2a2d1004-5283-40ef-96fd-8cc30c65cefa`, listed as "Frieght Company" in `UI/services/keyvalue-service.tsx`.
  - The seeder adds UPS, FedEx and DHL under `KeyValueIds.ShippingMethods` (`9da95117…`) (`Tools/KosmosERP.Seeder/DatabaseSeeder.Sales.cs`, `CarrierUps` etc.). That also puts carriers into the shipping-method dropdown.
- **Symptom:**
  - The freight carrier dropdown on shipment pages is empty.
  - The shipping method dropdown shows carriers (UPS, FedEx) mixed in with methods (Pickup, Common Carrier).
- **Programmer update:** Ryan added example carriers of UPS/FedEx/DHL to ShipmentModule.cs in the backend and updated KeyValueIds.cs in the backend as well.
- **Root cause:** the seeder and UI disagree on where carriers live. `2a2d1004…` has no constant in `KeyValueIds.cs`.
- **Decision needed:** whether carriers are their own lookup (the UI's model) or a kind of shipping method. The UI model looks intended.
- **Suggested fix (if carriers are their own lookup):**
  - Add `FreightCarriers = "2a2d1004-…"` to `Shared/KosmosERP.Models/KeyValueIds.cs`.
  - Seed carriers under it.
  - Fix the "Frieght" typo in the admin module list.
- **Done when:**
  - After re-seeding (`--reset`), the shipment pages list carriers only in the freight carrier dropdown.
  - The e2e suite still passes.

<a id="bug-005"></a>
## BUG-005: React hydration error #418 on most production page loads

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** Medium. React throws away the server-rendered markup and re-renders on the client. The cost is slower first paint and console errors; nothing breaks visibly.
- **Where:**
  - `UI/app/layout.tsx` and `UI/app/docs/layout.tsx`, both wrapping `<Provider>` from `UI/components/ui/provider.tsx`
  - Chakra UI v3 / Emotion
- **Symptom:** in a production build (`next build && next start`), pages throw `Minified React error #418` (server HTML didn't match the client). Measured: `/` 5 of 6 loads, `/login/database` 2 of 6, `/erp` 6 of 6. It doesn't happen in `next dev`.
- **Root cause:** there's no Emotion cache registry for the App Router. Server rendering writes `<style data-emotion=…>` tags inline in `<body>` (check with `curl http://localhost:3100/login/database | grep data-emotion`). Emotion's client cache moves them into `<head>` when it's created, and that races React hydration.
- **Evidence:** `UI/e2e/fixtures/test.ts` holds a `KNOWN_PAGE_ERRORS` allowlist entry that turns these errors into a `known-issue` report annotation instead of a failure.
- **Suggested fix:**
  - Add a client "registry" component that creates an Emotion cache and flushes inserted styles through `useServerInsertedHTML`. This is the standard App Router pattern, the same approach as MUI's `AppRouterCacheProvider`.
  - Wrap both layouts in it, and check Chakra v3's current Next.js guidance.
- **Done when:**
  - The `KNOWN_PAGE_ERRORS` hydration entry is deleted.
  - `npx playwright test --repeat-each=3 --retries=0` passes, with no pageerror containing `418` on a production build.

<a id="bug-006"></a>
## BUG-006: "Delete Record" on new-record forms crashes (no handler)

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** Medium. A visible button that throws. The user is left with a stuck confirm dialog showing a spinner.
- **Where:** `UI/components/page-actions.tsx` defaults `canDelete=true`. The Phase 2 page dumps show "Delete Record" on **10** create pages, the `new` page of each of these modules under `UI/app/erp/`:
  - `chartofaccounts`
  - `contacts`
  - `customers`
  - `journalentries`
  - `leads`
  - `opportunities`
  - `products`
  - `purchaseorders`
  - `shipments` (its create page is `shipments/new/[id]`)
  - `vendors`

  They behave differently:
  - **7 crash** when the delete is confirmed: contacts, customers, leads, opportunities, purchaseorders and vendors pass `onDelete={undefined}`, and `shipments/new/[id]` passes no `onDelete` at all.
  - **3 act as "cancel":** chartofaccounts, journalentries and products pass a handler that just navigates back to the list. That doesn't crash, but "Delete Record" is a misleading label for it.
- **Symptom:** a "Delete Record" button appears on a record that doesn't exist yet. On the 7 crashing pages, clicking it and then **Delete** in the confirm dialog calls `onDelete()`, which is undefined, giving `TypeError: onDelete is not a function`. The dialog stays open with its spinner.
- **Root cause:** the `canDelete` default and the missing prop on the create pages.
- **Evidence:** the button's presence is pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-006 …" (`test.fail`). The crash on confirm comes from reading the code and hasn't been clicked through in a browser.
- **Suggested fix:**
  - Default `canDelete` to `Boolean(onDelete)` in `page-actions.tsx`, or pass `canDelete={false}` on the create pages. The default is the more robust option.
  - Drop the "navigate back" handlers on the 3 cancel-style pages, or relabel that action "Cancel".
  - Grep for other `PageActionsComponent` uses without `onDelete`.
- **Done when:**
  - No create page shows "Delete Record".
  - The `test.fail` line is removed from the BUG-006 test in `known-bugs.spec.ts`, and the test passes.
  - Optionally, add a "no Delete Record" check to every `new` entry in `UI/e2e/catalog.ts`.

<a id="bug-007"></a>
## BUG-007: Lookup comboboxes filter on the hidden key, not the visible label

- **Severity:** Low. Typing to search in these dropdowns finds nothing, and users have to scroll instead.
- **Where:** nine comboboxes in `UI/components/`, all using `useListCollection({ itemToString: (item) => item.key, itemToValue: (item) => item.value, … })`:
  - `customer-payment-terms-combobox.tsx`
  - `freight-combobox.tsx`
  - `lead-stage-combobox.tsx`
  - `opportunity-stage-combobox.tsx`
  - `payment-method-combobox.tsx`
  - `product-category-combobox.tsx`
  - `production-status-combobox.tsx`
  - `shipment-method-combobox.tsx`
  - `transaction-type-combobox.tsx`
- **Symptom:** options display `item.value` (e.g. "Net 30"), but filtering uses `itemToString` = `item.key` (e.g. `payment_terms_net_30`). Typing "Net 30" matches nothing, since there's a space where the key has an underscore.
- **Root cause:** `itemToString` and `itemToValue` are swapped relative to what's displayed.
- **Evidence:** the e2e helper `CustomerFormPage.choose()` (`UI/e2e/pages/customers-pages.ts`) opens with ArrowDown instead of typing, specifically to avoid this. Its comment says so.
- **Suggested fix:**
  - Set `itemToString: (item) => item.value` (the label) in all nine components.
  - Check each component's `inputChange` / `onValueChange`, which look items up by `value`, so selection keeps working. The stored form value must still be `item.key`.
  - Consider one shared `KeyValueCombobox` to replace the nine near-identical copies.
- **Done when:**
  - Typing a label ("Net 60") filters to that option.
  - Change `choose()` to type-then-pick. The customers and products e2e specs pass, and so do the Tier 2 specs that use these comboboxes.

<a id="bug-008"></a>
## BUG-008: Login reports "incorrect password" when the API is unreachable

- **Severity:** Low. The message is misleading and sends users chasing the wrong problem.
- **Where:**
  - `UI/services/user-service.tsx` (`authenticateUser`, lines ~14–24)
  - `UI/app/login/database/page.tsx`
- **Symptom:** with the API down or blocked (network error, CORS), the login page says "The username or password is incorrect." Its intended "Unable to reach the authentication service." message never shows.
- **Root cause:** `authenticateUser` catches every error and **returns** `{ success: false, resultCode: -5 }` rather than rethrowing. The page's `catch` branch, which holds the "Unable to reach…" toast, is unreachable.
- **Evidence:** the e2e test `auth/login.spec.ts` › "an unreachable API fails the sign in" asserts only "Sign in failed", with a comment explaining why.
- **Suggested fix:** either rethrow network errors (those with no `ex.response`) from `authenticateUser`, or have the page check `resultCode === -5` and show the connectivity message.
- **Done when:**
  - The e2e test asserts `Unable to reach the authentication service.`, and the comment is removed.
  - The rejected-credentials test still passes.

<a id="bug-009"></a>
## BUG-009: Seeded users can't log in (unhashable seed password)

- **Severity:** Medium for development. The seeded dataset can't be used through the UI, and a future full-stack e2e suite would need a working login.
- **Where:**
  - `Tools/KosmosERP.Seeder/DatabaseSeeder.Sales.cs` (~line 128) sets `password = "seeded"` and `password_salt = "seeded"` for users `admin`, `ext-jordan`, `ext-riley` and `ext-morgan`.
- **Symptom:** logging in as any seeded user fails, most likely with a server error rather than a clean rejection.
- **Root cause:** `DatabaseAuthenticationProvider.HashPassword` (`Shared/KosmosERP.BusinessLayer/AuthenticiationProviders/DatabaseAuthenticationProvider.cs`, ~line 119) calls `Convert.FromBase64String(salt)`. `"seeded"` isn't valid Base64, which throws `FormatException`. Even with a valid salt, the stored `password` isn't a PBKDF2 hash.
- **Suggested fix:**
  - Generate a random salt and hash a known dev password with the same PBKDF2 settings (HMACSHA1, 10000 iterations, 32 bytes). Better still, reuse the API's own hashing in `UserModule`, so the two can't drift.
  - Document the dev password in the seeder README.
- **Done when:** after `dotnet run --project Tools/KosmosERP.Seeder -- --reset`, you can sign in at `/login/database` as `admin` with the documented password.

<a id="bug-010"></a>
## BUG-010: `CreditMemoHeaderDto` doesn't declare the `guid` it carries

- **Severity:** Low. It's a typing gap only.
- **Where:** `UI/models/credit-memo-models.tsx`. `CreditMemoHeaderDto implements BaseDto` but redeclares only some fields, and `guid` isn't one of them.
- **Symptom:** TypeScript rejects `creditMemo.guid`, even though the API returns it and the pages route by it.
- **Evidence:** `UI/e2e/catalog.ts` has a loosely typed `guid()` helper, with a comment, to work around this.
- **Suggested fix:**
  - Make it `extends BaseDto`, as most DTOs do, or add `guid?: string | null`.
  - Check the other `implements BaseDto` classes (`OrderHeaderDto`, `APInvoiceHeaderDto`, `APInvoiceLineDto`, …) for the same gap.
- **Done when:** the `catalog.ts` helper can take `{ guid?: string | null }` again, and `npm run test:e2e:typecheck` passes.

<a id="bug-011"></a>
## BUG-011: Date-only values show one day early in US time zones

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** Medium. Required dates, close dates and schedule dates display wrong for every user west of UTC.
- **Where:** pages that turn a C# `DateOnly` string (`"2026-04-01"`) into a `Date` with `new Date(value)`. Confirmed:
  - `UI/app/erp/salesorders/view/[id]/page.tsx` (~line 114, `required_date`)
  - `UI/app/erp/opportunities/view/[id]/page.tsx` (`expected_close`)
  - the production orders list (planned start and complete dates)
  - the subscriptions list (next date)

  The matching **edit** pages show the right day, because they parse with an explicit format.
- **Symptom:** in `America/Chicago`, a required date of `2026-04-01` shows as `03/31/2026`.
- **Root cause:** JavaScript parses a bare `yyyy-MM-dd` string as **UTC** midnight, which is still the previous day in US time zones. The shared grid formatter `UI/components/ag-grid/date-only-renderer.tsx` gets this right: it rebuilds the string as `MM/DD/YYYY`, which parses as local time. The pages above bypass it.
- **Evidence:**
  - Pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-011 …" (`test.fail`).
  - `UI/playwright.config.ts` pins `timezoneId: 'America/Chicago'` so it reproduces on every machine and in Docker/CI. With a UTC browser it doesn't show.
- **Suggested fix:**
  - Add one helper, e.g. `parseDateOnly(value)` using date-fns `parse(value.slice(0, 10), 'yyyy-MM-dd', new Date())`, and use it wherever a DateOnly field becomes a `Date`.
  - Grep `UI/app` for `new Date(` on the DateOnly fields: `required_date`, `order_date`, `expected_close`, `start_date`, `next_date`, `end_date`, `invoice_date`, `invoice_due_date`, `paid_on`, `planned_start_date`, `planned_complete_date`, `actual_completed_on`. That's the full list from `Shared/KosmosERP.BusinessLayer/Models/Module/*/Dto`.
- **Done when:**
  - The BUG-011 test passes without `test.fail`.
  - The sales order view, opportunity view, production order list and subscription list show the stored day.

<a id="bug-012"></a>
## BUG-012: Shipment pages show the raw shipping-method key

- **Severity:** Low. Users see `shipping_method_carrier` instead of "Common Carrier".
- **Where:**
  - `UI/app/erp/shipments/page.tsx` (the "Recently Shipped" grid)
  - `UI/app/erp/shipments/edit/[id]/page.tsx` (the read-only "Ship Via" field)
- **Symptom:** the Ship Via column and field show the lookup **key** stored on the shipment (`ship_via`), not its label.
- **Root cause:** the pages render `ship_via` directly. Unlike sales orders (`shipping_method_name`), `ShipmentHeaderDto` carries no `*_name` field for it.
- **Suggested fix:** either have the API return a `ship_via_name`, like orders do, or resolve the label on the client from the shipping-method lookup (module `9da95117…`, which is already loaded on these pages).
- **Done when:** both places show "Common Carrier" for `shipping_method_carrier`. Add a catalog check for the Ship Via value on `shipments/edit`.

<a id="bug-013"></a>
## BUG-013: `/erp/ar/new` is a stub with hard-coded sample data

- **Severity:** Low, but confusing if anyone reaches it.
- **Where:** `UI/app/erp/ar/new/page.tsx`
- **Symptom:** the page renders fixed welding-equipment rows ("Miller Model 203 Welder", …) and makes no API calls. The real invoice-from-order flow is `/erp/ar/new/[id]`, reached from "Create Invoice" on the AR list.
- **Decision needed:** delete the page, redirect it to `/erp/ar`, or build it into a "pick an order to invoice" screen.
- **Evidence:** `UI/e2e/catalog.ts` entry `ar/new` has a load check only, with a comment pointing here.
- **Done when:** the page is removed or redirected (update or remove its catalog entry), or it lists real orders ready for invoicing (give it real checks).

<a id="bug-014"></a>
## BUG-014: Form controls with no accessible name

- **Severity:** Low. Screen readers can't identify these controls, and tests have to find them by placeholder text.
- **Where** (from the Phase 2 accessibility dumps):
  - `UI/app/erp/contacts/edit/[id]/page.tsx`: the customer combobox is named only "Type to search".
  - `UI/app/erp/ap/new/page.tsx` and `UI/app/erp/ap/edit/[id]/page.tsx`:
    - the invoice number textbox has no name
    - the vendor and associated-object comboboxes are named "Type to search" or have no name
    - the three date pickers are all named "Select date"
    - these inputs sit in a Chakra `DataList` (`<dt>` label, `<dd>` value), which doesn't label them. The Phase 3 AP tests find them via `dataListValue()` in `UI/e2e/pages/controls.ts`.
  - `UI/app/erp/creditmemos/new/page.tsx`: the Credit Memo Date and Due Date inputs have no name at all, not even a placeholder.
  - `UI/app/erp/admin/users/page.tsx`: an icon-only button above the user tree has no name.
  - Date pickers across the app are all named "Select date", with no field context.
  - Grid row action buttons (e.g. "Delete" on order lines) are named only by their text, so they can't be told apart without a row scope.
  - Already fixed: the icon-only AR invoice search button in `UI/components/ar-invoice-selector.tsx` now has `aria-label="Search AR invoices"` (added in Phase 3).
- **Root cause:** controls rendered outside a Chakra `Field.Root` / `Field.Label`, and icon-only buttons without `aria-label`.
- **Suggested fix:** wrap each control in `Field.Root` + `Field.Label`, the pattern the customer forms use, or add `aria-label`. For react-datepicker, pass `ariaLabelledBy` or put it inside the Field.
- **Done when:**
  - Each control has a unique, meaningful accessible name.
  - The `ap/edit` catalog check can use `combo('Vendor', …)` instead of `combo('Type to search', …)`.

<a id="bug-015"></a>
## BUG-015: Credit memo edit page is titled "New Credit Memo"

- **Severity:** Low. It's a wrong heading, though users might think they're creating a duplicate.
- **Where:** `UI/app/erp/creditmemos/edit/[id]/page.tsx` renders `<h1>New Credit Memo</h1>`. It looks copied from `creditmemos/new`.
- **Suggested fix:** use "Edit Credit Memo - {credit_memo_number}", matching the view page's "Credit Memo - {number}".
- **Done when:** the heading is fixed and the `creditmemos/edit` catalog entry's `ready` uses the new heading.

<a id="bug-016"></a>
## BUG-016: Combobox options inside modal dialogs are hidden from assistive tech

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** Medium. Screen-reader and voice-control users can't pick a product when adding sales order lines or BOM items.
- **Where:**
  - `UI/components/product-combobox.tsx`, when used inside `UI/components/dialogs/add-sales-order-line.tsx` and `UI/components/dialogs/add-bom-item.tsx`.
  - Likely any Chakra combobox that wraps its list in `<Portal>` inside a modal `Dialog`.
- **Symptom:**
  - The options are visible and clickable, but they have no accessible name, so `getByRole('option', { name })` can't find them.
  - **Clicking an option can close the dialog.** On Admin → Lists › Add New Entry, picking the module with the mouse closes the whole dialog about 1 time in 10, a moment after the pick. Picking with the keyboard (ArrowDown/Enter) never did (0 of 10). The dialog treats the click on the portaled option as a click outside itself. Users can hit this too.
- **Root cause:** the combobox portals its listbox to `<body>`, outside the modal dialog. A modal dialog hides everything outside itself from assistive tech (`aria-hidden`), including the portaled options. (`product-combobox` also sets `zIndex: 9999` on the content, which looks like an earlier workaround for the same layering.)
- **Evidence:**
  - `searchAndChoose(..., { inModal: true })` in `UI/e2e/pages/controls.ts` falls back to matching `[role="option"]` by text, with a comment pointing here.
  - `UI/e2e/tests/admin/lists.spec.ts` › "adds a payment term with its number of days" is flaky for this reason: it failed 1 in 12 runs before the BUG-005 fix, and between 0 and 5 in 12 after it. CI retries twice, so it rarely fails there; local runs (no retries) can.
- **Suggested fix:** don't portal the combobox content when it's inside a dialog. Render `Combobox.Positioner` inline, or give it a portal container inside the dialog.
- **Done when:** the `inModal` fallback can be removed, the sales order and product BOM specs still pass, and `lists.spec.ts` passes `--repeat-each=20 --retries=0`.

<a id="bug-017"></a>
## BUG-017: Shipment line "Delete" button marks the line shipped

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** High. A user removing a line records it as shipped instead, which changes shipment and inventory data.
- **Where:** `UI/app/erp/shipments/edit/[id]/page.tsx`. The line grid's Actions column renders a red **"Delete"** button whose `onClick` is `handleShipLineClick`.
- **Symptom:** clicking "Delete" sends `PUT /ShipmentLine/UpdateShipmentLine` with `units_shipped = units_to_ship` and `is_complete: true`, then reloads the shipment.
- **Decision needed:** was the button meant to be "Ship" (mark the line shipped), which matches the handler, or "Delete" (remove the line), which matches the label? Pick one, then fix the other.
- **Done when:** the label and action agree, and a Tier 2 test in `UI/e2e/tests/shipments/shipments.spec.ts` covers the button. There's no test now, on purpose, so the wrong behavior doesn't get pinned as intended.

<a id="bug-018"></a>
## BUG-018: Non-taxable customers' invoice lines default to taxable

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** High. Tax-exempt customers can be taxed on invoices.
- **Where:** `UI/app/erp/ar/new/[id]/page.tsx` (~line 116):

  ```ts
  command.is_taxable = customerResponse.data?.is_taxable ? customerResponse.data?.is_taxable : true;
  ```

- **Symptom:** for a customer with `is_taxable: false`, every invoice line starts with Tax checked and is sent with `is_taxable: true`. Tax is only calculated when `tax_rate > 0`, so a non-taxable customer with a rate on file gets taxed.
- **Root cause:** the ternary falls back to `true` whenever the value is falsy, and `false` is falsy.
- **Evidence:** pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-018 …". With the marker off, it fails with "Expected: false, Received: true". Same page as BUG-019 (fixed), so the test needs no workaround any more.
- **Suggested fix:** `command.is_taxable = customerResponse.data?.is_taxable ?? true;`, or default to `false`. Check the same pattern for `tax_rate` on the line above.
- **Done when:** the BUG-018 test passes without `knownBug`.

<a id="bug-019"></a>
## BUG-019: Invoicing crashes unless payment terms are named "NETxx"

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** High. Against seeded data (terms named "Net 30"), creating an AR invoice from an order throws, and nothing is saved.
- **Where:** `UI/app/erp/ar/new/[id]/page.tsx` (~lines 85–97 and 225–233).
- **Symptom:** clicking "Save and Print Invoice" throws `RangeError: Invalid time value` and sends no request.
- **Root cause:** the due date is computed from the payment terms **name**: `Number(payment_terms_name.replace("NET", ""))`. For "Net 30" (mixed case, with a space) that's `NaN`, so the due date is an Invalid Date, and `format(invoiceDueDate, 'yyyy-MM-dd')` throws on save. The code has a TODO about exactly this. The page also keeps its dates as `toLocaleDateString()` strings and re-parses them, which depends on the browser locale.
- **Evidence:**
  - Pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-019 …". With the marker off, it fails with the `Invalid time value` page error.
  - The Tier 2 happy path in `UI/e2e/tests/accounting/ar-invoices.spec.ts` renames the terms to "NET30" to get past it.
- **Suggested fix:** store the term length as data (e.g. the lookup's `int_value`, or a days field on payment terms) instead of parsing the display name. Keep dates as `Date` objects, or ISO strings, not locale strings.
- **Done when:** the BUG-019 test passes without `knownBug`, and the `worldWithNet30Naming()` workaround in `ar-invoices.spec.ts` can be deleted.

<a id="bug-020"></a>
## BUG-020: Journal entry and chart of accounts pages pass props PageActions ignores

- **Severity:** Medium. Users get no success or failure feedback on these pages, and posted journal entries still show Save and "Delete Record".
- **Where:** `UI/app/erp/chartofaccounts/new/page.tsx`, `chartofaccounts/edit/[id]/page.tsx`, `journalentries/new/page.tsx` and `journalentries/edit/[id]/page.tsx` pass `showDelete`, `showSave`, `showSaveSuccess` and `showSaveFailed` to `UI/components/page-actions.tsx`. That component takes `canDelete`, `hidden`, `successSaved` and `failedSaved`.
- **Symptom:**
  - A failed post or save shows no "Record could not be saved!" message, and a successful one shows no "Record saved!".
  - On a posted journal entry, "Delete Record" and Save are still shown, because `showDelete` and `showSave` are ignored.
- **Root cause:** these pages were written against a different (older or planned) PageActions API.
- **Evidence:** pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-020 …" (a failed post shows no error). `UI/e2e/tests/accounting/journal-entries.spec.ts` › "a failed post leaves the entry as a draft" notes it.
- **Suggested fix:** use the component's real props on those four pages (`canDelete`, `failedSaved`, `successSaved`, and `hidden` or `saveDisabled` for posted entries). Or add the `show*` props to `PageActionsComponent`, but keep one API, not two.
- **Done when:** the BUG-020 test passes without `knownBug`, and a posted entry shows neither Save nor Delete.

<a id="bug-021"></a>
## BUG-021: Journal entry "Account" column is a free-text internal id

- **Severity:** Medium. To enter a journal entry, users have to know and type the database id of each account.
- **Where:** `UI/app/erp/journalentries/new/page.tsx` and `journalentries/edit/[id]/page.tsx`. The line grid's `chart_of_account_id` column uses `agTextCellEditor`.
- **Symptom:** the Account cell accepts any text and sends it as `chart_of_account_id`. There's no picker, no validation that the account exists, and the grid shows the raw id.
- **Suggested fix:** use an account picker as the cell editor. `UI/components/chart-of-account-combobox.tsx` already exists, and the GL account selector used by credit memos and AP is a model. Render account number and name in the cell.
- **Done when:** lines are entered by choosing an account, and `UI/e2e/pages/journal-entry-pages.ts` `fillLine()` picks accounts by number or name instead of typing ids.

<a id="bug-022"></a>
## BUG-022: BOM item dialog is titled "Add Address"

- **Severity:** Low.
- **Where:** `UI/components/dialogs/add-bom-item.tsx` (`<Dialog.Title>Add Address</Dialog.Title>`). It looks copied from the address dialog.
- **Suggested fix:** "Add BOM Item".
- **Done when:** fixed, and `UI/e2e/tests/products/product-flows.spec.ts` finds the dialog by name: `getByRole('dialog', { name: 'Add BOM Item' })`.

<a id="bug-023"></a>
## BUG-023: New Product can't be saved after a direct load or refresh

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** High. Opening `/erp/products/new` by URL, bookmark or refresh leaves Save permanently disabled. Arriving through Products → New Product works.
- **Where:** `UI/app/erp/products/new/page.tsx`. The permission and initialization `useEffect` has deps `[setValue]`.
- **Root cause:** the effect runs once on mount. On a direct load, auth isn't ready yet (`auth.authenticated` is false), so it returns early and never runs again. `hasWritePermission` stays false, and Save is `disabled={!formValid || !hasWritePermission}`. Confirmed by reading the component's React state in a dev build: `formValid` true, `hasWritePermission` false.
- **Evidence:** pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-023 …". The Tier 2 create test in `product-flows.spec.ts` navigates from the list instead.
- **Suggested fix:** add `auth.authenticated` to the deps, as the customer pages do, and keep the `hasInitialized` guard **after** the auth check. Then audit other pages whose init effects don't depend on auth: `grep -n "hasInitialized.current = true" -A…` in `UI/app/erp`. `admin/users/page.tsx` (`[]`) is a candidate, as is the Leads entry under Needs verification.
- **Done when:** the BUG-023 test passes without `knownBug`.

<a id="bug-024"></a>
## BUG-024: Production order edits are never saved

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** High. Changing a production line's status on the edit page looks like it works, but nothing is persisted.
- **Where:** `UI/app/erp/productionorders/edit/[id]/page.tsx`
  - `handleSaveClick` only calls `setSuccessSaved(false)`.
  - `CheckFormValidity` is empty, so Save never enables.
  - The grid's `onCellValueChanged` only updates local state.
- **Evidence:**
  - Pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-024 …". It changes a status and expects a `PUT …/ProductionOrder…` request; none is sent.
  - `UI/e2e/tests/production/production-orders.spec.ts` covers what does work: the status editor updates the grid.
- **Suggested fix:** call `productionOrderService.updateLine` (or `update` with the lines) when a status changes or on Save. Implement `CheckFormValidity`.
- **Done when:** the BUG-024 test passes without `knownBug`, and the production spec asserts the update payload.

<a id="bug-025"></a>
## BUG-025: Completed production orders stay editable

- **Severity:** Low, but it will matter once BUG-024 is fixed.
- **Where:** `UI/app/erp/productionorders/edit/[id]/page.tsx`: `const [colDefs] = useState([... { field: 'status', editable: !completedOrDisabled, ... }])`.
- **Root cause:** column definitions are kept in `useState`, so `editable` is computed once at mount, before the order loads. At that point `completedOrDisabled` is false, and it never updates.
- **Evidence:** pinned by `UI/e2e/tests/known-bugs.spec.ts` › "BUG-025 …", which checks for ag-grid's `ag-cell-inline-editing` class on double-click. `UI/app/erp/salesorders/edit/[id]/page.tsx` has the same `useState(colDefs)` pattern, so check it and others.
- **Suggested fix:** use `useMemo(() => colDefs, [completedOrDisabled])`, or `editable: (p) => !p.context.completedOrDisabled` with grid `context`, as the shipment edit page does for its buttons.
- **Done when:** the BUG-025 test passes without `knownBug`.

<a id="bug-026"></a>
## BUG-026: API endpoints with no authorization (Settings writable anonymously)

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference. While fixing it, the permission handler behind `[ERPAuthorize]` also turned out to be broken for every non-admin user; that is fixed too.

- **Severity:** High. Some write endpoints can be called by anyone who can reach the API, **without signing in**.
- **Where:** `Api/Controllers/*.cs`. There's no global authorization filter (`Api/Program.cs` only adds logging filters, and `ERPApiController` has no `[Authorize]`), so an action is protected only if it has `[ERPAuthorize(...)]`. Found while fixing BUG-001:

  | Controller | Actions without `ERPAuthorize` | Notes |
  | --- | --- | --- |
  | `SettingsController` | 7 of 7 | Includes `UpdateSettings`, `CreateSettings` and `DeleteSettings`; `[Authorize]` is commented out |
  | `CountryController`, `StateController` | 7 of 7 each | Includes Create, Update and Delete |
  | `UserController` | 8 of 19 | `AuthenticateUser` and token refresh must stay open. Check the rest |
  | `OpportunityController` | 2 of 9 | Check which |
  | `ReportsController` | 2 of 26 | Check which |
  | `NotificationController` | 3 of 3 | |
  | `DocsController` | 5 of 5 | Possibly intentional: server-side PDF rendering (`UI/app/docs/api`) may call these without a user token |
  | `DiagnosticsController`, `SAMLController` | all | SAML login must stay open. Diagnostics: decide |

  (Counts compare `[Http*]` attributes with `[ERPAuthorize` per file. `GlobalSearchController` has a class-level attribute.)
- **Symptom:** e.g. `PUT /api/v1/Settings/UpdateSettings` with no `Authorization` header changes the company settings.
- **Decisions needed:** which endpoints are meant to be public (login, SAML, maybe docs rendering, maybe country/state reads), and what the rest should require. The UI treats settings as admin-only (`admin_edit`), and the `UserController` endpoints use `ERPAuthorize(..., "admin")`.
- **Suggested fix:** add `[ERPAuthorize(...)]` to every non-public action (for Settings, match the admin rule the UI uses). Consider a global default-deny with explicit `[AllowAnonymous]` on the public ones, so a new controller can't be left open by accident. Add API tests that call each protected endpoint unauthenticated and expect 401 or 403.
- **Done when:** every action is either authorized or explicitly marked anonymous with a comment saying why, and there are tests for it.

<a id="bug-027"></a>
## BUG-027: Grid buttons send an empty token after a direct load

> **Fixed (uncommitted).** See [Fixed (uncommitted)](#fixed-uncommitted). The original report follows for reference.

- **Severity:** Medium. After loading one of these pages by URL, bookmark or refresh, its grid's Delete (or tag) button calls the API with no token. The real API answers 401 and nothing happens. Arriving through the app works, because auth is already ready on the first render.
- **Where:** each page keeps its grid `colDefs` in `useState`, so the cell renderers keep the handler from the **first** render. On a direct load that render runs before auth is ready, so `auth.token` is `""`:
  - `UI/app/erp/admin/document-types/page.tsx`: `handleDeleteClick`, `handleTagDelete`, `doTagDialogOpen`
  - `UI/app/erp/admin/lists/page.tsx`: `handleDeleteClick`
  - `UI/app/erp/creditmemos/edit/[id]/page.tsx`: `handleDeleteLineClick`
  - `UI/app/erp/opportunities/edit/[id]/page.tsx`: `handleDeleteLineClick`
  - `UI/app/erp/purchaseorders/edit/[id]/page.tsx`: `handleDeleteLineClick`
- **Same root cause, check too:** renderers that read state from the first render, such as `hidden={!hasEditPermission}` on `productionorders/page.tsx`, `shipments/page.tsx` and `vendors/page.tsx`. These may hide Edit buttons for admins after a direct load. BUG-025 is the same pattern for `editable`.
- **Found by:** the BUG-023 audit. The e2e API fixture now fails any test whose page calls a protected endpoint without a token, but no test clicks these buttons after a direct load yet.
- **Suggested fix:** the fix used for the sales order line Delete in BUG-023 (`salesorders/edit/[id]`): call the handler through a ref that's updated every render (`const deleteLineRef = useRef(handler); deleteLineRef.current = handler;` then `onClick={() => deleteLineRef.current(id)}`). Alternatively, have the renderer only call a state setter (as `shipments/edit/[id]` does for its Remove dialog), or build `colDefs` with `useMemo` on the values they read.
- **Done when:** a test per page loads it directly, clicks the button, and the API call carries a token. The fixture's token check enforces that automatically once the click is covered.

---

<a id="needs-verification"></a>
## Needs verification

These were seen in the mocked e2e pages but not confirmed as app bugs. They might be quirks of the mocked data.

- **Credit memo "AR Invoice" field is blank** on the view and edit pages, even though the record has `ar_invoice_number`. The value goes `creditmemos/view` → `ARInvoiceSelectorComponent` prop → `setValue('invoice_number', …)` in `UI/components/ar-invoice-selector.tsx`. Check against real data first.
- **Leads list: permission setup can be skipped.** `UI/app/erp/leads/page.tsx` sets `hasInitialized.current = true` **before** the `auth.authenticated` check. If the first render happens before auth is ready, the effect returns and never runs again, so `hasEditPermission` stays false and Edit buttons stay hidden. The customers page does the check first. Confirm in a browser whether Edit shows for admins on Leads.
- **Validation errors show before the user types.** On `vendors/new` and others, required fields are marked invalid on first load, so the form opens covered in red. This may be intended.
- **Shipments can probably be over-shipped.** In `UI/components/ag-grid/numeric-for-orders-shipped.tsx` the "Units To Ship" editor caps at `units_ordered - units_shipped`, where `units_shipped` is this new shipment's own count (0). It ignores `units_already_shipped`, so a fully shipped line can be shipped again. Check whether the API rejects it.
- **Shipment "Freight Charge" is marked required, but 0 passes.** `shipments/new/[id]` checks `Boolean(watch('freight_charge_amount'))`, and the field defaults to the *string* `"0"`, which is truthy. Decide whether $0 freight is allowed. If not, compare numerically. The shipments spec asserts current behavior and points here.
- **Global search ignores module permissions.** `POST /GlobalSearch/Search` (`Shared/.../Modules/GlobalSearchModule.cs`) returns customers, sales and purchase orders, vendors, contacts, opportunities, leads and documents to any signed-in user, even one without read access to those modules. Decide whether results should be filtered by the caller's module permissions.
- **SignalR hub now requires sign-in.** `/notification_hub` falls under the new default-deny policy. The UI doesn't use it today. A future client must send its token, and JWT bearer auth has to be set up to read it from the `access_token` query string (`JwtBearerEvents.OnMessageReceived`), which isn't configured.
- **Deleting an address cascades to orders.** `FK_order_headers_addresses_ship_to_address_id` is `ON DELETE CASCADE`, so hard-deleting an address deletes the orders shipped to it (and, through them, more). Seen while verifying BUG-002. Check other financial and transactional foreign keys for the same default and decide which should be `Restrict`.
- **Releasing a shipment sends a near-empty update.** After a successful save, `shipments/edit/[id]` sends a second `PUT /Shipment/UpdateShipmentHeader` with only `{ id, is_released: true }`. If the API treats missing fields as null (not "unchanged"), releasing would wipe ship-via, carrier and charges. Check the API's update semantics.

---

<a id="fixed-uncommitted"></a>
## Fixed (uncommitted)

These were fixed during the same session and sit in the working tree, not committed yet. They're listed here so nobody re-opens them.

- **Missing module GUIDs in `ERPModulesId`.** Database-auth users could never reach Subscriptions, Chart of Accounts, Journal Entries, Financial Transactions or Admin. Fixed in `UI/services/permissions-service.tsx` and `UI/lib/auth/role-mapping.ts`: `is_admin` now grants everything.
- **Inverted Save enable logic on edit pages.** A valid edit disabled Save and an invalid one enabled it. Fixed across the edit pages, and `page-actions.tsx` now takes `saveDisabled`.
- **BUG-027: grid buttons work after a direct load, and admins see Edit on every list.**
  - **Verified by test, not by reading.** A new spec loads each page by URL and clicks its grid buttons. The api fixture's token check flags any call without a token.
  - **Real bugs, fixed:**
    - Purchase Order edit, line **Delete**: sent no token (on the delete and the reload). It now calls through a ref, as sales orders do since BUG-023.
    - Document Types, **Edit Tags**: loaded the tags with no token. Same fix.
    - **Vendors and Production Orders lists:** the Edit button was **hidden for admins even when arriving through the app**, not just after a refresh. Their columns were `useState`, so `hidden={!hasEditPermission}` kept the first render's `false`. They now use `useMemo` on `hasEditPermission`, like the other list pages. Their icon-only buttons got accessible names (View, Edit, Print).
  - **Listed in the report, but fine:**
    - Lists, Opportunity line and Document Type deletes, and the tag delete, only open a confirm dialog through a state setter; the call runs in the page body.
    - Credit Memo line Delete rebuilds its columns after the GL accounts load, which is after auth.
    - All are now covered by the spec anyway.
  - **Tests:** new `UI/e2e/tests/pages/grid-actions.spec.ts` (8 tests, every page loaded directly):
    - line Delete on purchase orders, credit memos and opportunities
    - Lists delete
    - Document Types: Edit Tags, tag delete, and document type delete
    - Edit on the Vendors and Production Orders lists
    - It fails on the old code for the 4 real bugs. Full suite: 178 passed.
  - **Still to check:** the Leads list item under "Needs verification" (its permission effect sets `hasInitialized` before the auth check) wasn't part of this.
- **BUG-011: date-only values show the stored day.**
  - **Cause, wider than reported:** both `new Date("2026-04-01")` **and** date-fns 4's `format("2026-04-01", …)` read a bare date as UTC midnight, which is the previous day in US time zones. So the pages' own `format(dateString)` helpers had the bug too, not just `new Date`. Strings with a time part (`…T00:00:00`) and the grid's `DateOnlyRender` were already right.
  - **Fix:** new `UI/lib/date-only.ts` with `parseDateOnly` and `formatDateOnly`, which read `yyyy-MM-dd` as a local calendar day. Each page keeps its existing display format. Used for every `DateOnly` field (the 12 in the API DTOs) that was read through a UTC-parsing path:
    - `salesorders/view` (required date picker)
    - `opportunities/view` (expected close picker, which was given the raw string)
    - `productionorders` list (planned start and complete)
    - `productionorders/view` (order date)
    - `subscriptions` list (next date)
    - `subscriptions/edit` (next date, order date)
    - `ar/view` (order, invoice, due and paid dates)
    - the printable docs for AR invoices, sales orders and production orders
    - `sales-order-selector` (subscription start date). After picking April 1, the picker showed March 31.
  - **Not changed:** fields that are timestamps in the API (`created_on`, vendor approved/audit/retired, credit memo dates, journal entry dates). Their strings include a time, so they're read as local time. Pages that print the raw `yyyy-MM-dd` string show the right day, just unformatted.
  - **Tests:**
    - `UI/e2e/catalog.ts` now asserts DateOnly values on the 7 affected app pages with a `dateOnly()` helper. It builds the expected text from the string, never through `Date`.
    - With the fix reverted, all 7 fail; the date pickers show `03/31/2026` instead of `04/01/2026`.
    - The BUG-011 known-bug test is retired. Full suite: 170 passed.
  - **Not covered by e2e:** the printable docs pages and the subscription sales-order selector.
- **BUG-003: one production status list, `97dd4b13…`.**
  - **What was really wrong:** two lists were in use, and not only in the UI.
    - The old list, `f157469e…` with `production_status_*` keys, was used by the UI dropdowns and label, and by production orders that sales orders create (they started as `production_status_new`).
    - The new list, `97dd4b13…` with `production_order_status_*` keys, was used by the API seed, the seeder, and Shipments' "Ready To Ship" (`GetReadyToShip` filters on `production_order_status_ready_to_ship`).
    - Against real data, nobody could set Ready To Ship, so the shipping list stayed empty.
  - **Decisions (owner):** the new list is canonical. Existing orders are converted by stage. Canceled is added to the new list.
  - **API:**
    - `ProductionOrderStatus` (`Shared/KosmosERP.Models/Enums.cs`) now holds the new keys, plus `production_order_status_canceled`.
    - `ProductionOrderModule.SeedPermissions` creates the seven statuses from it, only adding missing ones, so renames in Lists survive.
    - `OrderModule` starts auto-created production orders and lines as Submitted.
    - `ShipmentModule.GetReadyToShip` uses the constant.
  - **Data migration `RetireOldProductionStatuses`:**
    - Production order headers and lines move to the new keys: New, Released and Scheduled → Submitted; Picking → Parts Pulled; Production → Work In Progress; QC → Quality Check; Completed → Complete; Canceled → Canceled.
    - The old list's entries are soft-deleted, so they drop out of Admin → Lists.
    - `Down` restores those entries but not order statuses (three old stages became Submitted).
    - Verified on MySQL 8.0.44: all 8 old statuses mapped on headers and lines. Untouched: an order already on the new list, an unknown status, the new list's entries, and a same-key row in another module. `Down` restored the old list.
  - **Seeder:** adds Canceled.
  - **UI:**
    - The status combobox, cell editor and cell renderer use `KeyValueModuleIds.ProductionStatuses`.
    - The duplicate "Production Status" (`f157469e…`) entry is removed from the Lists module menu.
  - **Tests:**
    - .NET:
      - `ProductionOrdersModuleTests.SeedPermissions_CreatesTheProductionStatuses` and `…_AddsOnlyMissingStatuses`.
      - `OrderModuleTests.Create_ManufacturedProduct_CreatesSubmittedProductionOrder`.
      - The module tests moved off the old keys, and the unused "Planned" lookup on the old id is dropped.
    - e2e:
      - `KeyValueModules.ProductionStatus` is the canonical id (the mocks followed the UI's wrong id before, which is why tests passed), and the mock list mirrors the API's seven statuses.
      - `production-orders.spec.ts` › "a line can be marked Ready To Ship…" saves `production_order_status_ready_to_ship` and checks the page only asks for the canonical list. It fails on the old UI.
  - **Not changed:** `SQL/KeyValues.sql`, an unreferenced script from the initial import, still inserts the old list. It's out of date in other ways too (payment terms numbered 1–4, a different lead-stage id). Delete it or regenerate it from the seeder rather than run it.
  - **Deploy:** run `dotnet ef database update`, then start the API so `SeedPermissions` adds Ready To Ship and Canceled where they're missing.
- **BUG-016: dropdowns inside dialogs render their options inside the dialog.**
  - **Cause:** the comboboxes portaled their option lists to `<body>`, outside the modal. The modal hid them from assistive tech, so they had no accessible name, and clicking one sometimes counted as a click outside, which closed the dialog.
  - **Fix:** Chakra's guidance is not to portal dropdowns inside a Dialog. Each affected component takes a `portalled` prop (default `true`, so nothing changes elsewhere) and renders `<Portal disabled={!portalled}>`. The dialogs pass `portalled={false}`:
    - `ProductCombobox`: add sales order, purchase order and opportunity line dialogs, and the BOM item dialog.
    - `ModuleListCombobox`: Admin → Lists › Add New Entry.
    - `ActivityStatusCombobox`, `ActivityTypeCombobox`, `PriorityCombobox`: the activities dialog.
    - `NewAddressBlock` (country and state): `new-address-dialog`.
    - `SalesOrderSelectorComponent`: the subscription dialog. Its own nested order-picker dialog still portals, as dialogs should.
    - `AgGridCustomPagination` (page-size select): the AR invoice selector dialog.
    - Admin → Users: the role combobox in its dialog uses `<Portal disabled>` directly.
  - **Found beyond the report:** the address, activities, subscription, AR-invoice-selector and users dialogs had the same problem; the report only named the product dialogs.
  - **Tests:**
    - The `inModal` fallback is removed from `UI/e2e/pages/controls.ts`, so dialog options are found by role and accessible name like any other.
    - Full suite passes (170). `lists.spec.ts --repeat-each=20 --retries=0` passed 40 of 40; it failed about 1 in 10 before.
    - With the fix reverted, the 5 dialog tests fail (BOM component, the Lists entry, 3 sales order line flows).
  - **Not covered by e2e yet:** the activities, address, subscription, AR invoice selector and users dialogs only get the page-render check. No test opens their dropdowns.
- **BUG-005: no more hydration errors on production page loads.**
  - **Cause:** with no Emotion cache registry for the App Router, server rendering wrote every Chakra style as an inline `<style data-emotion>` tag in `<body>`. Emotion's client moved them into `<head>` while React was hydrating, so React found markup that didn't match (`#418`).
  - **Fix:**
    - New `UI/components/ui/emotion-registry.tsx`: an Emotion cache (`compat` mode) that collects the styles inserted during server rendering and writes them into `<head>` through `useServerInsertedHTML`. Same approach as MUI's `AppRouterCacheProvider`.
    - `UI/app/layout.tsx` wraps `<Provider>` in it. It's only in the root layout: `app/docs/layout.tsx` nests a second `Provider` inside the root one, and a second registry would fight over the same styles.
    - `@emotion/cache` added as a direct dependency (it was already installed by Emotion).
  - **Verified on a production build:**
    - Server HTML for `/`, `/login/database` and `/erp` now has 3 Emotion style tags in `<head>` (two global, one combined) and none in `<body>`; before, there were 11 to 24 inline tags in `<body>`.
    - With the allowlist removed, the old code fails the login setup on `#418`.
    - `npx playwright test --repeat-each=3 --retries=0`: 505 of 506 passed with no hydration errors. The one failure was the Lists dialog flake, which predates this fix (see BUG-016).
  - **Tests:** the `KNOWN_PAGE_ERRORS` hydration entry in `UI/e2e/fixtures/test.ts` is deleted, so any hydration error now fails the test that hit it.
- **BUG-006: Delete Record only appears where there's something to delete.**
  - **`UI/components/page-actions.tsx`:**
    - `canDelete` now defaults to `Boolean(onDelete)`, so a page without a delete handler shows no Delete button.
    - The confirm dialog no longer gets stuck. It awaits the handler, then resets its spinner and closes, so a failed delete shows the page's error instead of a dialog with disabled buttons.
  - **Create pages:** removed `onDelete={undefined}` from contacts, customers, leads, opportunities, purchaseorders and vendors, and the fake "delete" handlers (which just navigated back to the list) from chartofaccounts, journalentries and products. `shipments/new/[id]` is fixed by the default.
  - **Found while fixing, same problem:**
    - `admin/settings` showed a crashing Delete Record too; the default fixes it.
    - `ar/view/[id]` forced `canDelete={true}` with an empty handler, so confirming did nothing and the dialog stuck. It's removed. The API has `DeleteARInvoice`, but whether invoices can be deleted from that page is a separate decision.
  - **Not changed:** `chartofaccounts/edit` and `journalentries/edit` show Delete without checking the delete permission (no `canDelete` prop). The API still enforces it.
  - **Tests:**
    - Catalog rule: entries can be marked `deletable` (or given the button's label, e.g. subscriptions' "Cancel Subscription"). Those must show the button to an admin, and every other page must not show Delete Record. That covers all 82 pages, including the 14 deletable edit pages.
    - `customers.spec.ts` › "a refused delete closes the confirmation and shows the error".
    - With the old code, the rule fails on all 12 affected pages and the delete test fails too.
    - The BUG-006 known-bug test is retired.
- **BUG-023: pages work after a direct load or refresh.**
  - **The bug:** `products/new` ran its permission check once, before auth was ready, so Save never enabled. Its deps now include `auth.authenticated`, so it re-runs once auth is ready.
  - **Same pattern fixed elsewhere:**
    - `admin/users` loaded users and roles with an empty token (deps `[]`). It now waits for auth.
    - The nine lookup comboboxes (`customer-payment-terms`, `freight`, `lead-stage`, `opportunity-stage`, `payment-method`, `product-category`, `production-status`, `shipment-method`, `transaction-type`) and `address-selector` fetched once with an empty token. They now skip until there's a token, and re-fetch when it arrives. Before this, every new-record page's dropdowns were empty after a refresh against the real API.
    - Sales order line Delete (`salesorders/edit/[id]`) used the first render's handler, so it sent an empty token after a direct load. It now calls through a ref. The other grid buttons with this problem are logged as [BUG-027](#bug-027).
  - **Guard:** the e2e `api` fixture now fails any test whose page calls a protected endpoint without a bearer token. The anonymous endpoints mirror `PublicEndpoints` in `AuthorizationTests.cs`. `MockApi` records request headers for this.
  - **Tests:**
    - `product-flows.spec.ts` › "new product" runs the full create flow both from the list and after a direct load. The direct-load test fails on the old page.
    - The token guard makes the render test for `admin/users` fail on the old page (`GET /User/GetUsers`, `GET /User/GetRoles`).
    - The BUG-023 known-bug test is retired.
- **BUG-017: the shipment line "Delete" button deletes the line.**
  - **Decision:** Delete. The old "ship" handler only set `units_shipped` and `is_complete`; inventory moves when the shipment is released, which skips deleted lines.
  - **UI** (`UI/app/erp/shipments/edit/[id]/page.tsx`):
    - Delete opens a "Remove line?" confirmation. Remove calls `DeleteShipmentLine` and drops the line from the grid, so a later Save doesn't send it. Cancel does nothing.
    - A failed delete shows "Record could not be saved!" and keeps the line.
    - The button is disabled without the shipping Delete permission (the endpoint requires it), and on released or completed shipments.
    - Each button is labelled "Delete line <description>".
    - `handleShipLineClick` is removed. The cell renderer only calls a state setter, since `colDefs` are captured once.
  - **API** (`ShipmentModule.DeleteLine`):
    - Refuses lines on a released shipment (`DataValidationError`), like `EditLine`.
    - Header `Delete` still removes all its lines through a private `SoftDeleteLine`, as before, and now loads `order_line` like `GetLineAsync` did.
  - **Tests:**
    - .NET: `ShipmentModuleTests.DeleteLine_OnReleasedShipment_IsRefused` and `Delete_ReleasedShipment_StillDeletesItsLines`.
    - e2e: `shipments/shipments.spec.ts` › "shipment line Delete":
      - Confirm removes the line and a later Save leaves it out.
      - Cancel keeps it.
      - An API refusal shows the error.
      - The button is disabled on released shipments and for read-only users.
- **BUG-024: production order line status changes are saved.**
  - **UI** (`UI/app/erp/productionorders/edit/[id]/page.tsx`):
    - Changing a line's status marks that line as unsaved, and Save turns on.
    - Save sends `UpdateProductionOrderLine` with `{ id, status }` for each changed line, then shows "Record saved!" or "Record could not be saved!".
    - Lines that fail stay unsaved, so Save can retry them.
    - Save stays disabled for completed orders and for users without edit permission.
    - Removed the empty `CheckFormValidity` stub and the unused `formValid` state.
    - The change handler used to copy by `rowIndex`, which points at the wrong row once the grid is sorted. It now keys on the line id.
    - `rowData` was typed as purchase-order lines; it's now production-order lines.
  - **API** (`ProductionOrderModule.Edit` / `EditLine`):
    - The status guard checked the *existing* status, so an edit that didn't send a status (quantity only, say) set it to null. It now updates the status only when one is sent.
    - `EditLine` compared `line_number` against the quantity; it now compares against `line_number`.
  - **Still blocked by BUG-003 against real data:** the status editor loads its options from the wrong lookup id, so with seeded data the dropdown may be empty. Saving works once a status can be picked.
  - **BUG-025** (same page, completed orders' grid still editable) is still open. Save is disabled for completed orders, so edits there are not persisted.
  - **Tests:**
    - .NET: `ProductionOrdersModuleTests.EditLine_StatusOnly_ChangesJustTheStatus`, `EditLine_WithoutStatus_KeepsStatus`, `Edit_WithoutStatus_KeepsStatus`.
    - e2e: `production/production-orders.spec.ts` › "edit" (saves the change and asserts the payload; a failed save shows an error and retries; a completed order can't be saved). The save test fails on the old page. The BUG-024 known-bug test is retired.
- **BUG-018: tax-exempt customers are no longer taxed on invoices.**
  - **UI** (`UI/app/erp/ar/new/[id]/page.tsx`): lines default to `customer.is_taxable ?? true`, so a customer's `false` sticks. `tax_rate` uses `?? 0` to match.
  - **API** (`ARInvoiceModule`): the API no longer trusts the client. `Create` clears `is_taxable` on the header and every line when the customer is tax-exempt, so the invoice total has no tax either. `MapToLineDatabaseModel` (used by `Create` and `CreateLine`) only taxes a line when the customer is taxable. So ticking Tax on a line by hand can't tax an exempt customer.
  - **Not changed:** `EditLine` still doesn't recalculate `line_tax` when `is_taxable` or the quantity changes. That's a separate issue, not a regression.
  - **Tests:**
    - .NET: `ARInvoiceModuleTests.Create_TaxesTaxableCustomer`, `Create_NeverTaxesTaxExemptCustomer`, `CreateLine_NeverTaxesTaxExemptCustomer`.
    - e2e: `ar-invoices.spec.ts` › "invoice tax" (taxable and tax-exempt customers). The exempt test fails with the old code ("Expected: false, Received: true"). The BUG-018 known-bug test is retired.
- **BUG-019: AR invoice due dates come from the payment term's days.**
  - **Days live in `int_value`:** a payment term's `int_value` is its length in days (NET30 → 30). `UI/app/erp/ar/new/[id]/page.tsx` looks up the customer's term by key (`GetKeyValuesByModule`) and uses `int_value`. A term without one (e.g. "Due on Receipt") is due on the invoice date. The display name no longer matters.
  - **Dates:** the page now keeps them as `Date` objects, and formats `yyyy-MM-dd` only when saving. They used to round-trip through `toLocaleDateString()`, which also broke saving for non-US browser locales.
  - **API defaults** (`ARInvoiceModule.SeedPermissions`): Net 15/30/45/60 now store 15/30/45/60 (they stored a sort order, 1–4). A copy-paste bug is also fixed: Net 30 was only created when Net 15 was missing.
  - **Data migration `SetPaymentTermDays`:** sets `int_value` on the four default terms in existing databases (only those keys in the payment-terms module; custom terms are untouched). Verified on MySQL 8.0.44 against old API-style (1–4) and seeder-style (`NULL`) rows.
  - **Seeder:** its Net terms now carry their days.
  - **Admin → Lists:** new optional **Number** field when adding an entry, and an editable **Number** column, so admins can set days on new terms. Editing a cell used to send the edited value as the label whatever the column; it now updates the right field.
  - **Tests:**
    - .NET: `ARInvoiceModuleTests.SeedPermissions_PaymentTermsCarryTheirDays` and `…_CreatesNet30_WhenNet15AlreadyExists`.
    - e2e: `ar-invoices.spec.ts` › "invoice due date" (a term named "Sixty days net" gets 60 days; a term without days is due the same day). The "NET30" workaround is removed.
    - New `admin/lists.spec.ts`. The BUG-019 known-bug test is retired.
- **BUG-002: sales orders can be created; order ↔ payment relationship fixed.**
  - **Real root cause:** `PaymentConfiguration` declared `Payment.HasMany(order_headers).HasForeignKey(OrderHeader.id).HasPrincipalKey(Payment.order_header_id)`. That made `order_headers.id` a *foreign key* (so EF gave it no AUTO_INCREMENT) and added a unique constraint `AK_payments_order_header_id`, which allowed only **one payment per order**. `order_number`'s `ValueGeneratedOnAdd` was not the cause: `payments.payment_number` uses the same pattern and `payments.id` kept its identity. The app assigns order numbers itself.
  - **Model:** a payment now belongs to one order (`payments.order_header_id → order_headers.id`, `ON DELETE RESTRICT` so payments can't vanish with an order). The unused `Payment.order_headers` navigation is removed.
  - **Migrations** (`Shared/KosmosERP.Database/Migrations`):
    - `AddModulesTable`: the `modules` table (`Module` entity, read and written by `BaseERPModule` at API startup) was added in 5a47a22 without a migration, so a database built from migrations lacked it. This migration also pins the seed data's timestamps (they used `DateTime.UtcNow`, which rewrote those rows in every new migration).
    - `FixOrderHeaderPaymentRelationship`: drops the inverted FK and the unique constraint, makes `order_headers.id` AUTO_INCREMENT (with FK checks off around that one statement, because other tables reference the column), and adds the corrected FK.
  - **EF tooling:** new `ERPDbContextDesignTimeFactory` (connection from `KOSMOS_MIGRATIONS_CONNECTION`), so `dotnet ef` works without uncommenting code. See `Shared/KosmosERP.Database/Readme.md`.
  - **Seeder:** the `ALTER order_headers.id` workaround is gone, and seeding now runs with FK checks **on**. They're off only during `--reset`'s bulk delete.
  - **Verified on MySQL 8.0.44 (Docker), starting from an empty database:**
    - all three migrations apply
    - `order_headers.id` is `auto_increment`; the new FK is `RESTRICT`; the old FK and unique constraint are gone
    - the full seeder runs with FK checks on (25 orders, 15 payments, …), and so does `--reset` followed by a re-seed
    - a second payment on one order is accepted
    - a payment for a non-existent order is rejected
  - **Existing databases:** apply the migrations with `dotnet ef database update`. They don't touch data, apart from the seeded document-type timestamps.
- **BUG-026: API authorization.**
  - **The permission handler was broken for non-admin users** (`Api/Authorization/ErpCustomAuthorizationHandler.cs`):
    - its query read *every* user's role permissions, not the caller's
    - its logic was inverted: users who held a permission were denied, and missing permissions were allowed
    - users not in the database were denied instead of falling back to role claims

    Now: admins are allowed everything; `ERPAuthorize(..., "admin")` is admin-only; no required permissions means any signed-in user; otherwise every required permission must come from one of the caller's own (non-deleted) roles for the controller's module; unknown users (e.g. Keycloak) fall back to the attribute's role-claim check. **Behavior change:** non-admin users now get exactly their role permissions, so some requests that wrongly succeeded before will now fail.
  - **Status codes:** `[ERPAuthorize]` now returns **403** when a signed-in user is refused (it returned 401), and 401 only when there's no valid sign-in.
  - **Default-deny:** `Api/Program.cs` sets a fallback policy requiring a signed-in user on every endpoint without `[AllowAnonymous]`. The OpenAPI document (`MapOpenApi().AllowAnonymous()`) stays public so Swagger UI can load it.
  - **Endpoint rules:**
    - **Public, each with a comment saying why:** `User/AuthenticateUser` (login), SAML (SSO), Diagnostics `health` (probes), all Docs endpoints (printable documents render headless with no session; GUID-addressed), and `Settings/GetBaseSettings` (the company header on those documents).
    - **Admin-only:** Settings, Country and State create/update/delete; the User reads that were open (`GetUser`, `GetUserByGuid`, `GetUserBySessionId`, `GetRolePermissions`, `GetPermissionSet`, `FindUser`, `GetUsersByDepartment`; the UI doesn't call them); Diagnostics `test-binding` and `test-simple`.
    - **Any signed-in user:** Settings, Country and State reads; Notification reads; Reports `Catalog` and `ProductCategories`; GlobalSearch (switched from `[Authorize]` to the same convention).
    - **Module rules:** Opportunity `Find` (`crm_read`) and `Create` (`crm_create`), matching the controller's other actions.
  - **Tests:** `Tests/KosmosERP.Tests.Shared/AuthorizationTests.cs` (18 tests) covers the handler (including another user's role granting access, requiring every permission, deleted role assignments, admin-only, and the unknown-user fallback), the attribute's 401 and 403, and a **guard** that every controller action has `[ERPAuthorize]` or `[AllowAnonymous]` and that the public endpoints are exactly the intended list.
  - **Not verified against a running API:** there was no MySQL instance, so this is unit-tested only. Smoke-test sign-in, a non-admin user, and printing a document on a real deployment.
- **BUG-001: admin pages are gated on the admin permission.**
  - `admin/settings` and `admin/adjustments` checked `HasPermission(Admin, "")` (`"admin_"`, never granted). They now require `admin_read`.
  - Their Save buttons were enabled for everyone who got in. They now require `admin_edit` (Settings) and `transaction_write` (Adjustments), mirroring the API's `/Transaction/*` rules.
  - `admin/users`, `admin/roles`, `admin/lists` and `admin/document-types` had **no** check. They now require `admin_read` too, matching the admin landing page. The API already enforced admin on the user and role endpoints, so this was a UI consistency gap, not a security hole. The Settings API gap is BUG-026.
  - Tests: the catalog's `admin/settings` and `admin/adjustments` entries run again with content checks. `auth/guard.spec.ts` checks a non-admin is redirected from all 7 admin pages. New `e2e/tests/admin/settings.spec.ts` covers saving settings and a failed save.
  - The settings factory now sends `fiscal_year_start` as a `yyyy-MM-dd` string, as the API does.
- **BUG-004: freight carriers are their own lookup.** Carriers now live only under `KeyValueIds.FreightCarriers` (`2a2d1004…`), the id the UI already requested.
  - `ShipmentModule.SeedPermissions` creates UPS, FedEx and DHL when the API starts (from commit 62c6e02). Their keys are now spelled `freight_carrier_ups/fedex/dhl`, fixed from "frieght" before any data used them.
  - The dev seeder no longer adds carriers under shipping methods. Its `Kv.Carrier*` constants, which are stamped on seeded shipments, now use the new keys.
  - The admin Lists module label is fixed: "Frieght Company" → "Freight Carrier" (`UI/services/keyvalue-service.tsx`).
  - New regression test: `ShipmentModuleTests.SeedPermissions_CreatesFreightCarriers_Once`. The e2e fixtures (`UI/e2e/factories/lookups.ts`) use the new keys.
  - **Existing databases seeded before this change** still have `carrier_ups/fedex/dhl` rows under shipping methods, and shipments pointing at those keys. Re-seed them (`dotnet run --project Tools/KosmosERP.Seeder -- --reset` with the API stopped, then start the API) or clean them up by hand.
- **Icon-only AR invoice search button.** It had no accessible name (part of BUG-014). It now has `aria-label="Search AR invoices"` in `UI/components/ar-invoice-selector.tsx`.
