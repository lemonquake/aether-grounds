# Cars stuck at the start: diagnosis and repair

21 September 2026. This addresses the reported regression in which the player and bots remained at 0 km/h after Go, including on Airah Mountains.

## Cause and change

The new WheelCollider integration modeled rolling resistance by applying a small service-brake torque continuously. Recorded telemetry showed throttle 1, approximately 2,210–2,701 Nm of motor torque at each wheel, four valid road contacts, no Rigidbody constraints, and zero wheel RPM. The wheels stayed locked despite engine torque.

`WheelPhysics.cs` now releases service brakes completely during normal acceleration. Rolling resistance is a separate opposing road force, bounded so it cannot reverse the car in a simulation step. It still depends on assembled mass and tire configuration. The change preserves finite engine torque, physical suspension, tire contact and weight-dependent acceleration.

The Drift control also applied constant rear braking and reproduced the same standstill. Drift now acts through the rear lateral tire-friction settings without a permanently applied brake. Countdown parking brakes, requested braking, reverse and pause behavior remain explicit states. Traffic suspension likewise releases its parking brakes during active simulation.

## Evidence

- Before repair: `Evidence/GameplayAudit/physics-regression.log` contains zero RPM with nonzero motor and brake torque, and four failed movement checks.
- Intermediate isolation: `brake-fix-test.log` restored acceleration, braking and reverse, while the independent Drift launch check still failed. This identified the second braking path.
- Repaired candidate: `physics-fixed.log` passed **12 of 12 checks**. Acceleration reached 119.6 km/h after four seconds; braking reduced speed to 46.8 km/h; reverse reached -13.7 m/s; holding Drift from standstill produced movement rather than a locked vehicle. Suspension contact, items, impact, recovery and wall collision checks also passed.
- Real circuit grids: `grid-launch.log` passed **48 of 48 checks**: player plus ten bot cars on each of Downtown Minda, Malasugue Town, Midnight Gardens and Airah Mountains, plus brake-release checks following pause/resume on each map. Each launch required observed speed over 15 km/h and displacement over four metres within the four-second sample. Every body type was represented among the bots. Tests used the normal countdown and engine/AI controls, without injecting forward velocity.
- Airah grid specifically: player peak 120.0 km/h, displacement 73.0 m; all ten bots moved 59.8–87.0 m during their samples.
- Playable build: `playable-build.log` completed successfully with zero errors, and the rebuilt `Build/Aether Grounds.exe` passed all 12 physics checks again in `playable-physics.log`. Its four-second acceleration was 119.6 km/h and Drift launch speed was 18.9 km/h.

`PhysicsSmokeTest.cs` now records motor torque, brake torque, wheel RPM, ground contact, mass and Rigidbody state for diagnosis. `VehicleLaunchTest.cs` provides the repeatable `-aetherLaunchTest` circuit-grid regression.

## Scope of this evidence

This proves launch recovery for the tested player configuration and all ten AI body types across the four circuit starts. It does not prove complete races, every upgrade combination, Rush or Stunts completion, balanced handling, civilian traffic steering, UI appearance, or mobile performance. The broader vehicle and gameplay overhaul remains active.

Candidate players were built under `.cache/VehicleCandidate*` so testing could proceed without replacing an executable the user might be running. `GameplayAuditBuild` now accepts `AETHER_BUILD_PATH` for that isolation. The normal playable location remains `Build/Aether Grounds.exe`.

## Next required work

1. Complete all remaining civilian wheel replacements and steering integration; verify noses, wheel contact and rolling motion in each traffic controller.
2. Check the new garage spring tension, rebound damping, chassis and part-weight views visually, including normal desktop sizes and phone layout.
3. Validate suspension settings and upgrades with measured drops, settling time, load distribution and all-body handling runs. Verify power/mass changes with controlled acceleration and maximum-speed comparisons.
4. Complete the full gameplay, Rush, Stunts and Airah regression suites. Update obsolete tests that still assume pre-overhaul fixed mass only where the new requirement supersedes them; preserve meaningful behavioral checks.
5. Finish the full gameplay audit and next-agent report. Do not describe this launch repair as completion of the whole overhaul.
