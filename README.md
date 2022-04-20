# Catlike Tower Defense

A shapes & pirates themed tower defense game.

You can play a web version of it at https://caspark.github.io/catlike-tower-defense/ , or download a release from https://github.com/caspark/catlike-tower-defense/releases :)

* Note the speed controls in the top right.
* You get more towers every few seconds.

## Details

This was based on the [Catlike Coding Tower Defence tutorial](https://catlikecoding.com/unity/tutorials/tower-defense/), with many changes and extensions to turn it into an actual game of sorts:

* Unity 2021.x instead of 2018
* Use Inputsystem package instead of legacy Input module
* Use Universal Render Pipeline instead of legacy default render pipeline
* Cinemachine-based camera controller with right-click rotating, wasd/arrow/middle-click panning, scroll wheel zooming
* Runtime UI for selecting towers/wall, current wave & spawn speed indicators, and lives left & kill counter - powered by UI Toolkit, including transition animations.
* Main menu with level select, implemented using additive scene loading & unloading
* [LDTK](https://ldtk.io/) integration for map editing for custom maps per scenario
* Custom animations for models (Mixamo anims retargeted using Mecanim avatar)
* Death particle effects that vary by enemy
* Win and loss state for each level, with graphical indicators
* Sound effects - mostly sourced from [Soniss' GDC](https://sonniss.com/gameaudiogdc) but also some custom made sound effects made in [SunVox](https://warmplace.ru/soft/sunvox/)
* A cooldown for how quickly you can build various towers (gain towers over time)
* Per level configuration for health and starting tower availability

It works as a game, however there are 2 things that would help it:

1. Preventing building on tiles which enemies are already routing to (so that enemies never overlap with towers/walls)
2. Actual game balance and design tuning :D

## Developing

```shell
git lfs install
git lfs pull
```

Then open Unity like normal.
