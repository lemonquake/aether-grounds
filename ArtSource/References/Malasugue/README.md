# Malasugue Town reference and accuracy

The route is a 1.87 km closed circuit in San Antonio, Davao, covering the two coordinates supplied by the user. X points east, Z points north; geometry is projected into local metres around 7.08915 N, 125.6262 E. `Tools/import_malasugue.py` deterministically rebuilds `Assets/Resources/Malasugue.json` from `neighborhood.osm`.

## What is grounded in map data

- 516 building footprints within the rendered neighborhood come from the retained OpenStreetMap extract, including polygon shapes and locations. The wider downloaded extract contains 641 footprints.
- Road connectivity, centerlines and public place labels come from OpenStreetMap. The circuit follows Maya-Maya, Bangus/Bangsi, Bolcan, Cabaguio, Del Pilar Extension, Tulingan and Malasugue. Street naming differs between providers: Google Maps describes the eastern connector as Holy Cross Drive.
- The residence label uses the user's approximate location and name: Leodones-Olivar Residence. The second supplied coordinate lies near Cabaguio / Del Pilar Extension.
- Source heights are retained when available. Most buildings do not have height or floor-count tags.

## What is reconstructed

This is a mapped, stylized neighborhood reconstruction, not a surveyed or photogrammetric clone. Roof colors, windows, gates, tanks, façade finishes and most heights are estimates. OSM omits many houses; additional homes fill unmapped roadside gaps, and their hierarchy names explicitly say `Reconstructed unmapped home`. Their precise placement is not verified. The gate at the user-supplied residence coordinate is an approximation, not a claim about the actual house appearance.

Road shoulders are widened to an 8.4 m racing surface where needed for the existing cars and passing. Race signs, pickups, jump/boost tiles, tricycle traffic and breakable market objects are gameplay additions. Named POIs reflect the map snapshot and may no longer be current businesses. No invented business names are presented as verified local landmarks.

## Visual reference inspected

- Google Maps coordinate search and driving directions between the supplied points, inspected 2026-09-10: https://www.google.com/maps/dir/7.0922515226973175,125.62697382377056/7.086026624224303,125.62414461720785/
- Google Maps satellite overview showed dense low-rise roofs, concrete neighborhood roads, narrow alleys and larger industrial/commercial buildings near Cabaguio.
- Street View resolved to `2203 Malasugue`, panorama at 7.0921717,125.6269755, but the panorama image stayed black in the available browser. No façade details were inferred from that black image.
- Google imagery is reference only and is not packaged as a texture or game asset.

## Data and asset credits

Map data © OpenStreetMap contributors, available under the Open Database License (ODbL) 1.0. The retained extract and derived map JSON are distributed under ODbL 1.0. https://www.openstreetmap.org/copyright and https://opendatacommons.org/licenses/odbl/1-0/

Existing Kenney Commercial, Nature, Food and Car assets are reused. Additional Kenney City Kit Suburban 2.0 assets (fence, low fence, planter, small tree and selected building models) were downloaded from https://kenney.nl/assets/city-kit-suburban. License: CC0; original archive, models and license retained under `ArtSource/ThirdParty/suburban`. Fences, planters and trees are used in the neighborhood. The imported house models are available for future editing; mapped houses are built from footprint meshes to preserve the actual outlines.

Tricycle geometry and procedural plaster, concrete slab and corrugated metal materials are authored in the project. No paid assets or downloaded executables are required.
