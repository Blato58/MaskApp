Below is a concrete iPhone UI/UX design for the app. I would structure it as **Library**, **Pages**, and **Device** tabs. The design should feel like a native iOS utility app: clear, fast, bottom tab navigation, large tap targets, sheets for editing, and contextual edit modes. Apple’s HIG describes tab bars as the right pattern for switching between top-level app sections, and search fields can use filtering scope/tokens for refining results. ([Apple Developer][1])

---

# App Structure

## Bottom Tabs

**Tab 1: Library**
For all reusable items. Search, filter, group, reorder, and edit item definitions.

**Tab 2: Pages**
For iOS-style swipeable pages made from smaller colored item tiles.

**Tab 3: Device**
For Bluetooth/device connection, scanning, status, brightness, and device settings.

Suggested tab icons:

| Tab     | Label     | SF Symbol                                                              |
| ------- | --------- | ---------------------------------------------------------------------- |
| Library | `Library` | `square.grid.2x2`                                                      |
| Pages   | `Pages`   | `rectangle.on.rectangle`                                               |
| Device  | `Device`  | `antenna.radiowaves.left.and.right` or `dot.radiowaves.left.and.right` |

---

# Global Visual Style

Use a clean iOS card/grid interface.

**Base design**

* Background: grouped iOS background.
* Cards: rounded rectangles, 16–20 pt corner radius.
* Primary action color: app accent color.
* Spacing: 16 pt screen padding, 12 pt grid gaps.
* Navigation title: large title on main screens.
* Main actions: top-right toolbar buttons.
* Edit/manage actions: contextual, not always visible.
* Destructive actions: red, hidden behind confirmation sheets.

**Recommended modes**

Use a segmented control near the top of each complex tab:

* Library: `Browse | Arrange`
* Pages: `Use | Manage`
* Device does not need a mode switch.

This keeps the app understandable. The user always knows whether they are using content or organizing it.

---

# Tab 1 — Library

## Purpose

The Library tab is the master collection of all items. Items are displayed as **two-column square cards**. Users can search, filter, group, reorder, and edit them.

## Screen Layout

```text
Library
[ Search items...                 ]

[ Browse | Arrange ]

[ Filter ] [ Group: Category v ] [ + Add ]

Group: Favorites
┌──────────────┐ ┌──────────────┐
│ Item Name    │ │ Item Name    │
│ Preview/info │ │ Preview/info │
│ Badge        │ │ Badge        │
└──────────────┘ └──────────────┘

Group: Memes
┌──────────────┐ ┌──────────────┐
│ Item Name    │ │ Item Name    │
│ Preview/info │ │ Preview/info │
│ Badge        │ │ Badge        │
└──────────────┘ └──────────────┘
```

## Library Browse Mode

This is the default mode.

### Top area

* Large title: **Library**
* Search field: `Search items`
* Segmented control: `Browse | Arrange`
* Filter/group row:

  * `Filter`
  * `Group: None / Type / Category / Favorite / Custom`
  * `+ Add`

### Item Card

Each item is a two-column square card.

Suggested card content:

```text
┌────────────────────┐
│ Name               │
│                    │
│ Short preview      │
│                    │
│ Type · Color · Tag │
└────────────────────┘
```

For your app, the card could show:

* Item name
* Short preview text
* Icon or type badge
* Color indicator
* Favorite star
* Last-used indicator
* Device compatibility indicator if relevant

### Card interactions

| Action             | Behavior                                            |
| ------------------ | --------------------------------------------------- |
| Tap card           | Use/select/send/open item depending on app behavior |
| Long press         | Context menu                                        |
| Swipe/context menu | Edit, Duplicate, Favorite, Move to Group, Delete    |
| `+ Add`            | Opens Add Item sheet                                |
| Search             | Filters cards live                                  |
| Filter             | Opens bottom sheet                                  |
| Group              | Opens grouping menu                                 |

