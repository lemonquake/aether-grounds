# Aether Grounds

A Windows, Android, and iPhone racing game by Aljay Leodones, built in Unity 6 with editable Blender assets.

## Airah Mountains

Choose **Game Setup → Airah Mountains → Play**, or select it in **Play → Circuit races**. This fourth circuit is a 10.38 km mountain lap with 385 metres of elevation variation, six overlooks, climbing bends, long steel trestles, sunny afternoon lighting, a visible sun and angle-dependent glare. It supports the existing 0–19 opponents and 1–5 lap settings.

The landscape includes 12,554 instanced trees, 5,746 rocks, ferns, fallen logs, picnic shelters and generated granite/moss and meadow textures. Two additional Blender tree models provide inexpensive distant forest silhouettes. Original editable models are in `ArtSource/AirahMountains.blend`; texture prompts and provenance are in `ArtSource/Airah-art-prompts.md`.

A long broken bridge has two road decks separated by a real gap of approximately 86 metres. Use the green boosters on the takeoff deck and the purple launcher at its edge to reach the landing deck. The bridge-specific pads provide the required launch speed; ordinary boost and jump tiles elsewhere retain their usual behavior. Missing the jump or resetting over its gap returns to the approach. Eighteen physical civilian cars patrol the circuit and turn back before the broken bridge. Look around Summit View for the small **made by Aljay Leodones** sign.

Scenery uses spatial batching, GPU instancing, distance culling, distant tree models and compressed mipmapped textures. PC targets 120 FPS; Phone controls/quality target 60–120 FPS according to the display refresh rate, with shorter shadows and reduced tree detail. Controls includes an optional 60 FPS cap. Desktop phone-quality measurements do not establish FPS on a physical Android device. Runtime checks and captures are in `Evidence/Airah`; use `-aetherAirahTest` for the full route test, or add `-aetherAirahQuick` to finish after geometry, performance, jump and recovery checks. Verification does not write the player's garage or credit saves.

## STUNTS, Store and impact update

Choose **Play → STUNTS → Play Heartstopper**. This solo, 4.8 km course starts 1,600 metres above the ocean, descends 1,000 metres, then climbs into a 600-metre gap to the next island. The second descent includes boost, slippery, jump, water and timed trap tiles, followed by another island jump and a curved finish. Wide landing decks give room to steer in flight. There is no time limit. Pass all six checkpoints in order while on the road to finish; R and automatic recovery return to the last earned checkpoint. Finishes grant credits and four upgrade parts, with a saved best time.

On the ground, use the usual driving controls. In the air, A/D turn and bank, W pitches down and S pitches up; release controls to settle. Phone steering turns and banks, while Accelerate/Brake control pitch. Boost approach speed is limited near the two launch ramps to keep the landing decks reachable. The road uses additional adhesion on the steep descents.

Civilian cars weigh 220–310 kg, jeepneys 380 kg, tricycles 120 kg, and Rush traffic 240 kg. Vehicle collisions launch light traffic and throw additional sparks, fragments and smoke. Rush traffic no longer locks rotation or overwrites collision velocity; damaged traffic has a 20-second driving interruption. Sidewalk residents have 45 kg physical bodies, tumble when struck, and emit red droplets and mist. They return after 24 seconds when their home is clear.

Shields double the car's actual collision mass, strengthen impacts against vehicles and breakable props, and restore normal mass when the shield ends. Suspension support scales with the shield's mass. Pulse has a 22-metre radius and a distance-dependent launch strength. Weapon pushes suspend automatic rollover for 1.5 seconds. Gravity mines remain until triggered and detonated; oil puddles retain their 12-second lifetime. Match cleanup removes the previous match's hazards.

**Store** replaces Support. Every product has a rendered 3D preview, with a rotating detail view, drag rotation and wheel zoom. Bonuses shown in the Store apply to the equipped saved car and appear in Garage statistics. Equipment ownership is still verified by player-bound signed codes, and existing purchases remain valid. Checkout fulfillment remains manual.

| Product | USD | Speed bonus | Acceleration / braking / grip bonus |
| --- | ---: | ---: | --- |
| Pip City Edition | 0.99 | 10.8 km/h | +1.5 / +2 / +0.5 |
| Comet Chrome Edition | 2.99 | 21.6 km/h | +2 / +3 / +0.6 |
| Nomad Night Edition | 5.99 | 18 km/h | +2.5 / +4 / +1.4 |
| Manta Gold Edition | 9.99 | 36 km/h | +3 / +4 / +1.2 |
| Golden Vanta | 9.99 | 43.2 km/h | +3.5 / +4 / +1.1 |
| Chrome Vanta | 7.99 | 32.4 km/h | +2.8 / +6 / +1.7 |
| Neon wheel kit | 0.99 | — | — / — / +0.5 |
| Starlight trail | 1.99 | — | +1 / — / — |
| Glass wheels | 2.99 | — | — / +2 / +1.1 |
| Glass chassis | 4.99 | 14.4 km/h | +2 / — / — |

