# Dolly Importer for Worlds

[![Latest Release](https://img.shields.io/github/release-date/Erallie/dolly-importer?display_date=published_at&style=for-the-badge&label=Latest%20Release)](https://github.com/Erallie/dolly-importer/releases/latest)
![Latest Downloads](https://img.shields.io/github/downloads/Erallie/dolly-importer/latest/total?sort=semver&style=for-the-badge&logo=Github&label=Latest%20Downloads)
![All-time Downloads](https://img.shields.io/github/downloads/Erallie/dolly-importer/total?style=for-the-badge&logo=GitHub&label=All-time%20Downloads)
<br>
[![Our Discord](https://img.shields.io/discord/1102582171207741480?style=for-the-badge&logo=discord&logoColor=ffffff&label=Our%20Discord&color=5865F2)](https://discord.gozarproductions.com)
[![Our Other Projects](https://img.shields.io/badge/Our%20Other%20Projects-%E2%9D%A4-563294?style=for-the-badge&logo=data%3Aimage%2Fwebp%3Bbase64%2CUklGRu4DAABXRUJQVlA4WAoAAAAQAAAAHwAAHwAAQUxQSGABAAABgFtbm5volyZTA%2BtibzK2H0w5sDkmhe3GmxrwxGg0839r%2FvkkOogIBW7bKB0c4%2BARYihzIqfd6dfO%2B%2B3XtHsq4jJhlIvcDRcgNB%2FeieQETorBHgghRtUYqwDs%2B4U4IpcvUB%2BVUPSK54uEnTwsUJoar2DeMpzLxQpeG5DH8lxyyfLivVYAwPBbkWdOBg3qFlqiLy679iHy9UDKMZRXmYxpCcusayTHG01K%2FEtatYWuj7oI9hL4BxsxVwhoP2mlAJJ%2BuuAflc6%2BEUCQTCX9EV87xBR2H75NxLZSpWiwzqdIm7ZO7uB3oEgZKbD9Nt3EmHweEPH1t1GNsZUbKeisiwjyTm5fA3SO1yCrADZXrV2PZQJPL1tjN4%2BxUL9ie1mJobzOnDwSx6ILiF%2FW%2BTUR4tcHx0UaV75JXC1a4g6Ky5dLcTSuy9q4HhTieF64Hy1A3GHB8gLLK2e92feuqnbfPK8IVlA4IGgCAACwDQCdASogACAAPk0cjEQioaEb%2BqwAKATEtgBOl7v9V3sHcA2wG4A3gD0APLP9jX9n%2F2jmqv5AZRh7J%2BN2fOx22iE%2F4TUsecFmY%2BSf1r%2BAP%2BTfzT%2FXdIB7KX7MtdIGr1A8H0jmrrfZvqButwOaYcLWYNRq5QgAAP7%2F%2FmIMpiVNn67QXpM1rrDmRS8Nr%2F6dhD%2Bq5e%2BM%2BAtUP1%2FxOj85Ol5y3ebjz%2BpHoOf%2FWW8a%2F2ojUaKVDkVqof%2Bv4f0f6ud8i58wusz%2Fyrj%2F%2BwnM3q0769dvK%2F%2BQe04xL49tkb9t6ylCqqezZtZGuGLJ%2F5iUrPqdYc%2F8VbYZfP%2FOpZP%2F4X4q%2BqS4gPOxzdINOe5PGv%2F0TS%2FJRf4LlFrFkrWtxlS8n40grV%2BKUu%2FiwzdQzImvwH81FxL1bZyTSsrYwMku1Pk9StTtWNjSR8ZWEYBH9eTn%2FvBERii5XaWOPJ%2FFVXtVQGbv%2BFRW5jbo9tfFDu%2BDHHf8LbgUd%2F8W8Id1AehBtRNsLQWbADmvF1QJU8x5tw%2FtTUwIoSaa%2F2jkcvyVHkAsb2qoIh1KF1pPdae%2BZaqjydy6nUa9agjrDk1G4pMhEUhH%2BV%2FIUe49MjhR%2FuxyFmwQ8dDogMyQ%2BdcSBa56Lwt1wyJ%2F22%2F5O98r6q6wiM63HyaYONd36W7br%2F0%2F6y2DZ3irAddj%2FRxntvr%2FbbChSYXAfEbO%2FD0G%2FFbMFqTHypodt9T6dAx%2BUjJYfHzFf%2FM3Ec%2FAtwbjc2gka6urN1MlSLb2VTS9Q5r8fkDzxZz6vu1OYUPUB1UFMIhYGvMATbxxoTmVhvpovzAc%2F8nbOjw3wAAA)](https://github.com/Erallie)
[![Donate](https://img.shields.io/badge/Donate-%24-563294?style=for-the-badge&logo=ko-fi&logoColor=FFFFFF&color=FF6433)](https://www.ko-fi.com/GozarProductions)

---

This is a Unity Editor tool for importing VRChat Camera Dolly paths from a VRChat-exported JSON file into a VRChat world so that your players can import it into their VRChat Cameras.

Instead of manually recreating camera paths and points in Unity, this importer reads the exported dolly JSON and automatically creates the corresponding **Paths** and **Points**, including their positions, rotations, and camera settings.

## Features

- Imports VRChat Camera Dolly JSON files directly into Unity
- Automatically creates all Paths and Points from the exported dolly
- Imports point position and rotation
- Imports camera settings stored on each point, including:
  - Zoom
  - Duration
  - Speed
  - Focal Distance
  - Aperture
  - Hue
  - Saturation
  - Lightness
  - Look At Me offsets
  - Exposure
- Supports multiple paths in a single dolly
- Can replace the paths of an existing dolly
- Automatically creates a new camera dolly from the included starting prefab when importing for the first time
- Includes a prefab toggle for the world so players can import the dolly into their VRC camera.
- Supports Unity Undo via `Ctrl + Z`

## Requirements

This tool is designed for **VRChat worlds** with a worlds SDK of 3.9.0 or greater.

## Installation

Start with a VRChat Worlds project managed through the VRChat Creator
Companion, with the Worlds SDK and UdonSharp available.

### Through VCC

Add the the listing to VCC [here](https://vcc.gozarproductions.com/), then add
**Dolly Importer** to your world project.

### Manual installation

Copy the package into:

`Packages/com.erallie.dolly-importer`

A release package ZIP contains the package's contents. Extract those
contents into the directory above.

Allow Unity to finish importing and compiling UdonSharp before
configuring the components.

## Opening the Dolly Importer

In Unity, open:

**Tools → Erallie → Dolly Importer → Import Dolly JSON Into Scene**

## Importing a Dolly

If this is your first time importing the dolly into the project:

1. Open **Tools → Erallie → Dolly Importer → Import dolly JSON Into Scene**.
2. Leave **Target Dolly** blank if importing the first time. Drag the existing dolly GameObject into **Target Dolly**.
3. Click **Browse...** and select the JSON file exported from VRChat.
4. Click **Import / Replace Dolly**.

> [!IMPORTANT]
> Importing into an existing dolly replaces its existing Paths and Points.

## After Importing

After the import finishes:

1. Select the dolly GameObject.
2. On the **Camera Dolly Animation** component, click **Collect Paths & Points**.
3. Configure the dolly's animation settings according to your liking.

For example, you may want to adjust its path type, loop behavior, speed settings, focus settings, or other Camera Dolly Animation options.

> [!IMPORTANT]
> Remember to click **Collect Paths & Points** after importing so that the Camera Dolly Animation component collects the newly created Paths and Points.

## Do Not Remove the Template Path and Point

This importer uses the first Path and the first Point inside it as templates when rebuilding an existing dolly.

Do **not** remove:

- The first Path
- The first Point inside the first Path
- The Camera Dolly Path component on the first Path
- The Camera Dolly Point component on the first Point

The hierarchy should contain at least:

```text
Dolly
└── Path
    └── Point
```

The required VRChat Camera Dolly components must remain attached to these objects.

Once the dolly has been imported, these objects will contain your actual imported path data. They still need to remain intact if you want to use that dolly as the target of a future import.

## Starting Prefab

The package includes a starting prefab containing the hierarchy and VRChat Camera Dolly components required by the importer.

When **Target Dolly** is left blank, the dolly importer automatically uses this prefab to create a new dolly in the scene.

You normally do not need to interact with or modify the starting prefab.

> [!WARNING]
> Do not modify or remove the required objects or camera dolly components from the starting prefab.

If the script detects that the starting prefab is missing the hierarchy or components necessary to perform an import, it will stop the import and ask you to **reimport the Dolly Importer package**.

## JSON Files

This tool is intended for JSON files exported by VRChat's camera dolly system.

The importer uses the exported Path and Point indexes to reconstruct the dolly hierarchy in Unity.

For example:

```text
Dolly
├── Path (0)
│   ├── Point (0)
│   ├── Point (1)
│   └── Point (2)
├── Path (1)
│   ├── Point (0)
│   └── Point (1)
└── Path (2)
    ├── Point (0)
    └── Point (1)
```

The number of Paths and Points is determined automatically from the JSON.

## Troubleshooting

### The importer says the starting prefab has been modified

The included starting prefab is missing one or more objects or components required by the importer.

Reimport the **Dolly Importer** package to restore the original starting prefab.

### My existing dolly cannot be imported into

Make sure the dolly still contains its first Path and first Point and that their VRChat Camera Dolly components have not been removed.

### The dolly does not follow the imported paths

After importing, select the dolly and click **Collect Paths & Points** on the Camera Dolly Animation component.

### I want to import a second dolly

Leave **Target Dolly** blank. The importer will automatically create a new dolly from the included starting prefab.

## Notes

This tool is an **Editor tool**. It is used to create and modify the camera dolly hierarchy while building your VRChat world and does not need to run inside VRChat.

The imported camera dolly itself uses VRChat's standard camera dolly components.