# Catlike Tower Defense

Output from following the [Catlike Coding Tower Defence tutorial](https://catlikecoding.com/unity/tutorials/tower-defense/), with a few tweaks:

* Unity 2021.x instead of 2018
* Use Inputsystem package instead of legacy Input module
* Use Universal Render Pipeline instead of legacy default render pipeline

And a few more "extra credit" things:

* Runtime UI for selecting towers/wall, current wave & spawn speed indicators, and lives left & kill counter - powered by UI Toolkit, including transition animations.
* Main menu with level select, implemented using additive scene loading & unloading
* [LDTK](https://ldtk.io/) integration for map editing for custom maps per scenario
* Custom animations for models (Mixamo anims retargeted using Mecanim avatar)
* Death particle effects that vary by enemy
* Win and loss state for each level, with graphical indicators
* Sound effects - mostly sourced from [Soniss' GDC](https://sonniss.com/gameaudiogdc) but also some custom made sound effects made in [SunVox](https://warmplace.ru/soft/sunvox/)

The main things missing for an actual game are:

* Some kind of constraint on how many towers you can build / how quickly (cooldown or gold supply)
* Indicator for where you can/can't build, and preventing building on tiles which enemies are already routing to (so that enemies never overlap)
* A way to abort an in-progress level (other than losing or winning)

## Developing

```shell
git lfs install
```

Then open Unity like normal.