Glass wheels keep their rubber tires and add transparent rims. Glass chassis replaces body panels with a transparent reflective material and adds visible frame rails and a battery. Both fit every saved body. These are modeled finishes; glass does not change the car's collider into a fragile object.

Run the rendered Windows player with `-aetherStuntTest -logFile <path>` for the new runtime checks, product screenshots and a continuous driving test. Evidence is saved to `Evidence/Stunts`; test mode does not persist garage, inventory, best-time or credit changes. `before-stunts.zip` contains the prior scripts, shaders and editor tools.

Windows: run **Build/Aether Grounds.exe**. Android: install **Build/Android/Aether-Grounds.apk** (Android 8.0 or newer, ARM64 phones with Vulkan or OpenGL ES 3.1 or newer). This is a locally signed, installable APK. Keep its signing key for future updates; a store release needs a dedicated release signing key. The Unity project is this folder; its scene is `Assets/Scenes/AetherGrounds.unity`. Blender source is `ArtSource/AetherGrounds.blend`.

## Play

**Rush Challenges** is the first mode on the Play screen. Select a map, then **Play Rush**. These are long, straight, point-to-point races with a time limit, moving traffic, ramps, boost tiles, roadblocks, coins, and an 80% offensive power-up pool. Circuit races remain available beside Rush; their settings are separate.

| Rush map | Length / limit | Rivals | Distinct hazards | Signature reward |
| --- | --- | --- | --- | --- |
| Sunset Coast — Epic Drag Race | 6 km / 180 seconds | 7 | Oil lanes, opposing traffic, beach ramps | Solar Turbo |
| Redrock Run — Canyon Gauntlet | 7.5 km / 210 seconds | 9 | Warned falling boulders, staggered barricades | Redrock Plating |
| Stormbreak — Thunder Sprint | 9 km / 245 seconds | 11 | Cycling electric lanes, bridge traffic, rain | Storm Coil |

Finish within the limit to keep the rewards. Gold requires a top-three finish within 110 / 140 / 165 seconds respectively. Silver requires finishing within 140 / 170 / 195 seconds. Other successful finishes earn Bronze. Base awards are 900 / 1,200 / 1,500 credits, plus 200 per medal tier above Bronze and 10 per collected coin. Finishes also grant 3–5 upgrade parts, one Rocket Pack, and a 35% chance of a Shield Pack. Each map's equipment is guaranteed on the first finish; later duplicates become two extra upgrade parts. There is no entry fee or loss of owned equipment on timeout. Prepared starting items are consumed when the countdown reaches Go, including after restarting.

The **Inventory** tab stores equipment, attachments, consumables and upgrade parts. Two shared slots apply to every garage car: Engine and Chassis. Solar Turbo improves acceleration and top speed; Storm Coil reduces boost energy use by 20%; Redrock Plating reduces incoming unshielded weapon impulse by 25%; the starter Road Grip Kit adds 0.6 grip. Solar Turbo, Storm Coil and Redrock Plating add visible 3D parts to the car. Select a Rocket or Shield Pack to prepare a starting item, then use E or the phone Item button during the race. Three upgrade parts buy one Engine, Tire or Brake level on the selected saved car, up to level 20. Credits, garage data, inventory and map records are stored locally; the new inventory uses the `rush-inventory-v1` save key.

The interface has three original illustrated map backgrounds, generated carbon panel textures, shaped buttons with hover/focus feedback and click sounds, custom equipment symbols, a Rush HUD, pause screen, loading screen and reward results. The actual races use runtime 3D scenery and custom ocean/sky shaders; the menu illustrations are separate artwork. Prompts and asset paths are in `ArtSource/Rush-art-prompts.md`.

- W / Up: accelerate. S / Down: brake, then reverse.
- A / D or Left / Right: steer.
- Space: drift. Drifting replenishes boost.
- Left Shift: use rechargeable boost.
- E: use the collected power-up.
- R: return an overturned or stuck car to the road.
- Escape: pause and resume.