### Context menu

```text
Edit
Duplicate
Add to Page
Move to Group
Favorite / Unfavorite
Delete
```

## Library Arrange Mode

This mode is for organization.

```text
Library
[ Search items... ]

[ Browse | Arrange ]

Done

Group: Favorites        [ Manage ]
☰ ┌──────────────┐ ☰ ┌──────────────┐
  │ Item Name    │   │ Item Name    │
  │ Preview      │   │ Preview      │
  └──────────────┘   └──────────────┘

Group: Memes            [ Manage ]
☰ ┌──────────────┐ ☰ ┌──────────────┐
  │ Item Name    │   │ Item Name    │
  │ Preview      │   │ Preview      │
  └──────────────┘   └──────────────┘
```

### Arrange mode behavior

* Cards become draggable.
* Groups become draggable/reorderable.
* Each group has a `Manage` button.
* Each item has a small drag handle.
* Tapping a card should not activate it in this mode.
* Long press starts drag.
* `Done` exits arrange mode.

### Manage button

The `Manage` button should open a sheet for the selected item or group.

For an item:

```text
Edit Item
Name
Preview/content
Icon
Color
Group
Favorite
Delete Item
```

For a group:

```text
Manage Group
Group name
Color
Sort items
Delete group
Move all items to...
```

## Add/Edit Item Sheet

Use a bottom sheet or full-screen modal depending on complexity.

```text
Add Item

Name
[ Text field ]

Content / Info
[ Text area ]

Icon
[ Icon picker ]

Color
[ Color picker ]

Group
[ Group picker ]

Options
[ ] Favorite
[ ] Show on quick pages

Cancel                      Save
```

For a small app, use a sheet. For a complex item editor, use a full-screen form.

---

# Tab 2 — Pages

## Purpose

The Pages tab lets the user manage and use iOS-style screen pages. A page contains smaller colored square tiles with icons and short names. In normal mode, the user swipes through pages. In manage mode, the user adds/removes pages and adds/removes/reorders page items.

## Main Layout

```text
Pages

[ Use | Manage ]

Page dots / page selector

┌────────────────────────────┐
│ Page: Main                 │
│                            │
│ ┌────┐ ┌────┐ ┌────┐ ┌────┐ │
│ │ 🔥 │ │ 😎 │ │ ⚡ │ │ ❤️ │ │
│ │Hey │ │Meme│ │Fast│ │Fav │ │
│ └────┘ └────┘ └────┘ └────┘ │
│                            │
│ ┌────┐ ┌────┐ ┌────┐ ┌────┐ │
│ │ 🐱 │ │ 🎵 │ │ 👀 │ │ +  │ │
│ │Cat │ │Song│ │Look│ │Add │ │
│ └────┘ └────┘ └────┘ └────┘ │
└────────────────────────────┘
```

## Pages Use Mode

This is the default mode.

### Behavior

* User swipes left/right between pages.
* Page indicator dots are visible.
* Each page is a grid of small colored square tiles.
* Tapping a tile activates/uses that item.
* Long press opens quick actions.

### Tile design

Small square tile:

```text
┌────────┐
│  Icon  │
│ Name   │
└────────┘
```

Recommended tile content:

* Icon centered
* Short name, max 1–2 words
* Background color
* Optional tiny status badge

### Tile interaction

| Action           | Behavior                                |
| ---------------- | --------------------------------------- |
| Tap tile         | Use/send item                           |
| Long press       | Preview, Edit, Remove from Page         |
| Swipe page       | Move between pages                      |
| Tap page dot     | Jump to page                            |
| Pull down search | Optional quick search across page items |

## Pages Manage Mode

```text
Pages

[ Use | Manage ]

[ + Page ] [ Add Items ] [ Edit Page ]

Page: Main                         [ ... ]

┌────┐ ┌────┐ ┌────┐ ┌────┐
│ 🔥 │ │ 😎 │ │ ⚡ │ │ +  │
│Hey │ │Meme│ │Fast│ │Add │
└────┘ └────┘ └────┘ └────┘
  ×      ×      ×

Page controls:
[ Rename Page ] [ Reorder Pages ] [ Delete Page ]
```

