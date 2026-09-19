# Airah Mountains art

Both textures were made with the built-in image generation tool. The supplied mountain circuit image was used as visual inspiration for winding elevated roads, dense woodland and mountain terrain; it was not copied into the game.

## Granite and moss

Saved asset: `Assets/Resources/Airah/GraniteMoss.png`.

Prompt: Use case: photorealistic-natural. Asset type: seamless tileable game terrain albedo texture for Airah Mountains. Create a square full-bleed orthographic close-up surface of weathered mountain granite interspersed with alpine moss and tiny grass patches. About 65 percent gray warm slate granite with strata, hairline cracks and fine mineral grains, 35 percent muted olive green moss in crevices. Fine realistic detail, natural restrained color, diffuse flat illumination with no directional shadows, no perspective, no objects, no text, no border. Edges must tile seamlessly in both axes. Intended for repeating on enormous sunny mountain slopes and rocks in a Unity driving game, readable at a distance without oversized features.

## Meadow

Saved asset: `Assets/Resources/Airah/Meadow.png`.

Prompt: Use case: photorealistic-natural. Asset type: seamless tileable albedo texture for a Unity mountain meadow terrain. Square full bleed orthographic surface: dense short alpine meadow grass, tiny clover leaves and fine dry straw, small patches of olive moss and exposed brown earth. Natural muted yellow-green and olive palette, varied but evenly distributed coverage, realistic fine detail at ground level, no flowers or large stones. Perfectly flat diffuse lighting without directional shadows. No perspective, no horizon, no text, no objects, no border. Tile seamlessly on all four edges. Intended to repeat over vast sunny afternoon mountain slopes, so avoid identifiable prominent features.

## Blender source

`ArtSource/AirahMountains.blend` is an editable original asset collection. `Tools/build_airah_assets.py` generates and exports its game meshes to `Assets/Resources/Models/Airah*.json`. Assets include three pines, a broadleaf tree, four granite formations, a fern, fallen timber, a picnic shelter, and two civilian cars. The runtime builds the road and terrain around these reusable models.

Textures use mipmaps, repeating coordinates, anisotropic filtering and a 1024-pixel runtime import limit. Granite is mapped on three axes to avoid stretching on cliffs. Vegetation is instanced in 240-metre cells and culled by view and distance. Phone quality shortens vegetation range, uses reduced shadows, and targets 60–120 FPS according to the display, with an optional 60 FPS cap. PC targets 120 FPS. Actual frame rates depend on the device; desktop phone-quality tests are not measurements on a physical phone.