Game Setup selects one of four circuits, 0–19 AI opponents, 1–5 laps, difficulty, and power-ups. Finish races for credits; each collected coin adds 100 race points and 10 finish credits. Coins become available again on your next lap. AI drivers use collected items and rechargeable boost, avoid water, overtake traffic, and aim for useful road tiles.

## Phone controls and startup

Overturned racers now auto-right only after one continuous second of physical support from the ground or an object. Airborne time does not count; leaving support resets the timer. The orientation correction happens while supported, without an automatic rotation continuing through the air. Weapon stun/rotation locks still delay recovery. Run `-aetherAirahMobileTest` to verify the contact delay, airborne behavior, directional multi-touch input and display frame-rate targets.

The new Aether Grounds logo appears above “by Aljay Leodones” and “Press anywhere to start”. Choose **Playing on a PC** or **Playing on a Phone** before the main menu. Controls can be changed from Controls later.

Phone mode uses four directional buttons: Left/Right steer, Up accelerates, and Down brakes/reverses. It also retains separate held buttons for Accelerate, Brake/Reverse, Drift, and Boost, plus a power-up button, Reset car, and Pause. Steering and several action buttons work simultaneously. The UI fits the landscape safe area. Drag the garage car to rotate and use two fingers to zoom. Android Back pauses/resumes the race or moves back through screens; it does not quit the application. Backgrounding the app pauses the race. Phone mode targets 60, 90 or 120 FPS according to the display refresh rate, with an optional 60 FPS cap in Controls, shorter shadows, reduced anti-aliasing and a capped render resolution.

Cars caught on an edge also have a five-second automatic recovery. The game checks lack of motion while trying to drive, or stationary unsupported wheels, and returns the car to an available road position. Moving clear cancels this timer. It ignores normal airborne movement and pauses with the race.

The 26 Malasugue tricycles follow the street circuit in both directions, turn toward a point ahead, and slow for traffic. Destroyed, overturned or displaced tricycles respawn after 20 active race seconds at a clear road position. A blocked spawn waits for room. Stalled traffic is detected after eight seconds and then enters that recovery timer.

## Garage

Save up to eight independent cars with **Add car**. Choose from ten base bodies: Vanta, Comet, Miso, Nomad, Spectre, Atlas, Pip, Cinder, Relay, and Manta. Comet is now a classic muscle coupe; Spectre is an open-wheel Formula car. The five new bodies are a pickup, bubble compact, hot rod, retro van, and enclosed prototype.

Drag the preview to rotate and tilt it, scroll to zoom, or use Front, Side, Rear, and Rotate. Each saved car retains its name, body, parts, paint, and upgrades. The garage migrates the previous single-car save and preserves credits.

- 32 colors for the body, trim, and wheels independently.
- Ten wheel geometries and 13 window tints.
- Ten modeled engines plus Standard: inline four, V6, V8 stacks, flat six, rotary, turbine, electric drive, twin turbo, supercharged V8, and V12.
- Ten additional wings, exhausts, and body graphic options in each category.
- Engine, tire, and brake upgrades each have 20 levels. The cost is 200 + 35 × current level credits. Each car has separate upgrades.

Visible garage engines and standard cosmetic parts change appearance. Performance comes from upgrades, equipped inventory gear and owned Store products. The Garage shows upgrades and Store bonuses next to the car.

## Circuits and effects

- **Downtown Minda:** Philippine city streets, markets, a raised flyover, coastal washouts and breakable scenery.
- **Malasugue Town:** a 1.87 km Davao neighborhood circuit through Maya-Maya, Bolcan, Cabaguio, Del Pilar, Tulingan and Malasugue. Includes 516 mapped building footprints, reconstructed houses where mapping is incomplete, the Leodones-Olivar Residence label, and 26 colliding tricycles. Roadside crates fracture, stalls break apart, and loose produce responds to impacts. The route has compact booster and jump tiles, lap coins and power-up pickups.
- **Midnight Gardens:** patterned stone roads, purple groves, stone arches, luminous mushrooms, lanterns, and drifting fireflies.

Malasugue Town uses actual road geometry and available building footprints, with estimated facades, most heights, and infill houses. It is a stylized reconstruction, not an exact replica. See `ArtSource/References/Malasugue/README.md` for the data, accuracy limits and source credits. Google Maps was inspected for reference; Google imagery is not distributed. Additional Kenney CC0 assets are retained in `ArtSource/ThirdParty/suburban`.

