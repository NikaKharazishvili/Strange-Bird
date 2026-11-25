# Strange Bird

A Flappy Bird-inspired 2D game built in Unity, with a skill system, a cosmetics shop and a day/night cycle.

[Download on itch.io](https://nikson212.itch.io/strangebird) · [Watch on YouTube](https://www.youtube.com/watch?v=Wz_O6TOYG9Y)

[![Watch on YouTube](https://img.youtube.com/vi/Wz_O6TOYG9Y/maxresdefault.jpg)](https://www.youtube.com/watch?v=Wz_O6TOYG9Y)

## Features
- **Core gameplay**: flap through scrolling obstacles and collect coins.
- **Skills** (upgraded in the shop, 3 levels each):
  - *Greed*: higher levels give a higher chance to collect double coins.
  - *Shield*: makes the player invulnerable for 2 seconds. Higher levels reduce its cooldown.
- **Customization**: unlockable birds, backgrounds and obstacle styles.
- **Progression**: coins, high score and purchases are saved between sessions.
- **Dynamic environment**: day/night cycle and decorative birds that sometimes chat.
- **Game speed**: Slow, Medium and Fast modes change movement speed, spawn rate and score gain.
- **Options**: rebindable flap key, sound toggle, FPS counter, optional background birds.

## Controls
Flap with the Space key (rebindable in Options) or the left mouse button. After dying, press the flap key to play again.

## Project structure
```
Assets/
  Art/         Sprites (sprite atlas packs Art/Sprites) and bird skin animation frames
  Audio/       Music and SFX
  Animations/  Animator controllers and clips
  Scenes/      Menu, Game
  Scripts/
    Core/        Constants (tuning values), PrefKeys (save keys), Utils (shared helpers)
    Gameplay/    Player, Whoosh effect, Movables (Bird, Coin, Obstacle)
    Managers/    GameManager (a round), MenuManager (menu, shop and options)
    UI/          Button effects, custom cursor and tooltips
```

## Technical notes
- Pooled gameplay objects (obstacles, coins, birds, whoosh effects) share the `Movable` base class.
- Game speed and skill levels index into arrays in `Constants`, so tuning needs no code changes.
- Release builds rename private methods and fields after compilation (`CodeObfuscator`). Use `[DontObfuscate]` to exclude a member.

## Requirements
Unity 6000.3.12f1. Open the project, load `Assets/Scenes/Menu.unity` and press Play.

## License
Copyright (c) Nikson. All rights reserved.

This repository is provided for viewing and evaluation only. No part of it, including code, art, audio and other assets, may be copied, modified, redistributed or used in other projects without explicit written permission. Third-party assets remain the property of their respective owners.
