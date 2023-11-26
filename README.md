# Roguelike-Framework
A framework for roguelike games in the Unity game engine. 

The goal of this project is to provide an extremely extensible boilerplate to build roguelike games on. The core ideas were built around mimicking the gameplay of games like Brogue, Cogmind, Caves of Qud, and DCSS, so it should support almost any form of roguelike gameplay.

This project is actively maintained, but not frequently - I tend to develop new framework features that I need in the projects that use it, and then merge the improvements once I'm done.

<!-- FEATURES -->
## Features
* Premade components for monsters, items, abilities, tiles, and other core roguelike features
* An extensible dungeon generation framework, with async generation
* A coroutine-based game loop with automatic load balancing to prevent spikes in the framerate
* An Action framework that encourages monster actions to stack and re-use each other, based on [this amazing talk from Bob Nystrom](https://www.youtube.com/watch?app=desktop&v=JxI3Eu5DPwE)
* Inverted-control Utility AI (based on COQ talks)
* Dynamic effects that tie monsters, items, and abilities together. The same effect is allowed to affect either one instance of these objects, or affect a monster globally, allowing for effects like "All items that this monster holds now deal lightning damage" or "When this monster takes damage, its next spell is free and double range" to be < 10 lines of code
* A custom animation pipeline that automatically determines animation dependencies and sorts animations to produce visually consistent results. The AnimationController can also be configured to increase in speed based on queued inputs to prevent animation blocking. Based on the [Juice Your Turns](https://www.youtube.com/watch?v=xSYVQc7cH-4) talk from roguelike celebration
* A custom input pipeline with easy rebinding and support for multi-key inputs without blocking.
* Symmetric Shadowcasting LOS [(based on Albert Ford's algorithm)](https://www.albertford.com/shadowcasting/)
* A variety of weird (and cool) random distributions - rather than hack in dice rolls that build up to a normal distribution, you can just sample a real one. Most distributions let you set bounds and the mean.

<!-- ROADMAP -->
## Roadmap

- [x] Merge all stat variables into on container
- [ ] Passive Animation support
- [ ] Map improvements
    - [ ] Flyweight Design
    - [ ] Custom Neighbor selection
    - [ ] New rendering pipeline to support non-euclidean maps
        - [ ] Support for tile rendering
        - [ ] Support for target rendering
        - [ ] Support for animation/effect rendering
- [ ] UI Improvements
    - [ ] Object pooling for UI elements
    - [ ] Core UI is data driven, rather than manual.

See the [open issues](https://github.com/Woodmanan/RoguelikeFramework/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#readme-top">back to top</a>)</p>

<!-- CONTACT -->
## Contact

If you use this project and run into any issues, feel free to get in touch at:

[ReplaceBracketsWithMyAccountName]@Hotmail.com

or open up an issue.

<p align="right">(<a href="#readme-top">back to top</a>)</p>