All race cars detect wrong-way heading and reverse travel. Five continuous seconds trigger the animated road return and orient the car in the correct race direction. Correcting direction cancels it; pausing freezes it. A separate five-second off-track countdown takes priority and measures the actual race corridor, including the flyover height. Side streets outside that corridor no longer count as valid racing surfaces. Recovery clears velocity and grants brief shield protection without awarding skipped race progress. Structure labels face outward and hide their mirrored backs. Surface-mounted turquoise chevrons pulse forward along every circuit; there are no text direction signs on Malasugue Town.

Power-ups use a custom framed crate that fractures into debris when collected. Homing missiles have a modeled body, nose, fins, nozzle, fire and smoke trails, swept collision detection, and area explosions. Unity Particle Systems provide fire, smoke, sparks, water spray, and pickup effects. Physical materials, soft shadows, reflection probes, studio lights, bloom, and subtle color grading replace the original stepped lighting.

## Source

- `Assets/Scripts/Vehicle.cs`: four-point spring/damper suspension, tire forces, braking, drift, AI steering, boost and collisions.
- `Assets/Scripts/TrackWorld.cs`: four large closed circuits, terrain, road meshes, barriers and Blender environment assets.
- `Assets/Scripts/Game.cs`: race lifecycle, positions, results, HUD, and pickups.
- `Assets/Scripts/GarageUI.cs`: interactive menus and saved multi-car garage.
- `Assets/Scripts/CarParts.cs`: modeled wheels, engines, wings, exhausts, graphics, and customization data.
- `Assets/Scripts/TrackFeatures.cs`: road tiles, ramps, water crossings, landmarks, and lap coins.
- `Assets/Scripts/RaceEffects.cs`: Unity particle effects, crate and missile models, and debris.
- `Assets/Scripts/Presentation.cs` and `Assets/Shaders`: physical materials, animated water, lighting, reflections, and post processing.
- `Tools/build_assets.py`: runs inside Blender and creates the five editable vehicles and environment library. Mesh parts retain semantic names and material roles. Exports in `Assets/Resources/Models` are generated from the Blender mesh geometry.
- `Assets/Editor/BuildGame.cs`: creates the scene and builds Windows and Android players.
- `Tools/build_overhaul_assets.py`: generates the redesigned Comet and Spectre plus five new bodies, retaining the original Vanta, Miso, and Nomad. The editable source is `ArtSource/OverhaulCars.blend`.
- `Assets/Art/Prefabs`: editable Unity prefabs for all ten car bodies and six environment pieces, with mesh and material assets alongside them. Use the **Aether Grounds** editor menu to reopen the game scene or regenerate these prefabs. Selected accessories are assembled at runtime.
- `Evidence`: build logs and real rendered screenshots.

## Rebuild

To regenerate all source models, run the installed Blender with `Tools/build_assets.py`, then `Tools/build_overhaul_assets.py`, in background mode. The second script must run last to retain the new Comet and Spectre. Then run Unity with this project and `-executeMethod BuildGame.Build -batchmode -quit`. For Android, install Unity Android Build Support with its SDK, NDK and OpenJDK, then use `-buildTarget Android -executeMethod BuildGame.BuildAndroid -batchmode -quit`. Opening the scene in Unity and pressing Play also runs the game. BuildGame verifies shader compilation as well as the player build result.

## Verification

Run the rendered Windows player with `-aetherRushTest -logFile <path>` for Rush verification. It checks all three maps, road collision after scenery batching, driving and traffic, map hazards, timeouts, pause and restart, equipment ownership and serialization, upgrade spending, duplicate reward protection, phone HUD rendering, forward-safe recovery, a full six-kilometre autonomous drive, and existing circuit generation. Screenshots and `test-result.txt` are saved under `Evidence/Rush`. Test mode does not persist garage, credit or inventory changes. `Evidence/Rush/before-rush.zip` is the source backup from before this update.

Run the executable with `-aetherOverhaulTest -logFile <path>` for catalog, garage serialization, physics interaction, and three-map 20-car checks. It captures real game frames under `Evidence/Overhaul`. Use a normal rendered player run for screenshots; Unity's `-batchmode` player cannot capture the screen. Test mode does not write the user's garage or credit save. `-aetherPhysicsTest` runs the isolated acceleration, braking, reverse, drift, collision, and power-up checks.

Driving uses a rigidbody and simulated suspension with arcade assistance. The game does not implement a full tire simulation, body deformation, multiplayer, controller mapping, or licensed audio. Water is a real-time rendered game surface with gameplay drag and particle spray; it is not a fluid simulation. Track scenery and geometry are generated deterministically when a race starts.

