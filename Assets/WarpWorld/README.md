# Warp World Services

## Crowd Control

### Setup

Add the `CrowdControl` behaviour to a `GameObject`. It can be either your main
game controller object or a standalone object dedicated for Crowd Control.

### Creating Effects

There are two types of effects to choose from: triggered and timed.

Triggered effects derive from `WarpWorld.CrowdControl.Effect` and invoke their
`OnTriggerEffect` method when ready to fire. They are best used with instant
actions, such as killing or teleporting the player, increasing or decreasing
lives and spawning enemies on screen.

Timed effects derive from `WarpWorld.CrowdControl.TimedEffect` and invoke their
`OnStartEffect` method when ready to fire and their `OnStopEffect` method when
their timer runs out. They are best used with actions affecting the game
temporarily, such as making the player invincible for 15 seconds, preventing
them from attacking for 10 seconds, or changing their run speed for 30 seconds.

Derived effect behaviours must then be placed on the same `GameObject`
containing the `CrowdControl` behaviour. Enabling/Disabling an effect behaviour
will prevent matching instances from firing, forcing them to retry instead. In
the case of `TimedEffect`, disabling or enabling the behaviour will also invoke
`OnPauseEffect` and `OnResumeEffect` respectively.

Disabling the `CrowdControl` behaviour will automatically stop any active effect
and cancel every effect currently queued. This can be used when returning to the
main menu for example.

### Using the Overlay

TBD

### Synchronizing Effects with the Server

TBD

### Getting a Game Key

TBD
