# Bin11

Лёгкая трей-утилита для Windows 11: иконка возле часов показывает заполненность корзины кольцевым индикатором с автоматическим цветом (белый/чёрный круг под тему системы, зелёный → жёлтый → красный по проценту заполнения).

> **⚠️ Для запуска требуется [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0).**
> Обычно на Windows 11 он уже стоит, но если приложение не запускается — почти всегда причина в этом.

---

## Скриншоты

| Пустая корзина | Заполненная корзина |
| :---: | :---: |
| ![Пустая корзина](screenshots/1.png) | ![Заполненная корзина](screenshots/2.png) |

---

## Возможности

- 🖱️ Правый клик → меню (Открыть корзину / Очистить корзину / Настройки / Выход), двойной левый клик → очистка.
- 📏 Порог «100%» — это **реальный лимит корзины**, настроенный в самой Windows (реестр `BitBucket\Volume\{GUID}\MaxCapacity` для каждого диска — то же значение, что видно в Свойствах корзины).
- 🔒 Защита от повторного запуска.

---

## Установка

Готовый `Bin11.exe` лежит в разделе **[Releases](../../releases)** — достаточно скачать и запустить. Иконка появится в трее рядом с часами; если её не видно, она прячется за стрелочкой «скрытых значков» на панели задач — просто перетащи её на видимую часть.

Единственное, что может понадобиться — установленный **.NET 8 Desktop Runtime** (ссылка в шапке). На большинстве машин под Windows 11 он уже присутствует, так что в типичном случае никаких дополнительных действий не требуется.

---

# Bin11 (English)

A lightweight tray utility for Windows 11: the icon next to the clock shows how full the Recycle Bin is with a ring indicator and automatic coloring (white/black track matching the system theme, green → yellow → red based on fill percentage).

> **⚠️ Requires [.NET 8 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet/8.0).**
> It's usually already installed on Windows 11, but if the app won't start — this is almost always the reason.

---

## Screenshots

| Empty Recycle Bin | Filled Recycle Bin |
| :---: | :---: |
| ![Empty Recycle Bin](screenshots/1.png) | ![Filled Recycle Bin](screenshots/2.png) |

---

## Features

- 🖱️ Right click → menu (Open Recycle Bin / Empty Recycle Bin / Settings / Exit), double left click → empty.
- 📏 The "100%" threshold is the **actual Recycle Bin limit** configured in Windows itself (registry `BitBucket\Volume\{GUID}\MaxCapacity` per drive — the same value shown in Recycle Bin Properties).
- 🔒 Single-instance protection.

---

## Installation

The ready-to-run `Bin11.exe` is available in the **[Releases](../../releases)** section — just download and run it. The icon will appear in the tray next to the clock; if you don't see it, it's hidden behind the "show hidden icons" arrow on the taskbar — just drag it onto the visible area.

The only thing you may need is the **.NET 8 Desktop Runtime** (link at the top). On most Windows 11 machines it's already there, so in the typical case no extra steps are required.