## Neighborhood verification

Run the rendered executable with `-aetherNeighborhoodTest -logFile <path>`. It tests map data, endpoints, road clearance, 20-car starts, tricycle movement and collision response, breakable crates, wrong-way timing/cancellation/pause, forward-facing returns, out-of-bounds recovery, AI race completion and first-map compatibility. Screenshots and results are saved in `Evidence/Malasugue`. Test mode does not write garage or credits.

`-aetherPlatformTest` verifies device selection, Back navigation, simultaneous phone controls, an actual unsupported chassis trap, and 20-second destroyed/displaced tricycle respawns. Results are written under `Evidence/Platform`.

## Premium additions

- Streetlight posts topple under collisions and explosions, lose their lights, and restore after ten active race seconds. Collision restoration waits if a racer is occupying the post's home.
- Game Setup and Play show circuit difficulty, traffic numbers, visibility and road hazards. Malasugue is marked very challenging because of its narrow village roads and night visibility.
- Rockets now hit racers within 16 metres, with substantially greater launch force and a larger scenery blast. Racing uses physical impacts and stun rather than an HP damage model.
- Pickups now choose between nine items. Five additions are Ice Blast (animated 1.5-second freeze and shatter), EMP (three-second boost interruption), Oil Slick (temporary grip loss), Gravity Mine (pull followed by an explosion), and Shockwave (wide launch wave). AI can use the additions. Shields block incoming hits/status effects; pauses stop their timers.
- Downtown Minda adds 20 moving jeepneys and 40 moving civilian vehicles to the circuit, with 80% traveling against the race direction. Imported civilian body meshes are aligned nose-first with their controller, rather than appearing to reverse. Traffic retains collision response and recovery. Afternoon sun has restrained, angle-dependent glare with obstruction checks. Generated asphalt and plaster textures use world-aligned mapping and anisotropic filtering.
- Malasugue Town has no directional sunlight, dark blue fog, streetlamp pools, car headlights, drifting mist and depth-aware ray-marched light volumes. Filmic tone mapping limits blown highlights when cars bunch together. Nearby light culling and cached vehicle renderer lists reduce unnecessary work.
- Store offers six special editions, priced from $0.99 to $9.99 USD, plus four equipment and effect kits. All include previews and performance bonuses. PayPal checkout is addressed to Aljay Leodones at lemonquake@gmail.com. Fulfillment uses manually verified payments and signed, player-bound unlock codes. No purchase was performed during development. See `Tools/Support-shop.md` for the owner workflow and account/settlement limitations.

The requested GPT Image 2.5 service refused generation because the connected account lacks the required plan. The textures use the built-in image generator instead; exact prompts and paths are recorded in `ArtSource/Premium-texture-prompts.md`.

Run a visible player with `-aetherPremiumTest -logFile <path>` to verify these systems and capture the Support menu, map details, Ice Blast, and all three circuits. Results are under `Evidence/Premium`. Do not start the player hidden when collecting screenshots; Windows can return black frames for hidden windows.
# iPhone build

The native iOS export is in `Build/iPhone/Aether-Grounds-Xcode`, with a transfer archive at `Build/iPhone/Aether-Grounds-Xcode.zip`. It includes the Airah Mountains update and phone controls. This is an Xcode project, not a signed IPA; compiling, signing, installing, and measuring performance on an iPhone still require a Mac and the owner's Apple Account.

On GitHub, the Xcode ZIP is committed as three files named `Aether-Grounds-Xcode.zip.part001` through `.part003` to stay within GitHub's per-file limit without Git LFS. From `Build/iPhone`, run `sh Reassemble-iPhone-Xcode.sh` on macOS or `powershell -ExecutionPolicy Bypass -File .\Reassemble-iPhone-Xcode.ps1` on Windows. The script verifies the reconstructed archive before use.

Read `Build/iPhone/Install-on-iPhone.md` for complete personal installation instructions. The existing export can be opened directly in Xcode without installing Unity on the Mac. For source changes, use Unity 6000.6.0f1 with iOS Build Support, switch to iOS in Build Profiles, and choose **Aether Grounds > Export iPhone Xcode Project**. The exporter enables Metal, landscape orientation, iOS 15 minimum, IL2CPP Release, and the iPhone high-refresh plist flag. Actual 60–120 FPS performance has not been verified on iPhone.

`Tools/package_iphone.py` packages the editable source and successful Xcode export with archive checksums. `Evidence/iPhone` contains the Unity export and preparation results.