### Manage mode actions

* Add page
* Rename page
* Delete page
* Reorder pages
* Add items to page
* Remove items from page
* Reorder tiles inside a page
* Change tile color/icon/name override

### Page toolbar

Use compact buttons:

```text
+ Page     Add Items     Edit Page
```

Or a single `+` menu:

```text
New Page
Add Items to Current Page
Import from Library
```

## Add Items to Page Flow

The cleanest flow:

1. User taps `Add Items`.
2. A searchable Library picker opens.
3. User selects multiple items.
4. User taps `Add`.
5. Items appear on current page.

```text
Add Items to Page

[ Search library... ]

○ Item A
○ Item B
● Item C
● Item D

Cancel                     Add 2
```

## Add Page Flow

```text
New Page

Page name
[ Text field ]

Page color
[ Color picker ]

Icon
[ Icon picker ]

Create
```

## Delete Page Flow

Use confirmation:

```text
Delete “Memes” page?

This removes the page layout only. Library items will not be deleted.

Cancel                     Delete Page
```

Important UX rule: deleting a page should not delete the underlying library items.

---

# Tab 3 — Device

## Purpose

The Device tab is for connecting to the hardware/device, scanning, showing connection state, and adjusting brightness.

## Main Layout

```text
Device

Connection
┌────────────────────────────┐
│ ● Connected                │
│ LED Mask / Device Name     │
│ Battery: 82%               │
│ Signal: Good               │
└────────────────────────────┘

[ Disconnect ]

Brightness
[ ━━━━━━━━━━━━━━━●──── ]

Device Controls
[ Scan for Devices ]
[ Auto-connect        on ]
[ Remember Device     on ]

Advanced
Firmware: 1.0.3
Last sync: Today 18:42
```

## Disconnected State

```text
Device

Not Connected
Connect to your device to use Library and Pages.

[ Scan for Devices ]

Saved Device
LED Mask
Last connected yesterday
[ Connect ]
```

## Scanning State

```text
Scanning...

Nearby Devices
┌────────────────────────────┐
│ LED Mask                   │
│ Strong signal              │
│                         Connect
└────────────────────────────┘

┌────────────────────────────┐
│ Unknown Device             │
│ Weak signal                │
│                         Connect
└────────────────────────────┘

[ Stop Scanning ]
```

## Connected State

Show connection status prominently.

Recommended status colors:

* Connected: green
* Connecting: orange/blue progress
* Disconnected: gray/red
* Error: red

### Connection card content

```text
Connected
Device name
Battery if available
Signal strength
Last command status
```

### Brightness control

Use a native slider:

```text
Brightness
Low ━━━━━━━━━━━━━━━●━━ High
```

Add quick presets:

```text
25%   50%   75%   100%
```

### Auto-connect settings

```text
Connection Settings

Auto-connect to last device       on
Reconnect automatically           on
Show connection notifications     off
```

## Device Error State

```text
Couldn’t connect

Make sure the device is powered on and nearby.

[ Try Again ]
[ Forget Device ]
```

---

# Recommended Navigation Model

## Library

* Main screen: grid
* Add/edit item: sheet or full-screen form
* Filters: bottom sheet
* Group management: sheet
* Item details: sheet

## Pages

* Main screen: page swiper
* Manage mode: same screen, controls exposed
* Add items: searchable picker sheet
* Edit page: sheet
* Reorder pages: full-screen list or drag-enabled sheet

## Device

* Main screen: connection dashboard
* Scan devices: sheet or inline list
* Advanced settings: pushed detail screen

---

# UX Rules

## 1. Do not mix “use” and “manage” too much

The app has many management features. The biggest risk is clutter.

