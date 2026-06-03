# Bean Battles Map Editor

[**Overview**](##overview) ·
[**Tutorial**](##tutorial) ·
[**Contributing**](##contributing)

## Overview

A Unity based map editor for Bean Battles.

The Bean Battles Map Editor allows you to create, export, and publish custom maps for Bean Battles.
It includes built in Bean Battles assets, map validation, Steam Workshop support.

Shout out to flarfo for the help and original idea.

## Tutorial
#### Video Tutorial : https://www.youtube.com/@gupagames/videos
### Getting Started

1. Download and extract the latest editor release zip: https://github.com/gupagames/bean-battles-map-editor/releases
2. Make a Unity account: https://login.unity.com/en/sign-in
3. Download Unity Hub: https://docs.unity.com/en-us/hub
4. Open Unity Hub and import the project from step 1.
5. Download the correct Unity editor version: https://unity.com/releases/editor/whats-new/2017.2.3f1#installs
6. Open the project.

#### Note: The first time opening the project may take some time.

### Making a Map

Unity is the game engine used to create Bean Battles. You only need to know the basics to create maps. If you have no experience, here are some tutorials:

* https://learn.unity.com/tutorial/get-started-with-the-unity-editor
* https://www.youtube.com/watch?v=HwI90YLqMaY&list=PLZ1b66Z1KFKhO7R6Q588cdWxdnVxpPmA8

There are builtin models/prefabs you can use from Bean Battles in:

```
GG/GameContent/... 
```

It is recommended to create a new folder for any new assets you add to the project. For example, your project window should look like:

```
GG/...
MyAssets/...
```

1. To make your own map, you must first create a new map. This is located at the top of the screen in the toolbar at `GG > Map Editor > Create New Map`. This should open your new map scene.

You can structure your map however you like, but the default map (recommended) is split into 3 parts in the hierarchy window: `MapSettings`, `Map`, and `Spawns`.

2. `MapSettings` is where all information/settings about your map are located. You must have a map name, author, and at least 1 valid stage.

#### Note: Hovering over each element in the inspector window will show a tooltip explaining what each field does.

3. `Map` is where you will create your actual map environment.

4. `Spawns` is where you decide where players, weapons, etc. will start. They are split into a few different categories. By default, 1 of each is already created for you.

   * Player Spawns (16 required): Spawns for players.
   * Team Spawns (5 required): Spawns where teams of players start.
   * Vehicle Spawns (optional): Spawn points for vehicles.
   * Weapon Spawns (1 required): Spawn points for weapons. If you have fewer spawns than weapons, multiple weapons will drop on one spawn.
   * Winner Stand (1 required): Where the end podium will be located.
   * Default Camera (1 required): Where the lobby camera will be located.

   #### Note: Make sure spawns do not clip into any colliders to avoid complications.

5. After you finish building your map, set up MapSettings, and place all spawns, you can export and play your map.

If you are unsure where to begin when building your map, there is an example map located in:

```
GG/GameContent/Scenes/...
```

### Exporting and Publishing

You can export/publish your map from the toolbar: `GG > Map Editor > Export/Publish`

1. **Export > As Map**
   Saves the map locally to your Bean Battles map directory. You can share this `.bbmap` file with friends. They will need to place it in their map directory folder, which can be accessed from Bean Battles.

2. **Publish > As Workshop Item**
   Publishes your map to the Steam Workshop where anyone can subscribe to it. This is recommended once your map is finished.

3. You can also export/import your map projects. If you do this, any assets you add must be outside the `GG` folder.

#### Note: If you are unable to export/publish your project, map validation is likely failing. Read the error messages to understand what you need to fix.

## Contributing

See [Contributing](CONTRIBUTING.md)
