# Finish and results update

The Windows build is `Build/Aether Grounds.exe`.

Finish detection uses a swept front-bumper crossing against the visible finish plane. Circuit laps require three ordered quarter-course sections, preventing repeated line crossings from granting laps while avoiding the old dependency on accumulated route-point distance after recovery. Rush uses its final gate, and Heartstopper accepts an airborne finish after the first five checkpoints. Recovery resets the crossing sample so a teleport cannot count as a crossing.

The player gets an immediate FINISH animation, placement reveal, two confetti cannons and a generated musical fanfare. This transitions into a rendered car show, then a results screen. The top three actual finishers appear on rotating gold, silver and bronze pedestals with the existing brushed-metal texture, metallic shading, captured reflections, softboxes and moving volumetric spotlights. Each participant has a portrait rendered from their customized car. Portrait and podium render targets are released when leaving or restarting.

Circuit results show lap splits and best lap. Rush shows medal time targets and rewards. Heartstopper shows checkpoint completion and air time. The standings scroll and selecting a driver displays that driver's statistics. Rewards has a separate detail panel. Opponents can finish while results are shown, until the Rush deadline or 60 seconds after the player finishes in other modes. Unfinished racers receive no invented times or podium places.

Per-racer statistics: top speed, out-of-bounds time, distance, air time, drift time, boost time, destroyed props, civilian hits, unique traffic cars hit, power-ups used, collisions, resets, lap times, and coins. Statistics stop when the racer finishes. Destroyed props and civilians receive the responsible vehicle from collisions or supported weapon blasts; repeated contacts do not repeatedly award the same destruction or knockdown. Distance derives from physical velocity and excludes recovery teleports.

Verification: `-aetherFinishTest` runs crossing, lap, attribution, rewards, timeout, portrait and cleanup checks across all three circuits, three Rush courses and Heartstopper. Screenshots and the verification log are under `Evidence/Finish` and `Evidence/finish-preview.log`. Test mode does not persist garage or reward changes. `-aetherTest` is the existing full driving smoke test.