Use mode separation:

* Library: `Browse | Arrange`
* Pages: `Use | Manage`

When the user is in normal mode, hide reorder/delete controls.

## 2. Make destructive actions reversible or confirmed

Deleting groups, pages, or items should require confirmation.

Better:

* Removing from page ≠ deleting from Library.
* Deleting item from Library should warn that it removes it from all pages.

## 3. Use sheets for focused tasks

Good sheet tasks:

* Add item
* Edit item
* Rename group
* Add page
* Add items to page
* Scan devices

Good full-screen tasks:

* Complex item editor
* Bulk reorder pages
* Advanced device settings

Apple describes sheets as suitable for focused tasks that users complete before returning to the parent view. ([Apple Developer][2])

## 4. Keep grid cards highly scannable

Cards should not show too much text.

For Library cards:

* Name
* Preview
* 1–2 badges

For Page tiles:

* Icon
* Short name

## 5. Make empty states useful

### Empty Library

```text
No items yet

Create your first item or import a preset pack.

[ Add Item ]
```

### Empty Page

```text
This page is empty

Add items from your Library.

[ Add Items ]
```

### No Device

```text
No device connected

Scan nearby devices to start using the app.

[ Scan for Devices ]
```

---

# Concrete Screen Names

## Tab 1: Library

* `LibraryScreen`
* `LibraryBrowseMode`
* `LibraryArrangeMode`
* `ItemEditorSheet`
* `GroupEditorSheet`
* `FilterSheet`

## Tab 2: Pages

* `PagesScreen`
* `PageUseMode`
* `PageManageMode`
* `PageEditorSheet`
* `AddItemsToPageSheet`
* `ReorderPagesScreen`

## Tab 3: Device

* `DeviceScreen`
* `DeviceScannerSheet`
* `DeviceSettingsScreen`
* `BrightnessControl`
* `ConnectionStatusCard`

---

# Suggested App Copy

## Tab names

Use:

```text
Library
Pages
Device
```

Avoid:

```text
Items
Screens
Bluetooth
```

`Library`, `Pages`, and `Device` explain the mental model better.

## Mode labels

Use:

```text
Browse | Arrange
Use | Manage
```

Avoid:

```text
Normal | Edit
Default | Edit
Mode 1 | Mode 2
```

`Browse`, `Arrange`, `Use`, and `Manage` describe what the user is doing.

---

# Minimal Wireframe Summary

```text
TAB 1 — LIBRARY

Library
[ Search items... ]
[ Browse | Arrange ]
[ Filter ] [ Group ] [ + Add ]

Group Header
[ Item Card ] [ Item Card ]
[ Item Card ] [ Item Card ]


TAB 2 — PAGES

Pages
[ Use | Manage ]

< swipeable page >
[ Tile ] [ Tile ] [ Tile ] [ Tile ]
[ Tile ] [ Tile ] [ Tile ] [ + ]

Page dots


TAB 3 — DEVICE

Device

[ Connection Status Card ]

Brightness
[ Slider ]

[ Scan for Devices ]
[ Auto-connect toggle ]
[ Remember Device toggle ]
```

---

# My Recommended Final UX

Use this structure:

1. **Library tab** is the database of all items.
2. **Pages tab** is the fast-use launcher made from those items.
3. **Device tab** is only hardware status and controls.
4. Never make the user wonder whether they are editing or using something.
5. Use segmented controls for mode switching.
6. Use sheets for editing and pickers.
7. Keep normal mode clean; show management controls only in management modes.

This gives you a simple app that can scale without turning into a cluttered settings panel.

[1]: https://developer.apple.com/design/human-interface-guidelines/tab-bars?utm_source=chatgpt.com "Tab bars | Apple Developer Documentation"
[2]: https://developer.apple.com/design/human-interface-guidelines/sheets?utm_source=chatgpt.com "Sheets | Apple Developer Documentation"
